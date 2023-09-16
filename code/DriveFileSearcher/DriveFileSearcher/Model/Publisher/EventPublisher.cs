using DriveFileSearcher.Model.Publisher.Dto;
using DriveFileSearcher.Model.Publisher.Interfaces;
using System;

namespace DriveFileSearcher.Model.Publisher
{
    public interface IEventPublisher
    {
        event EventHandler<IFolderInfo>? FolderFound;
        event EventHandler<IDriveInfo>? DriveFound;
        event EventHandler? ScanCompleted;

        void SendScanCompleted();
        void PublishFolderInfo(IFolderInfo folderInfo);
        void PublishDriveInfo(string name);
    }

    public class EventPublisher: IEventPublisher
    {
        public event EventHandler<IFolderInfo>? FolderFound;
        public event EventHandler<IDriveInfo>? DriveFound;
        public event EventHandler? ScanCompleted;

        public void SendScanCompleted()
        {
            OnScanCompleted();
        }

        public void PublishFolderInfo(IFolderInfo folderInfo)
        {
            OnFolderFound(folderInfo);
        }

        public void PublishDriveInfo(string name)
        {
            var driveInfo = new DriveInfo
            {
                Name = name
            };

            OnDriveFound(driveInfo);
        }

        protected virtual void OnFolderFound(IFolderInfo folderInfo)
        {
            FolderFound?.Invoke(this, folderInfo);
        }

        protected virtual void OnDriveFound(IDriveInfo driveInfo)
        {
            DriveFound?.Invoke(this, driveInfo);
        }

        protected virtual void OnScanCompleted()
        {
            ScanCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
