using GraphiGrade.Judge.Configuration;
using GraphiGrade.Judge.DTOs.Enums;
using GraphiGrade.Judge.Models;
using GraphiGrade.Judge.Repositories.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GraphiGrade.Judge.Repositories;

public class SubmissionsRepository : ISubmissionRepository
{
    private readonly IMongoCollection<Submission> _submissionsCollection;

    public SubmissionsRepository(IOptions<JudgeDatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

        _submissionsCollection = mongoDatabase.GetCollection<Submission>(databaseSettings.Value.CollectionName);
    }

    public async Task<Submission?> GetBySubmissionIdAsync(string submissionId, CancellationToken cancellationToken = default)
    {
        return await _submissionsCollection
            .Find(x => x.Id == submissionId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<Submission?> GetQueuedSubmissionAsync(CancellationToken cancellationToken = default)
    {
        return await _submissionsCollection
            .Find(x => x.Status == SubmissionStatus.Queued)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task AddSubmissionAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        await _submissionsCollection.InsertOneAsync(submission, cancellationToken: cancellationToken);
    }

    public async Task UpdateSubmissionAsync(string submissionId, Submission submission, CancellationToken cancellationToken = default)
    {
        await _submissionsCollection.ReplaceOneAsync(x => x.Id == submissionId, submission, cancellationToken: cancellationToken);
    }
}
