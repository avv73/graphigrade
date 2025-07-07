using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
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
}
