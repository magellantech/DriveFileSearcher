
namespace DriveFileSearcher.Model.Publisher.Interfaces
{
    public interface IFolderInfo
    {
        public string? Name { get; set; }
        public long TotalSize { get; set; }
        public int FileCount { get; set; }
    }
}
