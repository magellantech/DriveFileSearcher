using DriveFileSearcher.Model.Publisher.Interfaces;

namespace DriveFileSearcher.Model.Publisher.Dto
{
    public class FolderInfo: IFolderInfo
    {
        public string? Name { get; set; }
        public long TotalSize { get; set; }
        public int FileCount { get; set; }
        public bool isBigFilePresent { get; set; }
    }
}
