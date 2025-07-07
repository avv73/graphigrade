using GraphiGrade.Business.Extensions;
using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using GraphiGrade.Contracts.DTOs.Common;

namespace GraphiGrade.Business.Services;

public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExerciseMapper _exerciseMapper;
    private readonly IUserMapper _userMapper;
    private readonly ISubmissionMapper _submissionMapper;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(IUnitOfWork unitOfWork, IExerciseMapper exerciseMapper, IUserMapper userMapper, ISubmissionMapper submissionMapper, ILogger<ExerciseService> logger)
    {
        _unitOfWork = unitOfWork;
        _exerciseMapper = exerciseMapper;
        _userMapper = userMapper;
        _submissionMapper = submissionMapper;
        _logger = logger;
    }

    public async Task<ServiceResult<GetExerciseResponse>> GetExerciseByIdAsync(int id, ClaimsPrincipal userClaimsPrincipal, CancellationToken cancellationToken)
    {
        Exercise? matchedExercise;

        try
        {
            matchedExercise = await _unitOfWork.Exercises.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when checking existing exercises!");

            return ServiceResultFactory<GetExerciseResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (matchedExercise == null ||
            (!matchedExercise.IsVisible && !userClaimsPrincipal.IsAdmin()))
            // Do not show invisible exercises to non-admins.
        {
            return ServiceResultFactory<GetExerciseResponse>.CreateError(HttpStatusCode.NotFound);
        }

        string imageBlobUrl = matchedExercise.ExpectedImage.StorageUrl;
        CommonResourceDto createdByUser = _userMapper.MapToCommonResourceDto(matchedExercise.CreatedBy);
        IEnumerable<CommonResourceDto> submissions = matchedExercise.Submissions.Select(_submissionMapper.MapToCommonResourceDto);

        return ServiceResultFactory<GetExerciseResponse>.CreateResult(_exerciseMapper.MapToGetExerciseResponse(matchedExercise, imageBlobUrl, createdByUser, submissions));
    }
}
