using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface ISubmissionMapper
{
    CommonResourceDto MapToCommonResourceDto(Submission submission);
}
