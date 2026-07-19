using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class BookCopiesForm : Form
    {
        private readonly BookCopyService _copyService = new();
        private readonly string _bookId;

        private DataGridView dgvCopies;
        private ComboBox cmbCondition;
        private Button btnAdd, btnUpdateCondition, btnDelete, btnClose;
        private Label lblTitle;

        public BookCopiesForm(string bookId)
        {
            _bookId = bookId;
            InitializeComponent();
            Load += async (s, e) => await LoadCopiesAsync();
        }

        private void InitializeComponent()
        {
            Text = "Manage Book Copies";
            Width = 660;
            Height = 580;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 247, 250);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(21, 67, 140)
            };

            lblTitle = new Label
            {
                Text = "Book Copies",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 13),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            dgvCopies = new DataGridView
            {
                Location = new Point(20, 65),
                Width = 610,
                Height = 370,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var pnlControls = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblCondition = new Label { Text = "Condition:", Location = new Point(10, 12), Width = 80, Font = new Font("Segoe UI", 9) };
            cmbCondition = new ComboBox
            {
                Location = new Point(95, 8),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCondition.Items.AddRange(new object[] { "New", "Good", "Fair", "Damaged" });
            cmbCondition.SelectedIndex = 0;

            btnAdd = new Button { Text = "Add Copy", Location = new Point(255, 6), Width = 110, Height = 32, BackColor = Color.FromArgb(46, 139, 87), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnUpdateCondition = new Button { Text = "Update Condition", Location = new Point(10, 50), Width = 140, Height = 32, FlatStyle = FlatStyle.Flat };
            btnUpdateCondition.Click += BtnUpdateCondition_Click;

            btnDelete = new Button { Text = "Delete Copy", Location = new Point(160, 50), Width = 120, Height = 32, BackColor = Color.FromArgb(178, 34, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button { Text = "Close", Location = new Point(510, 50), Width = 90, Height = 32, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            pnlControls.Controls.AddRange(new Control[]
            {
                lblCondition, cmbCondition, btnAdd, btnUpdateCondition, btnDelete, btnClose
            });

            Controls.AddRange(new Control[]
            {
                pnlHeader, lblTitle, dgvCopies, pnlControls
            });
        }

        private async Task LoadCopiesAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var copies = await _copyService.GetCopiesByBookAsync(_bookId);
                dgvCopies.DataSource = copies.Select(c => new { c.CopyId, c.Condition, c.Status }).ToList();
                if (dgvCopies.Columns["CopyId"] != null)
                    dgvCopies.Columns["CopyId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load copies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string GetSelectedCopyId()
        {
            if (dgvCopies.SelectedRows.Count == 0) return null;
            return dgvCopies.SelectedRows[0].Cells["CopyId"].Value?.ToString();
        }

        private async Task AddCopyAsync()
        {
            try
            {
                var copy = new BookCopy { BookId = _bookId, Condition = cmbCondition.SelectedItem.ToString() };
                await _copyService.AddCopyAsync(copy);
                await LoadCopiesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateSelectedConditionAsync()
        {
            var id = GetSelectedCopyId();
            if (id == null) { MessageBox.Show("Please select a copy first."); return; }

            try
            {
                var copy = new BookCopy
                {
                    CopyId = id,
                    BookId = _bookId,
                    Condition = cmbCondition.SelectedItem.ToString(),
                    Status = dgvCopies.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "Available"
                };
                await _copyService.UpdateCopyAsync(copy);
                await LoadCopiesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteSelectedAsync()
        {
            var id = GetSelectedCopyId();
            if (id == null) { MessageBox.Show("Please select a copy to delete."); return; }
            if (MessageBox.Show("Delete this copy?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                await _copyService.DeleteCopyAsync(id, _bookId);
                await LoadCopiesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            await AddCopyAsync();
        }

        private async void BtnUpdateCondition_Click(object sender, EventArgs e)
        {
            await UpdateSelectedConditionAsync();
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            await DeleteSelectedAsync();
        }
    }
}
