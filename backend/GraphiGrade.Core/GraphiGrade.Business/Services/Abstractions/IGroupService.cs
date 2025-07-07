using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Contracts.DTOs.User.Responses;

namespace GraphiGrade.Business.Services.Abstractions;

public interface IGroupService
{
    Task<ServiceResult<GetGroupResponse>> GetGroupByIdAsync(int id, CancellationToken cancellationToken);
}
