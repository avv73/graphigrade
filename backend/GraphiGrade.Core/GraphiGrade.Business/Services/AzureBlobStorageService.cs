using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Business.Configurations;
using Microsoft.Extensions.Options;

namespace GraphiGrade.Business.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IOptions<GraphiGradeConfig> configOptions)
        {
            var config = configOptions.Value;
            var connectionString = config.AzureBlobStorageConnectionString;
            var containerName = config.AzureBlobStorageContainerName;
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                throw new InvalidOperationException("Azure Blob Storage configuration is missing.");

            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string?> StoreImageAsync(string imageBase64)
        {
            if (string.IsNullOrWhiteSpace(imageBase64)) return null;
            
            byte[] imageBytes = Convert.FromBase64String(imageBase64);
            string blobName = $"exercise-images/{Guid.NewGuid()}.png";
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            
            using (var stream = new MemoryStream(imageBytes))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "image/png" });
            }
            
            return blobClient.Uri.ToString();
        }

        public async Task<string?> StoreSourceCodeAsync(string sourceCodeBase64)
        {
            if (string.IsNullOrWhiteSpace(sourceCodeBase64)) return null;
            
            byte[] sourceCodeBytes = Convert.FromBase64String(sourceCodeBase64);
            string blobName = $"source-code/{Guid.NewGuid()}.c";
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            
            string contentType = "text/plain";
            
            using (var stream = new MemoryStream(sourceCodeBytes))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            }
            
            return blobClient.Uri.ToString();
        }
        
        public async Task<string> RetrieveContentAsync(string url)
        {
            BlobClient blobClient = new BlobClient(new Uri(url));
            var response = await blobClient.DownloadContentAsync();
            byte[] contentBytes = response.Value.Content.ToArray();
            return Convert.ToBase64String(contentBytes);
        }
    }
}
