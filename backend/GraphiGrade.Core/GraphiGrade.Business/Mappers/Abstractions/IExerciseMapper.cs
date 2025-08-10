using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Exercise.Requests;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;
using GraphiGrade.Data.Models;

namespace GraphiGrade.Business.Mappers.Abstractions;

public interface IExerciseMapper
{
    CommonResourceDto MapToCommonResourceDto(Exercise exercise);

    GetExerciseResponse MapToGetExerciseResponse(Exercise exercise, string imageBlobUrl, CommonResourceDto createdByUser, IEnumerable<CommonResourceDto> submissions);

    CreateExerciseResponse MapToCreateExerciseResponse(Exercise exercise, string imageBlobUrl, CommonResourceDto createdByUser, IEnumerable<CommonResourceDto> submissions);

    Exercise MapFromCreateRequest(CreateExerciseRequest request, User author, FileMetadata fileMetadata);
}
