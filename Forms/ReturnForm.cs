using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class ReturnForm : Form
    {
        private readonly BorrowService _borrowService = new();
        private readonly InternalStudentService _studentService = new();

        private DataGridView dgvBorrows;
        private TextBox txtStudentSearch;
        private Button btnSearch, btnReturn, btnMarkLost, btnRefresh, btnClose;
        private Label lblSelectedBorrow, lblTitle;

        public ReturnForm()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadActiveBorrowsAsync();
        }

        private void InitializeComponent()
        {
            Text = "Return Book";
            Width = 920;
            Height = 610;
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
                Text = "Return Book",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 13),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var pnlSearch = new Panel
            {
                Location = new Point(20, 65),
                Width = 870,
                Height = 44,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(10, 12), Width = 55, Font = new Font("Segoe UI", 9) };
            txtStudentSearch = new TextBox { Location = new Point(70, 8), Width = 260, PlaceholderText = "Search by student name..." };
            btnSearch = new Button { Text = "Search", Location = new Point(340, 6), Width = 90, Height = 28, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += async (s, e) => await LoadActiveBorrowsAsync(txtStudentSearch.Text.Trim());

            btnRefresh = new Button { Text = "Refresh", Location = new Point(445, 6), Width = 90, Height = 28, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += async (s, e) => { txtStudentSearch.Clear(); await LoadActiveBorrowsAsync(); };

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtStudentSearch, btnSearch, btnRefresh });

            dgvBorrows = new DataGridView
            {
                Location = new Point(20, 125),
                Width = 870,
                Height = 340,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvBorrows.SelectionChanged += DgvBorrows_SelectionChanged;

            lblSelectedBorrow = new Label { Text = "No borrow selected", Location = new Point(20, 442), Width = 600, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Italic) };

            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnReturn = new Button
            {
                Text = "Mark as Returned",
                Location = new Point(20, 18),
                Width = 160,
                Height = 38,
                BackColor = Color.FromArgb(46, 139, 87),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnReturn.Click += BtnReturn_Click;

            btnMarkLost = new Button
            {
                Text = "Mark as Lost",
                Location = new Point(195, 18),
                Width = 130,
                Height = 38,
                BackColor = Color.FromArgb(178, 34, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnMarkLost.Click += BtnMarkLost_Click;

            btnClose = new Button { Text = "Close", Location = new Point(770, 18), Width = 90, Height = 38, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            pnlFooter.Controls.AddRange(new Control[] { btnReturn, btnMarkLost, btnClose });

            Controls.AddRange(new Control[]
            {
                pnlHeader, lblTitle, pnlSearch, dgvBorrows, lblSelectedBorrow, pnlFooter
            });
        }

        private async Task LoadActiveBorrowsAsync(string studentNameFilter = null)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                List<Borrow> borrows;
                if (!string.IsNullOrWhiteSpace(studentNameFilter))
                {
                    var students = await _studentService.SearchStudentAsync(studentNameFilter);
                    var studentIds = students.Select(s => s.StudentId).ToList();
                    borrows = new List<Borrow>();
                    foreach (var sid in studentIds)
                    {
                        var b = await _borrowService.GetActiveBorrowsByStudentAsync(sid);
                        borrows.AddRange(b);
                    }
                }
                else
                {
                    borrows = await _borrowService.GetActiveBorrowsAsync();
                }

                dgvBorrows.DataSource = borrows
                    .Select(b => new
                    {
                        b.BorrowId,
                        b.StudentName,
                        b.BookTitle,
                        BorrowDate = b.BorrowDate.ToString("yyyy-MM-dd"),
                        DueDate = b.DueDate.ToString("yyyy-MM-dd"),
                        b.Status
                    })
                    .ToList();

                if (dgvBorrows.Columns["BorrowId"] != null)
                    dgvBorrows.Columns["BorrowId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load borrows: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string GetSelectedBorrowId()
        {
            if (dgvBorrows.SelectedRows.Count == 0) return null;
            return dgvBorrows.SelectedRows[0].Cells["BorrowId"].Value?.ToString();
        }

        private void DgvBorrows_SelectionChanged(object sender, EventArgs e)
        {
            var id = GetSelectedBorrowId();
            if (id == null)
            {
                lblSelectedBorrow.Text = "No borrow selected";
                btnReturn.Enabled = false;
                btnMarkLost.Enabled = false;
                return;
            }
            var row = dgvBorrows.SelectedRows[0];
            lblSelectedBorrow.Text = $"{row.Cells["StudentName"].Value} - {row.Cells["BookTitle"].Value} (Due: {row.Cells["DueDate"].Value})";
            btnReturn.Enabled = true;
            btnMarkLost.Enabled = true;
        }

        private async void BtnReturn_Click(object sender, EventArgs e)
        {
            var id = GetSelectedBorrowId();
            if (id == null) return;

            if (MessageBox.Show("Mark this book as returned?", "Confirm Return", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                await _borrowService.ReturnBookAsync(id);
                MessageBox.Show("Book returned successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadActiveBorrowsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async void BtnMarkLost_Click(object sender, EventArgs e)
        {
            var id = GetSelectedBorrowId();
            if (id == null) return;

            if (MessageBox.Show("Mark this book as lost?", "Confirm Lost", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                await _borrowService.MarkAsLostAsync(id);
                MessageBox.Show("Book marked as lost.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadActiveBorrowsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
