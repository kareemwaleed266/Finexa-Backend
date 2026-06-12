namespace Finexa.Application.Common.Files
{
    public interface IFileStorageService
    {
        Task<FileUploadResultDto> UploadImageAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            long sizeInBytes,
            string folder);

        Task DeleteAsync(string publicId);
    }
}