namespace Contractors.Dtos
{
    public class FileDownloadResultDto
    {

        public MemoryStream FileContent { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }


    }
}
