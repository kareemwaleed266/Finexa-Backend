using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Finexa.Application.Common.Files;
using Microsoft.Extensions.Options;

namespace Finexa.Integration.Files
{
    public class CloudinaryFileStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileStorageService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.CloudName) ||
                string.IsNullOrWhiteSpace(settings.ApiKey) ||
                string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary settings are missing");
            }

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);

            _cloudinary = new Cloudinary(account)
            {
                Api =
                {
                    Secure = true
                }
            };
        }

        public async Task<FileUploadResultDto> UploadImageAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            long sizeInBytes,
            string folder)
        {
            if (fileStream == null || sizeInBytes <= 0)
                throw new Exception("Invalid file");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return new FileUploadResultDto
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId,
                FileName = fileName,
                ContentType = contentType,
                SizeInBytes = sizeInBytes
            };
        }

        public async Task DeleteAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return;

            var deleteParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);
        }
    }
}