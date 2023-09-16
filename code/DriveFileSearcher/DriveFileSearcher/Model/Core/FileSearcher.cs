using DriveFileSearcher.Model.Core.Interfaces;
using DriveFileSearcher.Model.Publisher.Dto;
using DriveInfo = System.IO.DriveInfo;
using DriveFileSearcher.Model.Publisher;
using System.Threading.Tasks;
using Path = System.IO.Path;
using System.Threading;
using System.IO;
using System;
using NLog;


namespace DriveFileSearcher.Model.Core
{
    public class FileSearcher : IFileSearcher
    {
        private bool _isPaused = false;
        private CancellationTokenSource _сancellationTokenSource;
        private readonly ILogger _logger;
        private const long TargetFileSize = 10 * 1024 * 1024;//10M
        private readonly IEventPublisher _publisher;


        public FileSearcher(ILogger logger, IEventPublisher publisher)
        {
            _logger = logger;
            _publisher = publisher;
            _сancellationTokenSource = new CancellationTokenSource();
        }

        public bool IsPaused
        {
            get { return _isPaused; }
        }

        public void CancelScan()
        {
            _сancellationTokenSource.Cancel();
            _isPaused = false;
        }

        public void PauseScan()
        {
            _isPaused = true;
        }

        public async Task StartOrResumeScan(string selectedDriveRotPath)
        {
            if (_isPaused)
            {
                _isPaused = false;
            }
            else
            {
                await Task.Run(async () =>
                {
                    _сancellationTokenSource = new CancellationTokenSource();
                    await ScanDriveAndSearchFilesAsync(selectedDriveRotPath);
                    _publisher.SendScanCompleted();
                });
            }
        }

        public void ListDrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            if (drives is null)
                return;


            foreach (DriveInfo drive in drives)
                _publisher.PublishDriveInfo(drive.Name);
        }

        public async Task ScanDriveAndSearchFilesAsync(string path)
        {
            try
            {
                DriveInfo? drive = new DriveInfo(path);

                if (!drive.IsReady)
                    return;

                DirectoryInfo rootDirectory = new DirectoryInfo(path);
                FileInfo[]? files = GetFiles(rootDirectory);

                if (files is null)
                    return;

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_сancellationTokenSource.Token))
                {
                    var options = new ParallelOptions
                    {
                        CancellationToken = cts.Token
                    };

                    var folderInfo = new FolderInfo
                    {
                        Name = rootDirectory.FullName,
                        FileCount = files.Length,
                    };

                    long totalSize = 0;

                    // Asynchronously process each file
                    await Parallel.ForEachAsync(files, options, async (file, token) =>
                    {
                        ProcessFile(file, folderInfo, ref totalSize);

                        while (_isPaused)
                        {
                            if (token.IsCancellationRequested)
                                break;
                            await Task.Delay(100, token);
                        }
                    });

                    folderInfo.TotalSize = totalSize;

                    await GetDirectories(folderInfo, rootDirectory, cts);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Search files error");
            }
        }

        private async Task GetDirectories(FolderInfo folderInfo, DirectoryInfo rootDirectory, CancellationTokenSource cts)
        {
            if (folderInfo.isBigFilePresent)
            {
                _publisher.PublishFolderInfo(folderInfo);
            }

            foreach (var subDirectory in rootDirectory.GetDirectories())
            {
                if (!cts.Token.IsCancellationRequested)
                {
                    await ScanDriveAndSearchFilesAsync(subDirectory.FullName);
                }
                else
                {
                    break;
                }
            }
        }

        private FileInfo[]? GetFiles(DirectoryInfo rootDirectory)
        {
            try
            {
                return rootDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        private void ProcessFile(FileInfo file, FolderInfo folderInfo, ref long totalSize)
        {
            try
            {
                if (file.Length > TargetFileSize)
                {
                    folderInfo.isBigFilePresent = true;
                }

                Interlocked.Add(ref totalSize, file.Length);// Utilizes Interlocked.Add for safe incrementation
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error processing file: {file.FullName}, Error: {e.Message}");
            }
        }
    }
}
