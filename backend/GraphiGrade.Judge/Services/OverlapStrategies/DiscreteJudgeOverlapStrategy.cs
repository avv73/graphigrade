using System.Drawing;
using GraphiGrade.Judge.Common;
using GraphiGrade.Judge.Services.OverlapStrategies.Abstractions;

namespace GraphiGrade.Judge.Services.OverlapStrategies;

public class DiscreteJudgeOverlapStrategy : IJudgeOverlapStrategy
{
    public double OverlapSolutionWithExpectedResult(ImageDecorator capturedSolution, ImageDecorator expectedResult, out ImageDecorator? overlappedResult)
    {
        if (capturedSolution.Height != expectedResult.Height ||
            capturedSolution.Width != expectedResult.Width)
        { 
            overlappedResult = null;

            return 0;
        };

        int totalPixels = capturedSolution.Height * expectedResult.Width;
        int mismatchedPixels = 0;

        ImageDecorator resultOfComparison = new ImageDecorator(capturedSolution.Width, expectedResult.Height);

        for (int x = 0; x < expectedResult.Width; x++)
        {
            for (int y = 0; y < expectedResult.Height; y++)
            {
                if (capturedSolution.GetPixel(x, y) != expectedResult.GetPixel(x, y))
                {
                    mismatchedPixels++;
                    resultOfComparison.SetPixel(x, y, Color.White);
                }
            }
        }

        double score = 1 - ((double)mismatchedPixels / totalPixels);
        overlappedResult = resultOfComparison;

        return score;
    }
}
