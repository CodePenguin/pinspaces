using GongSolutions.Shell;
using System;
using System.IO;

namespace Pinspaces.Shell.Controls
{
    public class FileListItem
    {
        public FileListItem(string uri)
        {
            Uri = uri;
            Refresh();
        }

        public string DisplayName { get; private set; }
        public string FileTypeDescription { get; private set; }
        public DateTime LastModifiedDateTime { get; private set; }
        public int Size { get; private set; }
        public string Uri { get; private set; }

        public void Refresh()
        {
            var shellItem = new ShellItem(Uri);
            var fileInfo = new FileInfo(shellItem.FileSystemPath);
            DisplayName = shellItem.DisplayName;
            LastModifiedDateTime = fileInfo.LastWriteTime;
            FileTypeDescription = shellItem.FileTypeDescription;

            if (!shellItem.IsFolder || fileInfo.Extension.Equals(".zip", StringComparison.CurrentCultureIgnoreCase))
            {
                Size = (int)Math.Ceiling(fileInfo.Length / 1024d);
            }
            else
            {
                Size = 0;
            }

            // FIX!!!
            //item.ImageIndex = shellItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OverlayIndex);
        }
    }
}
