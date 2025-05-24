using GraphiGrade.Judge.Common;

namespace GraphiGrade.Judge.Services.OverlapStrategies.Abstractions;

public interface IJudgeOverlapStrategy
{
    double OverlapSolutionWithExpectedResult(ImageDecorator capturedSolution, ImageDecorator expectedResult, out ImageDecorator? overlappedResult);
}
