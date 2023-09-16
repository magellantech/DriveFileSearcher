using DriveFileSearcher.Helpers;
using DriveFileSearcher.Model.Core;
using DriveFileSearcher.Model.Core.Enum;
using DriveFileSearcher.Model.Core.Interfaces;
using DriveFileSearcher.Model.Publisher;
using DriveFileSearcher.Model.Publisher.Interfaces;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DriveFileSearcher.VM
{
    public class ViewModel : ViewModelBase
    {
        public ObservableCollection<IDriveInfo> Drives { get; set; }
        public ObservableCollection<IFolderInfo> Folders { get; set; }
        private readonly IFileSearcher _scaner;
        private readonly ILogger _logger;

        public ViewModel()
        {
            _logger = LogManager.GetLogger(StrConsts.Logger);
            Drives = new ObservableCollection<IDriveInfo>();
            Folders = new ObservableCollection<IFolderInfo>();

            IEventPublisher publisher = new EventPublisher();
            _scaner = new FileSearcher(_logger, publisher);

            publisher.DriveFound += ViewModel_DriveFound;
            publisher.FolderFound += ViewModel_FolderFound;
            publisher.ScanCompleted += ViewModel_ScanCompleted;

            PropertyChanged += ViewModel_PropertyChanged;
        }

        private IDriveInfo? _selectedDrive;

        public IDriveInfo? SelectedDrive
        {
            get { return _selectedDrive; }
            set
            {
                if (_selectedDrive != value)
                {
                    _selectedDrive = value;
                    NotifyPropertyChanged(nameof(SelectedDrive));
                }
            }
        }

        private State _currentState;

        public State CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    NotifyPropertyChanged(nameof(CurrentState));
                }
            }
        }

        public async Task CancelScanAsync()
        {
            await DispatcherHelper.InvokeAsync(() =>
            {
                _scaner.CancelScan();
                CurrentState = State.NotActive;
            });
        }

        private async void ViewModel_ScanCompleted(object? sender, EventArgs e)
        {
            await DispatcherHelper.InvokeAsync(() =>
            {
                CurrentState = State.NotActive;
            });
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
                return;

            if (e.PropertyName.Equals("SelectedDrive") && CurrentState != State.NotActive)
            {
                await CancelScanAsync();
            }
        }

        private async void ViewModel_DriveFound(object? sender, IDriveInfo e)
        {
            if (e is not null && !string.IsNullOrEmpty(e.Name))
            {
                await DispatcherHelper.InvokeAsync(() =>
                {
                    Drives.Add(new DriveInfoViewModel
                    {
                        Name = e.Name
                    });
                });
            }
        }

        private async void ViewModel_FolderFound(object? sender, IFolderInfo e)
        {
            if (e is not null && !string.IsNullOrEmpty(e.Name))
            {
                await DispatcherHelper.InvokeAsync(() =>
                {
                    Folders.Add(new FolderInfoViewModel
                    {
                        Name = e.Name,
                        FileCount = e.FileCount,
                        TotalSize = e.TotalSize
                    });
                });
            }
        }

        private async void Start(object param)
        {
            await DispatcherHelper.InvokeAsync(async () =>
            {
                try
                {
                    if (CurrentState != State.Scan)
                    {
                        await StartOrResumeScan();
                    }
                    else
                    {
                        Pause();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Start searching error");
                }
            });
        }

        private async Task StartOrResumeScan()
        {
            if (SelectedDrive is null)
                return;

            if (string.IsNullOrEmpty(SelectedDrive.Name))
                return;

            if (CurrentState != State.Paused)
                Folders.Clear();

            CurrentState = State.Scan;
            await _scaner.StartOrResumeScan(SelectedDrive.Name);
        }

        private void Pause()
        {
            _scaner.PauseScan();
            CurrentState = State.Paused;
        }

        public void ListDrives()
        {
            _scaner.ListDrives();
        }

        #region Commands
        public bool CanExecute(object parameter)
        {
            return true;
        }

        private ICommand? _startCommand;

        public ICommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                    _startCommand = new DelegateCommand(CanExecute, Start);
                return _startCommand;
            }
        }
        #endregion commands
    }
}
