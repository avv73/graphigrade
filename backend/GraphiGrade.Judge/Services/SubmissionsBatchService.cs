using GraphiGrade.Judge.DTOs;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Mappers.Abstractions;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Repositories.Abstractions;
using GraphiGrade.Judge.Services.Abstractions;
using GraphiGrade.Judge.Validators;
using GraphiGrade.Judge.Validators.Abstractions;

namespace GraphiGrade.Judge.Services;

public class SubmissionsBatchService : ISubmissionsBatchService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IJudgeSubmissionRequestValidator _judgeSubmissionRequestValidator;

    private readonly IJudgeSubmissionRequestMapper _judgeSubmissionRequestToSubmissionMapper;
    private readonly IJudgeBatchResponseMapper _judgeBatchResponseMapper;

    private readonly ILogger<SubmissionsBatchService> _logger;

    public SubmissionsBatchService(
        ISubmissionRepository submissionRepository,
        IJudgeSubmissionRequestValidator judgeSubmissionRequestValidator,
        IJudgeSubmissionRequestMapper judgeSubmissionRequestToSubmissionMapper,
        IJudgeBatchResponseMapper judgeBatchResponseMapper,
        ILogger<SubmissionsBatchService> logger)
    {
        _submissionRepository = submissionRepository;
        _judgeSubmissionRequestValidator = judgeSubmissionRequestValidator;
        _judgeSubmissionRequestToSubmissionMapper = judgeSubmissionRequestToSubmissionMapper;
        _judgeBatchResponseMapper = judgeBatchResponseMapper;
        _logger = logger;
    }

    public async Task<JudgeBatchResponse> BatchSubmissionAsync(JudgeSubmissionRequest request, CancellationToken cancellationToken)
    {
        ErrorResult validationResult = _judgeSubmissionRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            return _judgeBatchResponseMapper.Map(validationResult);
        }

        Submission submission = _judgeSubmissionRequestToSubmissionMapper.Map(request);

        try
        {
            await _submissionRepository.AddSubmissionAsync(submission, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Error while batching submission to database!");

            return _judgeBatchResponseMapper.Map(
                ErrorResultFactory.BuildError(SubmissionErrorCode.UnknownProcessingError,
                    "Internal error, please try again later"));
        }

        return _judgeBatchResponseMapper.Map(submission);
    }

    public async Task<JudgeBatchResponse> GetSubmissionAsync(string submissionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(submissionId))
        {
            return _judgeBatchResponseMapper.Map(
                ErrorResultFactory.BuildError(SubmissionErrorCode.InputValidationError,
                    "Expected submission id"));
        }

        if (!Guid.TryParse(submissionId, out _))
        {
            return _judgeBatchResponseMapper.Map(
                ErrorResultFactory.BuildError(SubmissionErrorCode.InputValidationError,
                    "Expected valid UUID as submission id"));
        }

        Submission? submission;
        try
        {
            submission = await _submissionRepository.GetBySubmissionIdAsync(submissionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Error while getting submission to database!");

            return _judgeBatchResponseMapper.Map(
                ErrorResultFactory.BuildError(SubmissionErrorCode.UnknownProcessingError,
                    "Internal error, please try again later"));
        }

        if (submission == null)
        {
            return _judgeBatchResponseMapper.Map(
                ErrorResultFactory.BuildError(SubmissionErrorCode.SubmissionNotFound,
                    "Submission was not found"));
        }

        return _judgeBatchResponseMapper.Map(submission);
    }
}
