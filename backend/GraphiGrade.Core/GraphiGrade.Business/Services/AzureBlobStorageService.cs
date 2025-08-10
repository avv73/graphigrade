using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Business.Configurations;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

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

        public async Task<string> RetrieveImageAsync(string url)
        {
            BlobClient blobClient = new BlobClient(new Uri(url));
            var response = await blobClient.DownloadContentAsync();
            byte[] imageBytes = response.Value.Content.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }
}
