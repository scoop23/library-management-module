using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class BorrowForm : Form
    {
        private readonly BorrowService _borrowService = new();
        private readonly InternalStudentService _studentService = new();
        private readonly BookService _bookService = new();
        private readonly BookCopyService _copyService = new();

        private DataGridView dgvStudents, dgvBooks, dgvCopies;
        private TextBox txtStudentSearch, txtBookSearch;
        private Button btnSearchStudent, btnSearchBook, btnBorrow, btnClose;
        private Label lblSelectedStudent, lblSelectedBook;
        private string _selectedStudentId, _selectedStudentName;
        private string _selectedBookId, _selectedCopyId;

        public BorrowForm()
        {
            InitializeComponent();
            Load += async (s, e) =>
            {
                await LoadStudentsAsync();
                await LoadBooksAsync();
            };
        }

        private void InitializeComponent()
        {
            Text = "Borrow Book";
            Width = 1000;
            Height = 750;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 247, 250);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(21, 67, 140)
            };

            var lblTitle = new Label
            {
                Text = "Borrow Book",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 13),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var lblStudentHeader = new Label { Text = "Step 1: Select Student", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 65), AutoSize = true, ForeColor = Color.FromArgb(21, 67, 140) };
            txtStudentSearch = new TextBox { Location = new Point(20, 90), Width = 260, PlaceholderText = "Search student by name..." };
            btnSearchStudent = new Button { Text = "Search", Location = new Point(290, 88), Width = 90, Height = 28, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearchStudent.Click += async (s, e) => await LoadStudentsAsync(txtStudentSearch.Text.Trim());

            dgvStudents = new DataGridView
            {
                Location = new Point(20, 122),
                Width = 945,
                Height = 120,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            lblSelectedStudent = new Label { Text = "No student selected", Location = new Point(20, 248), Width = 945, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Italic) };

            var lblBookHeader = new Label { Text = "Step 2: Select Book", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 278), AutoSize = true, ForeColor = Color.FromArgb(21, 67, 140) };
            txtBookSearch = new TextBox { Location = new Point(20, 303), Width = 260, PlaceholderText = "Search book by title..." };
            btnSearchBook = new Button { Text = "Search", Location = new Point(290, 301), Width = 90, Height = 28, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearchBook.Click += async (s, e) => await LoadBooksAsync(txtBookSearch.Text.Trim());

            dgvBooks = new DataGridView
            {
                Location = new Point(20, 335),
                Width = 945,
                Height = 110,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvBooks.SelectionChanged += DgvBooks_SelectionChanged;

            lblSelectedBook = new Label { Text = "No book selected", Location = new Point(20, 451), Width = 945, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Italic) };

            var lblCopyHeader = new Label { Text = "Step 3: Select Copy", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 481), AutoSize = true, ForeColor = Color.FromArgb(21, 67, 140) };

            dgvCopies = new DataGridView
            {
                Location = new Point(20, 506),
                Width = 945,
                Height = 90,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvCopies.SelectionChanged += DgvCopies_SelectionChanged;

            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnBorrow = new Button
            {
                Text = "Borrow Selected Copy",
                Location = new Point(720, 15),
                Width = 180,
                Height = 38,
                BackColor = Color.FromArgb(46, 139, 87),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnBorrow.Click += BtnBorrow_Click;

            btnClose = new Button { Text = "Close", Location = new Point(20, 15), Width = 100, Height = 38, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            pnlFooter.Controls.AddRange(new Control[] { btnBorrow, btnClose });

            Controls.AddRange(new Control[]
            {
                pnlHeader, lblTitle, lblStudentHeader, txtStudentSearch, btnSearchStudent, dgvStudents, lblSelectedStudent,
                lblBookHeader, txtBookSearch, btnSearchBook, dgvBooks, lblSelectedBook,
                lblCopyHeader, dgvCopies, pnlFooter
            });
        }

        private async Task LoadStudentsAsync(string keyword = null)
        {
            try
            {
                var students = !string.IsNullOrWhiteSpace(keyword)
                    ? await _studentService.SearchStudentAsync(keyword)
                    : await _studentService.GetAllStudentsAsync();

                var active = students
                    .Where(s => s.Status == "Active")
                    .Select(s => new { s.StudentId, s.FirstName, s.LastName, s.YearLevel, s.ContactNumber, s.ProgramId })
                    .ToList();

                dgvStudents.DataSource = active;

                if (dgvStudents.Columns["StudentId"] != null) dgvStudents.Columns["StudentId"].Visible = false;

                if (active.Count == 0)
                    MessageBox.Show($"No students found{(string.IsNullOrWhiteSpace(keyword) ? "" : $" matching '{keyword}'")}. Total from RTDB: {students.Count}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load students:\n{ex.Message}\n\n{ex.InnerException?.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadBooksAsync(string keyword = null)
        {
            try
            {
                var books = !string.IsNullOrWhiteSpace(keyword)
                    ? await _bookService.SearchBooksAsync(keyword)
                    : await _bookService.GetAllBooksAsync();

                dgvBooks.DataSource = books
                    .Where(b => b.AvailableCopies > 0 && b.Status == "Available")
                    .Select(b => new { b.BookId, b.Title, b.Author, b.AvailableCopies })
                    .ToList();

                if (dgvBooks.Columns["BookId"] != null)
                    dgvBooks.Columns["BookId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load books: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadCopiesForBookAsync(string bookId)
        {
            try
            {
                var copies = await _copyService.GetCopiesByBookAsync(bookId);
                dgvCopies.DataSource = copies
                    .Where(c => c.Status == "Available")
                    .Select(c => new { c.CopyId, c.Condition, c.Status })
                    .ToList();

                if (dgvCopies.Columns["CopyId"] != null)
                    dgvCopies.Columns["CopyId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load copies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0)
            {
                _selectedStudentId = null;
                _selectedStudentName = null;
                lblSelectedStudent.Text = "No student selected";
                UpdateBorrowButton();
                return;
            }
            var row = dgvStudents.SelectedRows[0];
            _selectedStudentId = row.Cells["StudentId"].Value?.ToString();
            _selectedStudentName = $"{row.Cells["FirstName"].Value} {row.Cells["LastName"].Value}";
            lblSelectedStudent.Text = $"Selected: {_selectedStudentName}";
            UpdateBorrowButton();
        }

        private async void DgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                _selectedBookId = null;
                _selectedCopyId = null;
                lblSelectedBook.Text = "No book selected";
                dgvCopies.DataSource = null;
                UpdateBorrowButton();
                return;
            }
            var row = dgvBooks.SelectedRows[0];
            _selectedBookId = row.Cells["BookId"].Value?.ToString();
            var title = row.Cells["Title"].Value?.ToString();
            lblSelectedBook.Text = $"Selected: {title} ({row.Cells["AvailableCopies"].Value} available)";
            _selectedCopyId = null;
            await LoadCopiesForBookAsync(_selectedBookId);
            UpdateBorrowButton();
        }

        private void DgvCopies_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCopies.SelectedRows.Count == 0)
            {
                _selectedCopyId = null;
                UpdateBorrowButton();
                return;
            }
            _selectedCopyId = dgvCopies.SelectedRows[0].Cells["CopyId"].Value?.ToString();
            UpdateBorrowButton();
        }

        private void UpdateBorrowButton()
        {
            btnBorrow.Enabled = _selectedStudentId != null && _selectedCopyId != null;
        }

        private async void BtnBorrow_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == null || _selectedCopyId == null) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                btnBorrow.Enabled = false;

                await _borrowService.BorrowBookAsync(_selectedStudentId, _selectedBookId, _selectedCopyId);

                MessageBox.Show($"Book borrowed successfully to {_selectedStudentName}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadBooksAsync();
                if (_selectedBookId != null)
                    await LoadCopiesForBookAsync(_selectedBookId);

                _selectedCopyId = null;
                UpdateBorrowButton();
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
