using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Messages;

public class UnprocessedSubmissionMessage
{
    public Submission Submission { get; }

    public UnprocessedSubmissionMessage(Submission submission)
    {
        Submission = submission;
    }
}
