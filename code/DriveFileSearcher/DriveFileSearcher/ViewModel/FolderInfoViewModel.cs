using DriveFileSearcher.Model.Publisher.Interfaces;

namespace DriveFileSearcher.VM
{
    public class FolderInfoViewModel: IFolderInfo
    {
        public string? Name { get; set; }
        public long TotalSize { get; set; }
        public int FileCount { get; set; }
    }
}
