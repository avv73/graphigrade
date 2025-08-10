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
    /// Retrieves base64 image from blob storage.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<string> RetrieveImageAsync(string url);

}
