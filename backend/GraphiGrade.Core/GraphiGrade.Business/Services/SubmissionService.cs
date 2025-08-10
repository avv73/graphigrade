using GraphiGrade.Business.Configurations;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.ServiceModels.Judge;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Contracts.DTOs.Submission.Requests;
using GraphiGrade.Contracts.DTOs.Submission.Responses;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace GraphiGrade.Business.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJudgeService _judgeService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IUserResolverService _userResolverService;
    private readonly GraphiGradeConfig _config;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(
        IUnitOfWork unitOfWork,
        IJudgeService judgeService,
        IBlobStorageService blobStorageService,
        IUserResolverService userResolverService,
        IOptions<GraphiGradeConfig> config,
        ILogger<SubmissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _judgeService = judgeService;
        _blobStorageService = blobStorageService;
        _userResolverService = userResolverService;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<ServiceResult<SubmitSolutionResponse>> SubmitSolutionAsync(
        int exerciseId,
        SubmitSolutionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SourceCodeBase64.Length >= _config.MaximumBase64CharsInResultPattern)
        {
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.BadRequest,
                "Source code size is too big!");
        }

        string username = _userResolverService.Username;

        // Get user
        User? user;
        try
        {
            user = await _unitOfWork.Users.GetFirstByFilterAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when retrieving user");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (user == null)
        {
            _logger.LogError("Unable to find user for claim: {Username}", username);
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(HttpStatusCode.Forbidden);
        }

        // Get exercise
        Exercise? exercise;
        try
        {
            exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when retrieving exercise");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (exercise == null)
        {
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(HttpStatusCode.NotFound);
        }

        // Store source code in blob storage
        string? sourceCodeBlobUrl;
        try
        {
            sourceCodeBlobUrl = await _blobStorageService.StoreSourceCodeAsync(request.SourceCodeBase64);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while storing source code blob");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when uploading data, please try again later");
        }

        if (string.IsNullOrWhiteSpace(sourceCodeBlobUrl))
        {
            _logger.LogError("Empty source code blob!");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(HttpStatusCode.InternalServerError);
        }

        // Create file metadata for source code
        int fileSize = (int)Math.Ceiling(request.SourceCodeBase64.Length / 4.0) * 3;
        var sourceCodeFileMetadata = new FileMetadata
        {
            Size = fileSize,
            Type = (byte)FileType.SourceCode,
            StorageUrl = sourceCodeBlobUrl
        };

        await _unitOfWork.FilesMetadata.AddAsync(sourceCodeFileMetadata);

        // Get expected pattern from exercise
        string expectedPatternBase64;
        try
        {
            // Convert the blob URL to base64 - in real implementation you might need to download from blob storage
            // For now, assuming we can access the base64 directly or store it differently
            expectedPatternBase64 = await GetBase64FromBlobUrl(exercise.ExpectedImage.StorageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expected pattern");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when retrieving exercise data, please try again later");
        }

        // Submit to judge service
        var judgeRequest = new JudgeBatchRequest
        {
            SourceCodeBase64 = request.SourceCodeBase64,
            ExpectedPatternBase64 = expectedPatternBase64,
            Timestamp = DateTime.UtcNow
        };

        JudgeBatchResponse? judgeResponse;
        try
        {
            judgeResponse = await _judgeService.SubmitForJudgingAsync(judgeRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting to judge service");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when submitting for judging, please try again later");
        }

        if (judgeResponse == null)
        {
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Judge service is currently unavailable, please try again later");
        }

        // Create submission record
        var submission = new Submission
        {
            ExerciseId = exerciseId,
            UserId = user.Id,
            JudgeId = judgeResponse.SubmissionId,
            Status = (byte)judgeResponse.Status,
            ErrorCode = (byte)judgeResponse.ErrorCode,
            SourceCodeId = sourceCodeFileMetadata.Id,
            SubmittedAt = DateTime.UtcNow,
            LastUpdate = judgeResponse.Timestamp
        };

        await _unitOfWork.Submissions.AddAsync(submission);

        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to commit changes to DB!");
            return ServiceResultFactory<SubmitSolutionResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when saving submission, please try again later");
        }

        var response = new SubmitSolutionResponse
        {
            SubmissionId = judgeResponse.SubmissionId,
            Status = (Contracts.DTOs.Submission.Responses.SubmissionStatus)judgeResponse.Status,
            SubmittedAt = submission.SubmittedAt
        };

        return ServiceResultFactory<SubmitSolutionResponse>.CreateResult(response);
    }

    private async Task<string> GetBase64FromBlobUrl(string blobUrl)
    {
        try
        {
            return await _blobStorageService.RetrieveContentAsync(blobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving content from blob storage for URL: {BlobUrl}", blobUrl);
            throw;
        }
    }
}