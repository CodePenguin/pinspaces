using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FilePinboard
{
    public partial class PinboardWindow : Form
    {
        private List<Type> cellTypes = new List<Type>();
        private Point targetPoint;

        public PinboardWindow()
        {
            InitializeComponent();

            var cell = CreateNewCell(typeof(TextBoxCell));
            cell.Left = 10;
            cell.Top = 50;
            cell.PanelColor = Color.FromArgb(217, 237, 247);
            cell.Title = "Basic Panel";
            Controls.Add(cell);

            cell = CreateNewCell(typeof(FileListCell));
            cell.Left = 300;
            cell.Top = 50;
            cell.Width = 400;
            cell.Height = 200;
            cell.Title = "File List Panel";
            Controls.Add(cell);

            GenerateNewCellControlsMenu();
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            targetPoint = PointToClient(contextMenuStrip.Bounds.Location);
            var control = FindContextParent(contextMenuStrip.SourceControl);

            newCellMenuItem.Visible = control is PinboardWindow;
            removeCellMenuItem.Visible = control is PinboardCell;
        }

        private PinboardCell CreateNewCell(Type cellType)
        {
            var cell = Activator.CreateInstance(cellType, null) as PinboardCell;
            cell.Title = "New " + GetCellDisplayName(cellType);
            cell.ContextMenuStrip = contextMenuStrip;
            return cell;
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Control FindContextParent(Control control)
        {
            while (control != null && !(control is PinboardCell) && !(control is PinboardWindow))
            {
                control = control.Parent;
            }
            return control;
        }

        private void GenerateNewCellControlsMenu()
        {
            var types = Assembly.GetAssembly(typeof(PinboardCell)).GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PinboardCell)));
            foreach (var type in types)
            {
                var newMenuItem = new ToolStripMenuItem
                {
                    Text = GetCellDisplayName(type)
                };
                newMenuItem.Click += NewCellMenuItem_Click;
                newMenuItem.Tag = cellTypes.Count;
                newCellMenuItem.DropDownItems.Add(newMenuItem);
                cellTypes.Add(type);
            }
        }

        private string GetCellDisplayName(Type type)
        {
            return type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.Name;
        }

        private void NewCellMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var cellType = cellTypes[(int)menuItem.Tag];
            var cell = CreateNewCell(cellType);
            cell.Left = targetPoint.X;
            cell.Top = targetPoint.Y;
            Controls.Add(cell);
        }

        private void RemoveCellMenuItem_Click(object sender, EventArgs e)
        {
            var cell = FindContextParent(contextMenuStrip.SourceControl) as PinboardCell;
            Controls.Remove(cell);
            cell.Dispose();
        }
    }
}
