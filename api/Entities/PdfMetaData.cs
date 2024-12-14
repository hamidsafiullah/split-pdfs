namespace api.Entities
{
    public class PdfMetaData
    {
        public int Id { get; set; }
        public string? OriginalFileName { get; set; }
        public string? SplitFileName { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
