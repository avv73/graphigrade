using GraphiGrade.Business.Configurations;
using GraphiGrade.Business.ServiceModels;
using GraphiGrade.Business.ServiceModels.Factories;
using GraphiGrade.Business.ServiceModels.Judge;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Contracts.DTOs.Submission.Requests;
using GraphiGrade.Contracts.DTOs.Submission.Responses;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Business.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJudgeService _judgeService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IUserResolverService _userResolverService;
    private readonly ISubmissionMapper _submissionMapper;
    private readonly GraphiGradeConfig _config;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(
        IUnitOfWork unitOfWork,
        IJudgeService judgeService,
        IBlobStorageService blobStorageService,
        IUserResolverService userResolverService,
        ISubmissionMapper submissionMapper,
        IOptions<GraphiGradeConfig> config,
        ILogger<SubmissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _judgeService = judgeService;
        _blobStorageService = blobStorageService;
        _userResolverService = userResolverService;
        _submissionMapper = submissionMapper;
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
            exercise = await _unitOfWork.Exercises.GetByIdWithIncludesAsync(exerciseId, 
                query => 
                    query.Include(ex => ex.ExpectedImage));
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
            SubmittedAt = DateTime.UtcNow,
            LastUpdate = judgeResponse.Timestamp,
            SourceCode = sourceCodeFileMetadata
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

    public async Task<ServiceResult<GetSubmissionStatusResponse>> GetSubmissionStatusAsync(int submissionId, CancellationToken cancellationToken)
    {
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
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (user == null)
        {
            _logger.LogError("Unable to find user for claim: {Username}", username);
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(HttpStatusCode.Forbidden);
        }

        // Get submission from database
        Submission? submission;
        try
        {
            submission = await _unitOfWork.Submissions.GetByIdAsync(submissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when retrieving submission");
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }

        if (submission == null)
        {
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(HttpStatusCode.NotFound);
        }

        // Check if user owns this submission
        if (submission.UserId != user.Id && !user.IsTeacher)
        {
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(HttpStatusCode.Forbidden);
        }

        // Get status from judge service
        JudgeBatchResponse? judgeResponse;
        try
        {
            judgeResponse = await _judgeService.GetSubmissionStatusAsync(submission.JudgeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving submission status from judge service");
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when retrieving submission status, please try again later");
        }

        if (judgeResponse == null)
        {
            return ServiceResultFactory<GetSubmissionStatusResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Judge service is currently unavailable, please try again later");
        }

        // Update submission status if changed
        if (submission.Status != (byte)judgeResponse.Status || submission.ErrorCode != (byte)judgeResponse.ErrorCode)
        {
            submission.Status = (byte)judgeResponse.Status;
            submission.ErrorCode = (byte)judgeResponse.ErrorCode;
            submission.LastUpdate = judgeResponse.Timestamp;

            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update submission status in database");
                // Continue processing even if database update fails
            }
        }

        // Map to response
        var response = _submissionMapper.MapToGetSubmissionStatusResponse(submission, judgeResponse);

        return ServiceResultFactory<GetSubmissionStatusResponse>.CreateResult(response);
    }

    public async Task<ServiceResult<GetUserSubmissionsResponse>> GetUserSubmissionsAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetFirstByFilterAsync(u => u.Username == username);
            if (user == null)
            {
                return ServiceResultFactory<GetUserSubmissionsResponse>.CreateError(HttpStatusCode.NotFound, "User not found");
            }

            var submissions = (await _unitOfWork.Submissions.GetAllByFilterAsync(s => s.UserId == user.Id))
                .OrderByDescending(s => s.SubmittedAt)
                .ToList();

            bool changed = false;
            foreach (var s in submissions)
            {
                try
                {
                    var judge = await _judgeService.GetSubmissionStatusAsync(s.JudgeId, cancellationToken);
                    if (judge == null)
                    {
                        continue; // skip if judge service unavailable for this item
                    }

                    byte newStatus = (byte)judge.Status;
                    byte newError = (byte)judge.ErrorCode;
                    DateTime newTimestamp = judge.Timestamp;

                    if (s.Status != newStatus)
                    {
                        s.Status = newStatus;
                        changed = true;
                    }

                    if (s.ErrorCode != newError)
                    {
                        s.ErrorCode = newError;
                        changed = true;
                    }

                    if (s.LastUpdate != newTimestamp)
                    {
                        s.LastUpdate = newTimestamp;
                        changed = true;
                    }

                    if (judge.SubmissionResult != null)
                    {
                        decimal newScore = Convert.ToDecimal(judge.SubmissionResult.ExecutionAccuracy);
                        decimal roundedNewScore = Math.Round(newScore, 5, MidpointRounding.AwayFromZero);
                        if (!s.Score.HasValue || Math.Round(s.Score.Value, 5, MidpointRounding.AwayFromZero) != roundedNewScore)
                        {
                            s.Score = roundedNewScore;
                            changed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync submission {SubmissionId} with judge service", s.JudgeId);
                }
            }

            if (changed)
            {
                try
                {
                    await _unitOfWork.SaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to persist submission updates during user submissions sync");
                }
            }

            var dtos = submissions
                .Select(s => new GetUserSubmissionDto
                {
                    Id = s.Id,
                    SubmittedAt = s.SubmittedAt,
                    Score = s.Score,
                    Status = (Contracts.DTOs.Submission.Responses.SubmissionStatus)s.Status
                })
                .ToList();

            return ServiceResultFactory<GetUserSubmissionsResponse>.CreateResult(new GetUserSubmissionsResponse
            {
                Submissions = dtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when retrieving submissions for user {Username}", username);
            return ServiceResultFactory<GetUserSubmissionsResponse>.CreateError(
                HttpStatusCode.InternalServerError,
                "Unexpected error when fetching data, please try again later");
        }
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