using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace LibraryManagementSystem.Services
{
    /// <summary>
    /// Uploads book cover images to a Firebase Storage bucket (which is backed by GCS).
    /// Scoped narrowly to what Book Management needs; report/receipt storage belongs
    /// to the Reports and Fine modules once those are integrated.
    /// </summary>
    public class FirebaseStorageService
    {
        private readonly StorageClient _client;
        private readonly string _bucketName;

        public FirebaseStorageService(string bucketName, string credentialsPath)
        {
            _bucketName = bucketName;
            var credential = GoogleCredential.FromFile(credentialsPath);
            _client = StorageClient.Create(credential);
        }

        public async Task<string> UploadCoverImageAsync(string localFilePath, string bookId)
        {
            var extension = Path.GetExtension(localFilePath);
            var objectName = $"book-covers/{bookId}{extension}";

            using var fileStream = File.OpenRead(localFilePath);
            await _client.UploadObjectAsync(_bucketName, objectName, null, fileStream);

            return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
        }
    }
}
