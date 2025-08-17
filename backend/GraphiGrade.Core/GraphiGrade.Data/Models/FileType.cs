namespace GraphiGrade.Data.Models;

/// <summary>
/// Defines the types of files that can be stored in the system
/// </summary>
public enum FileType : byte
{
    /// <summary>
    /// Exercise expected result image
    /// </summary>
    Image = 0,
    
    /// <summary>
    /// User submission source code
    /// </summary>
    SourceCode = 1,
    
    /// <summary>
    /// Judging result image
    /// </summary>
    ResultImage = 2
}