using GraphiGrade.Judge.Models;

namespace GraphiGrade.Judge.Messages;

public class ProcessedSubmissionMessage
{
    public Submission Submission { get; set; }

    public ProcessedSubmissionMessage(Submission submission)
    {
        Submission = submission;
    }
}
