using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Group.Requests;
using GraphiGrade.Contracts.DTOs.Group.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace GraphiGrade.Business.Services;

public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGroupMapper _groupMapper;
    private readonly IUserMapper _userMapper;
    private readonly IExerciseMapper _exerciseMapper;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IUnitOfWork unitOfWork, IGroupMapper groupMapper, IUserMapper userMapper, IExerciseMapper exerciseMapper, ILogger<GroupService> logger)
    {
        _unitOfWork = unitOfWork;
        _groupMapper = groupMapper;
        _userMapper = userMapper;
        _exerciseMapper = exerciseMapper;
        _logger = logger;
    }
    
    public async Task<ServiceResult<GetGroupResponse>> GetGroupByIdAsync(int id, CancellationToken cancellationToken)
    {
        Group? matchedGroup;

        try
        {
            matchedGroup = await _unitOfWork.Groups.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when checking existing groups!");

            return ServiceResultFactory<GetGroupResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (matchedGroup == null)
        {
            return ServiceResultFactory<GetGroupResponse>.CreateError(HttpStatusCode.NotFound);
        }

        List<CommonResourceDto> groupUserDtoList = matchedGroup.UsersGroups?
            .Select(groupUser => _userMapper.MapToCommonResourceDto(groupUser.User))
            .ToList() ?? new();

        List<CommonResourceDto> groupExercisesDtoList = matchedGroup.ExercisesGroups?
            .Select(groupExercises => _exerciseMapper.MapToCommonResourceDto(groupExercises.Exercise))
            .ToList() ?? new();

        return ServiceResultFactory<GetGroupResponse>.CreateResult(_groupMapper.MapToGetGroupResponse(matchedGroup, groupUserDtoList, groupExercisesDtoList));
    }

    public async Task<ServiceResult<CreateGroupResponse>> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if group with the same name already exists
            bool groupExists = await _unitOfWork.Groups.ExistsAsync(g => g.GroupName == request.GroupName);
            
            if (groupExists)
            {
                return ServiceResultFactory<CreateGroupResponse>.CreateError(
                    HttpStatusCode.BadRequest,
                    "A group with this name already exists");
            }

            // Create new group
            var newGroup = new Group
            {
                GroupName = request.GroupName
            };

            await _unitOfWork.Groups.AddAsync(newGroup);
            await _unitOfWork.SaveAsync();

            // Handle user assignments if provided
            List<CommonResourceDto> assignedUsersDtos = new();
            List<string> assignmentErrors = new();
            
            if (request.UserIds != null && request.UserIds.Any())
            {
                var assignmentResult = await ValidateAndAssignUsersToGroup(newGroup.Id, request.UserIds, cancellationToken);
                
                if (assignmentResult.IsError)
                {
                    _logger.LogWarning("User assignment errors occurred during group creation: {Error}", assignmentResult.Error!.ErrorMessage);
                    // Continue with group creation but note the assignment errors
                }
                else
                {
                    assignedUsersDtos = assignmentResult.Result!.AssignedUsers.ToList();
                    // Log any assignment warnings
                    if (assignmentResult.Result.Errors.Any())
                    {
                        _logger.LogWarning("Some user assignments failed: {Errors}", string.Join("; ", assignmentResult.Result.Errors));
                    }
                }
            }

            // Note: We still return success even if some user assignments failed
            // The response will include which users were successfully assigned
            return ServiceResultFactory<CreateGroupResponse>.CreateResult(
                _groupMapper.MapToCreateGroupResponse(newGroup, assignedUsersDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when creating group!");

            return ServiceResultFactory<CreateGroupResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when creating group, please try again later");
        }
    }

    private async Task<ServiceResult<UserAssignmentResponse>> ValidateAndAssignUsersToGroup(
        int groupId, 
        IEnumerable<int> userIds, 
        CancellationToken cancellationToken)
    {
        var assignedUsers = new List<CommonResourceDto>();
        var invalidUserMessages = new List<string>();

        foreach (int userId in userIds.Distinct()) // Remove duplicates
        {
            try
            {
                // Check if user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    invalidUserMessages.Add($"User with ID {userId} does not exist");
                    continue;
                }

                // Check if user is teacher/admin - they cannot be assigned to groups
                if (user.IsTeacher)
                {
                    invalidUserMessages.Add($"User '{user.Username}' is a teacher and cannot be assigned to groups");
                    continue;
                }

                // Check if user is already in this group
                bool userAlreadyInGroup = await _unitOfWork.UsersGroups.ExistsAsync(ug => 
                    ug.UserId == userId && ug.GroupId == groupId);

                if (userAlreadyInGroup)
                {
                    invalidUserMessages.Add($"User '{user.Username}' is already assigned to this group");
                    continue;
                }

                // Create the user-group relationship
                var userGroup = new UsersGroups
                {
                    UserId = userId,
                    GroupId = groupId
                };

                await _unitOfWork.UsersGroups.AddAsync(userGroup);
                assignedUsers.Add(_userMapper.MapToCommonResourceDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user {UserId} for group {GroupId}", userId, groupId);
                invalidUserMessages.Add($"Error processing user with ID {userId}");
            }
        }

        // Save all user-group relationships
        if (assignedUsers.Any())
        {
            await _unitOfWork.SaveAsync();
        }

        // Return the result with both successful assignments and errors
        var result = new UserAssignmentResponse
        {
            AssignedUsers = assignedUsers,
            Errors = invalidUserMessages
        };

        return ServiceResultFactory<UserAssignmentResponse>.CreateResult(result);
    }
}
