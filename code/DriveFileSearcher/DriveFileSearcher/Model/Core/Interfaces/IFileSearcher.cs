using System.Threading.Tasks;

namespace DriveFileSearcher.Model.Core.Interfaces
{
    public interface IFileSearcher
    {
        void ListDrives();
        void PauseScan();
        void CancelScan();
        Task StartOrResumeScan(string selectedDriveRotPath);
    }
}
