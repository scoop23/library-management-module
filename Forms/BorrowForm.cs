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
            Width = 980;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var lblTitle = new Label
            {
                Text = "Borrow Book",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 67, 140),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblStudentHeader = new Label { Text = "1. Select Student", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 55), AutoSize = true };
            txtStudentSearch = new TextBox { Location = new Point(20, 80), Width = 240, PlaceholderText = "Search student by name..." };
            btnSearchStudent = new Button { Text = "Search", Location = new Point(270, 78), Width = 80, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearchStudent.Click += async (s, e) => await LoadStudentsAsync(txtStudentSearch.Text.Trim());

            dgvStudents = new DataGridView
            {
                Location = new Point(20, 110),
                Width = 920,
                Height = 130,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            lblSelectedStudent = new Label { Text = "No student selected", Location = new Point(20, 248), Width = 920, ForeColor = Color.Gray };

            var lblBookHeader = new Label { Text = "2. Select Book", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 275), AutoSize = true };
            txtBookSearch = new TextBox { Location = new Point(20, 300), Width = 240, PlaceholderText = "Search book by title..." };
            btnSearchBook = new Button { Text = "Search", Location = new Point(270, 298), Width = 80, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearchBook.Click += async (s, e) => await LoadBooksAsync(txtBookSearch.Text.Trim());

            dgvBooks = new DataGridView
            {
                Location = new Point(20, 330),
                Width = 920,
                Height = 100,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvBooks.SelectionChanged += DgvBooks_SelectionChanged;

            lblSelectedBook = new Label { Text = "No book selected", Location = new Point(20, 438), Width = 920, ForeColor = Color.Gray };

            var lblCopyHeader = new Label { Text = "3. Select Copy", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 465), AutoSize = true };

            dgvCopies = new DataGridView
            {
                Location = new Point(20, 490),
                Width = 920,
                Height = 80,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvCopies.SelectionChanged += DgvCopies_SelectionChanged;

            btnBorrow = new Button
            {
                Text = "Borrow Selected Copy",
                Location = new Point(700, 560),
                Width = 180,
                Height = 35,
                BackColor = Color.FromArgb(46, 139, 87),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnBorrow.Click += BtnBorrow_Click;

            btnClose = new Button { Text = "Close", Location = new Point(890, 560), Width = 80, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            Controls.AddRange(new Control[]
            {
                lblTitle, lblStudentHeader, txtStudentSearch, btnSearchStudent, dgvStudents, lblSelectedStudent,
                lblBookHeader, txtBookSearch, btnSearchBook, dgvBooks, lblSelectedBook,
                lblCopyHeader, dgvCopies, btnBorrow, btnClose
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
