using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Contracts.DTOs.Group.Requests;
using GraphiGrade.Contracts.DTOs.Group.Responses;
using GraphiGrade.Contracts.DTOs.User.Responses;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IGroupService
{
    Task<ServiceResult<GetGroupResponse>> GetGroupByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<ServiceResult<CreateGroupResponse>> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken);
    
    Task<ServiceResult<GetAllGroupsResponse>> GetAllGroupsAsync(CancellationToken cancellationToken);

    Task<ServiceResult<UserAssignmentResponse>> AssignStudentToGroupAsync(int groupId, int studentId, CancellationToken cancellationToken);
}
