using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program : Form
{
    private TextBox textBox;
    private CheckedListBox checkedListBox;

    private const string ToDoListFile = "ToDoList.txt";
    private const string WindowPositionFile = "Position.txt";
    private const string ArchivesFile = "Archives.txt";

    static void Main()
    {
        Application.Run(new Program());
    }

    public Program()
    {
        Text = "CheckList";
        Size = new System.Drawing.Size(250, 150);
        MinimumSize = new System.Drawing.Size(250, 150);
        StartPosition = FormStartPosition.Manual;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

        if (!File.Exists(WindowPositionFile))
        {
            StartPosition = FormStartPosition.CenterScreen;
        }

        this.Icon = new Icon("Asset\\Icon.ico");

        LoadWindowPositionAndSize();

        checkedListBox = new CheckedListBox();
        checkedListBox.Location = new System.Drawing.Point(5, 5);
        checkedListBox.Size = new System.Drawing.Size(Width - 25, Height - 80);
        checkedListBox.BorderStyle = BorderStyle.None;
        checkedListBox.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        checkedListBox.ItemCheck += CheckedListBox_ItemCheck;
        Controls.Add(checkedListBox);

        textBox = new TextBox();
        textBox.Location = new System.Drawing.Point(5, Height - textBox.Height - 50);
        textBox.Size = new System.Drawing.Size(Width - 25, textBox.Height);
        textBox.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
        textBox.KeyDown += InputTextBox_KeyDown;
        Controls.Add(textBox);

        SizeChanged += Program_SizeChanged;
        LocationChanged += Program_LocationChanged;

        LoadData();
    }

    private void Program_SizeChanged(object? sender, EventArgs e)
    {
        if (sender is not null)
        {
            checkedListBox.Size = new System.Drawing.Size(Width - 25, Height - 80);
            textBox.Size = new System.Drawing.Size(Width - 25, textBox.Height);
            textBox.Location = new System.Drawing.Point(5, Height - textBox.Height - 50);

            SaveWindowPositionAndSize();
        }
    }

    private void Program_LocationChanged(object? sender, EventArgs e)
    {
        SaveWindowPositionAndSize();
    }

    private void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox textBox && e.KeyCode == Keys.Enter)
        {
            string inputText = textBox.Text.Trim();
            if (!string.IsNullOrEmpty(inputText))
            {
                checkedListBox.Items.Add(inputText, false);
                textBox.Text = string.Empty;

                SaveData();
            }
        }
    }

    private void CheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (e.NewValue == CheckState.Checked)
        {
            var selectedItem = checkedListBox.Items[e.Index].ToString();
            checkedListBox.SetItemChecked(e.Index, false);
            Task.Run(() => RemoveItemAfterDelay(selectedItem, 200));
        }
    }

    private void RemoveItemAfterDelay(string? item, int delay)
    {
        if (item != null)
        {
            Thread.Sleep(delay);
            this.BeginInvoke((Action)(() => checkedListBox.Items.Remove(item)));

            SaveData();
            ArchiveCompletedItem(item);
        }
    }

    private void ArchiveCompletedItem(string item)
    {
        string timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        string archiveEntry = $"{timestamp}: {item}";

        try
        {
            File.AppendAllText(ArchivesFile, archiveEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"완료 항목 보관 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveData()
    {
        File.WriteAllLines(ToDoListFile, checkedListBox.Items.Cast<string>());
    }

    private void LoadData()
    {
        if (File.Exists(ToDoListFile))
        {
            string[] lines = File.ReadAllLines(ToDoListFile);
            foreach (string line in lines)
            {
                checkedListBox.Items.Add(line, false);
            }
        }
    }

    private void LoadWindowPositionAndSize()
    {
        if (File.Exists(WindowPositionFile))
        {
            string[] positionLines = File.ReadAllLines(WindowPositionFile);
            if (positionLines.Length == 4 &&
                positionLines[0].StartsWith("x: ") && int.TryParse(positionLines[0].Substring(3), out int left) &&
                positionLines[1].StartsWith("y: ") && int.TryParse(positionLines[1].Substring(3), out int top) &&
                positionLines[2].StartsWith("w: ") && int.TryParse(positionLines[2].Substring(3), out int width) &&
                positionLines[3].StartsWith("h: ") && int.TryParse(positionLines[3].Substring(3), out int height))
            {
                if (left < 0) left = 0;
                if (top < 0) top = 0;
                if (top + 100 > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    top = (int)(0.9 * Screen.PrimaryScreen.WorkingArea.Height);
                }

                this.StartPosition = FormStartPosition.Manual;
                this.Location = new System.Drawing.Point(left, top);
                this.Size = new System.Drawing.Size(width, height);
            }
        }
    }

    private void SaveWindowPositionAndSize()
    {
        string[] positionLines =
        {
            $"x: {this.Left}",
            $"y: {this.Top}",
            $"w: {this.Width}",
            $"h: {this.Height}"
        };
        File.WriteAllLines(WindowPositionFile, positionLines);
    }
}