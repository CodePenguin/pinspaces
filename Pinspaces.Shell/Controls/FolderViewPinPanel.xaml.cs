using Pinspaces.Core.Data;
using Pinspaces.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Pinspaces.Shell.Controls
{
    [PinType(DisplayName = "Folder View", PinType = typeof(FolderViewPin))]
    public partial class FolderViewPinPanel : UserControl, IPinControl
    {
        private FileSystemWatcher fileSystemWatcher;
        private bool pendingRefresh;
        private FolderViewPin pin;
        private Window window;

        public FolderViewPinPanel()
        {
            InitializeComponent();
            DataContext = this;

            shellListView.RefreshItems += ShellListView_RefreshItems;

            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Control ContentControl => this;

        public ObservableCollection<ShellListItem> Items => shellListView.Items;

        public void AddContextMenuItems(ContextMenu contextMenu)
        {
            var menuItem = new MenuItem { Header = "Select folder..." };
            menuItem.Click += SelectFolderContextMenuItem_Click;
            contextMenu.Items.Add(menuItem);
        }

        public void LoadPin(Guid pinspaceId, Pin pin)
        {
            this.pin = pin as FolderViewPin;
            _ = Task.Run(RefreshItems);
            InitializeFileSystemWatcher();
        }

        private void DeinitializeFileSystemWatcher()
        {
            fileSystemWatcher?.Dispose();
            fileSystemWatcher = null;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            pendingRefresh = true;
        }

        private void InitializeFileSystemWatcher()
        {
            DeinitializeFileSystemWatcher();
            fileSystemWatcher = new FileSystemWatcher(pin.FolderPath);
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private async Task RefreshItems()
        {
            var shellItems = await Task.Run(RetrieveShellItems);
            Items.Clear();
            foreach (var item in shellItems)
            {
                Items.Add(item);
            }
        }

        private Task<IEnumerable<ShellListItem>> RetrieveShellItems()
        {
            var list = new List<ShellListItem>();
            var directoryInfo = new DirectoryInfo(pin.FolderPath);
            foreach (var info in directoryInfo.EnumerateFileSystemInfos())
            {
                list.Add(new ShellListItem(info));
            }
            return Task.FromResult((IEnumerable<ShellListItem>)list);
        }

        private async void SelectFolderContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pin.FolderPath = dialog.SelectedPath;
                await RefreshItems();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(pin.FolderPath)));
            }
        }

        private async void ShellListView_RefreshItems(object sender, EventArgs e)
        {
            await RefreshItems();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            window = Window.GetWindow(this);
            window.Activated += Window_Activated;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Activated -= Window_Activated;
            DeinitializeFileSystemWatcher();
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            if (pendingRefresh)
            {
                pendingRefresh = false;
                await RefreshItems();
            }
        }
    }
}
