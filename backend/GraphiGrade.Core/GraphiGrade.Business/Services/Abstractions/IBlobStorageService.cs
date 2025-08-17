namespace GraphiGrade.Business.Services.Abstractions;

public interface IBlobStorageService
{
    /// <summary>
    /// Stores image in a blob storage. Returns url of the image.
    /// </summary>
    /// <param name="imageBase64"></param>
    /// <returns></returns>
    Task<string?> StoreImageAsync(string imageBase64);

    /// <summary>
    /// Stores source code in a blob storage. Returns url of the source code file.
    /// </summary>
    /// <param name="sourceCodeBase64">Base64 encoded source code content</param>
    /// <returns>URL of the stored source code file</returns>
    Task<string?> StoreSourceCodeAsync(string sourceCodeBase64);

    /// <summary>
    /// Retrieves base64 content from blob storage (works for any file type).
    /// </summary>
    /// <param name="url">The blob storage URL</param>
    /// <returns>Base64 encoded content</returns>
    Task<string> RetrieveContentAsync(string url);
}
