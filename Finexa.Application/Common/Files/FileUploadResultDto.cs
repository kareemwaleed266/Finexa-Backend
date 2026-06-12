namespace Finexa.Application.Common.Files
{
    public class FileUploadResultDto
    {
        public string Url { get; set; } = default!;

        public string PublicId { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public string ContentType { get; set; } = default!;

        public long SizeInBytes { get; set; }
    }
}