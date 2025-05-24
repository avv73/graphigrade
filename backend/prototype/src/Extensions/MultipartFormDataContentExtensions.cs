namespace GraphiGrade.Extensions;

public static class MultipartFormDataContentExtensions
{
    public static async Task<Dictionary<string, string>> ToDictionaryAsync(this MultipartFormDataContent content)
    {
        var result = new Dictionary<string, string>();

        foreach (var part in content)
        {
            var key = part.Headers.ContentDisposition?.Name?.Trim('\"');
            var value = await part.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(key))
            {
                result[key] = value;
            }
        }

        return result;
    }
}