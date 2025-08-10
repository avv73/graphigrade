using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs.Common;
using GraphiGrade.Contracts.DTOs.Exercise.Requests;
using GraphiGrade.Contracts.DTOs.Exercise.Responses;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using System.Net;
using GraphiGrade.Business.Configurations;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Business.Services;

public class ExerciseService : IExerciseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExerciseMapper _exerciseMapper;
    private readonly IUserMapper _userMapper;
    private readonly ISubmissionMapper _submissionMapper;
    private readonly IUserResolverService _userResolverService;
    private readonly GraphiGradeConfig _config;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(
        IUnitOfWork unitOfWork,
        IExerciseMapper exerciseMapper,
        IUserMapper userMapper,
        ISubmissionMapper submissionMapper,
        IUserResolverService userResolverService,
        IOptions<GraphiGradeConfig> config,
        IBlobStorageService blobStorageService,
        ILogger<ExerciseService> logger)
    {
        _unitOfWork = unitOfWork;
        _exerciseMapper = exerciseMapper;
        _userMapper = userMapper;
        _submissionMapper = submissionMapper;
        _userResolverService = userResolverService;
        _config = config.Value;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<ServiceResult<GetExerciseResponse>> GetExerciseByIdAsync(int id, CancellationToken cancellationToken)
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
            (!matchedExercise.IsVisible && !_userResolverService.IsAdmin))
        // Do not show invisible exercises to non-admins.
        {
            return ServiceResultFactory<GetExerciseResponse>.CreateError(HttpStatusCode.NotFound);
        }

        string imageBlobUrl = matchedExercise.ExpectedImage.StorageUrl;
        CommonResourceDto createdByUser = _userMapper.MapToCommonResourceDto(matchedExercise.CreatedBy);
        IEnumerable<CommonResourceDto> submissions = matchedExercise.Submissions.Select(_submissionMapper.MapToCommonResourceDto);

        return ServiceResultFactory<GetExerciseResponse>.CreateResult(
            _exerciseMapper.MapToGetExerciseResponse(matchedExercise, imageBlobUrl, createdByUser, submissions));
    }

    public async Task<ServiceResult<CreateExerciseResponse>> CreateExerciseAsync(
        CreateExerciseRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ExpectedImageBase64.Length >= _config.MaximumBase64CharsInResultPattern)
        {
            return ServiceResultFactory<CreateExerciseResponse>.CreateError(
                HttpStatusCode.BadRequest,
                "Image size is too big!");
        }

        string username = _userResolverService.Username;

        User? authorUser;

        try
        {
            authorUser = await _unitOfWork.Users.GetFirstByFilterAsync(s => s.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error when retrieving existing user");

            return ServiceResultFactory<CreateExerciseResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (authorUser == null)
        {
            _logger.LogError($"Unable to find user for claim: {username}");

            return ServiceResultFactory<CreateExerciseResponse>.CreateError(HttpStatusCode.Forbidden);
        }

        string? blobUrl;

        try
        {
            blobUrl = await _blobStorageService.StoreImageAsync(request.ExpectedImageBase64);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Error while storing image blob");

            return ServiceResultFactory<CreateExerciseResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when uploading data, please try again later");
        }

        if (string.IsNullOrWhiteSpace(blobUrl))
        {
            _logger.LogError("Empty blob!");

            return ServiceResultFactory<CreateExerciseResponse>.CreateError(HttpStatusCode.InternalServerError);
        }

        int fileSize = (int)Math.Ceiling(request.ExpectedImageBase64.Length / 4.0) * 3;

        FileMetadata fileMetadata = new FileMetadata
        {
            Size = fileSize,
            Type = (byte)FileType.Image,
            StorageUrl = blobUrl
        };

        await _unitOfWork.FilesMetadata.AddAsync(fileMetadata);

        Exercise exercise = _exerciseMapper.MapFromCreateRequest(request, authorUser, fileMetadata);

        await _unitOfWork.Exercises.AddAsync(exercise);

        List<ExercisesGroups> exercisesGroups = new List<ExercisesGroups>();

        foreach (int requestGroupId in request.GroupIds)
        {
            try
            {
                Group? groupToAdd = await _unitOfWork.Groups.GetByIdAsync(requestGroupId);

                if (groupToAdd == null)
                {
                    _logger.LogWarning($"Received unknown group for request: {requestGroupId}");
                    continue;
                }

                exercisesGroups.Add(new ExercisesGroups
                {
                    Exercise = exercise,
                    Group = groupToAdd
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(exception: ex, "Error while retrieving groups");
            }
        }

        exercise.ExercisesGroups = exercisesGroups;

        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, "Unable to commit changes to DB!");


            return ServiceResultFactory<CreateExerciseResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when uploading data, please try again later");
        }


        CommonResourceDto createdByUser = _userMapper.MapToCommonResourceDto(authorUser);
        IEnumerable<CommonResourceDto> submissions = [];

        return ServiceResultFactory<CreateExerciseResponse>.CreateResult(
            _exerciseMapper.MapToCreateExerciseResponse(exercise, blobUrl, createdByUser, submissions));
    }
}
