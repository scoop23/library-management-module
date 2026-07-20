using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class BooksForm : Form
    {
        private readonly BookService _bookService = new();
        private readonly CategoryService _categoryService = new();

        private DataGridView dgvBooks;
        private TextBox txtSearch;
        private ComboBox cmbCategory;
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnArchive, btnUnarchive, btnCopies, btnRefresh, btnCategories, btnBorrow, btnReturn, btnClearance;
        private Label lblTitle;

        public BooksForm()  
        {
            InitializeComponent();
            Load += BookForms_Load; 
        }

        private void InitializeComponent()
        {
            Text = "Library Management";
            Width = 1060;
            Height = 720;
            BackColor = Color.FromArgb(245, 247, 250);
            StartPosition = FormStartPosition.CenterScreen;

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(21, 67, 140)
            };

            lblTitle = new Label
            {
                Text = "Library Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            var pnlSearch = new Panel
            {
                Location = new Point(20, 75),
                Width = 1010,
                Height = 50,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(12, 14), Width = 55, Font = new Font("Segoe UI", 9) };
            txtSearch = new TextBox { Location = new Point(70, 10), Width = 260, PlaceholderText = "Search by title..." };

            btnSearch = new Button { Text = "Search", Location = new Point(345, 8), Width = 90, Height = 30, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += BtnSearch_Click;

            var lblCategory = new Label { Text = "Category:", Location = new Point(455, 14), Width = 65, Font = new Font("Segoe UI", 9) };
            cmbCategory = new ComboBox { Location = new Point(525, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "CategoryId" };
            cmbCategory.SelectedIndexChanged += SelectIndex_Changed;

            btnRefresh = new Button { Text = "Refresh", Location = new Point(745, 8), Width = 90, Height = 30, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += BtnRefresh_Click;

            btnCategories = new Button { Text = "Categories", Location = new Point(855, 8), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat };
            btnCategories.Click += BtnCategories_Click;

            pnlSearch.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, lblCategory, cmbCategory, btnRefresh, btnCategories });

            dgvBooks = new DataGridView
            {
                Location = new Point(20, 140),
                Width = 1010,
                Height = 420,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAdd = new Button { Text = "Add Book", Location = new Point(10, 10), Width = 100, Height = 30, BackColor = Color.FromArgb(46, 139, 87), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button { Text = "Edit", Location = new Point(115, 10), Width = 70, Height = 30, FlatStyle = FlatStyle.Flat };
            btnEdit.Click += BtnEdit_Click;

            btnCopies = new Button { Text = "Copies", Location = new Point(190, 10), Width = 70, Height = 30, FlatStyle = FlatStyle.Flat };
            btnCopies.Click += BtnCopies_Click;

            btnArchive = new Button { Text = "Archive", Location = new Point(265, 10), Width = 75, Height = 30, FlatStyle = FlatStyle.Flat };
            btnArchive.Click += BtnArchive_Click;

            btnUnarchive = new Button { Text = "Unarchive", Location = new Point(345, 10), Width = 85, Height = 30, FlatStyle = FlatStyle.Flat };
            btnUnarchive.Click += BtnUnarchive_Click;

            btnDelete = new Button { Text = "Delete", Location = new Point(435, 10), Width = 70, Height = 30, BackColor = Color.FromArgb(178, 34, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            var lblNav = new Label { Text = "Navigate:", Location = new Point(515, 15), Width = 65, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray };

            btnBorrow = new Button { Text = "Borrow Book", Location = new Point(585, 10), Width = 105, Height = 30, BackColor = Color.FromArgb(46, 139, 87), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBorrow.Click += BtnBorrow_Click;

            btnReturn = new Button { Text = "Return Book", Location = new Point(695, 10), Width = 105, Height = 30, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReturn.Click += BtnReturn_Click;

            btnClearance = new Button { Text = "Clearance", Location = new Point(805, 10), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat };
            btnClearance.Click += BtnClearance_Click;

            pnlActions.Controls.AddRange(new Control[]
            {
                btnAdd, btnEdit, btnCopies, btnArchive, btnUnarchive, btnDelete, lblNav, btnBorrow, btnReturn, btnClearance
            });

            Controls.AddRange(new Control[]
            {
                pnlHeader, pnlSearch, dgvBooks, pnlActions
            });
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var items = new List<Category> { new() { CategoryId = "", Name = "All Categories" } };
                items.AddRange(categories);
                cmbCategory.DataSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadBooksAsync(string keyword = null, string categoryId = null)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                List<Book> books = !string.IsNullOrWhiteSpace(keyword)
                    ? await _bookService.SearchBooksAsync(keyword)
                    : !string.IsNullOrEmpty(categoryId)
                        ? await _bookService.GetBooksByCategoryAsync(categoryId)
                        : await _bookService.GetAllBooksAsync();

                dgvBooks.DataSource = books
                    .Select(b => new
                    {
                        b.BookId,
                        b.ISBN,
                        b.Title,
                        b.Author,
                        Category = b.CategoryName,
                        b.TotalCopies,
                        b.AvailableCopies,
                        b.Status
                    })
                    .ToList();

                if (dgvBooks.Columns["BookId"] != null)
                    dgvBooks.Columns["BookId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load books: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string GetSelectedBookId()
        {
            if (dgvBooks.SelectedRows.Count == 0) return null;
            return dgvBooks.SelectedRows[0].Cells["BookId"].Value?.ToString();
        }

        private void OpenBookDetails(string bookId)
        {
            using var form = new BookDetailsForm(bookId);
            if (form.ShowDialog(this) == DialogResult.OK)
                _ = LoadBooksAsync();
        }

        private void EditSelected()
        {
            var id = GetSelectedBookId();
            if (id == null) { MessageBox.Show("Please select a book to edit."); return; }
            OpenBookDetails(id);
        }

        private void ManageCopiesSelected()
        {
            var id = GetSelectedBookId();
            if (id == null) { MessageBox.Show("Please select a book first."); return; }
            using var form = new BookCopiesForm(id);
            form.ShowDialog(this);
            _ = LoadBooksAsync();
        }

        private async Task ArchiveSelectedAsync()
        {
            var id = GetSelectedBookId();
            if (id == null) { MessageBox.Show("Please select a book to archive."); return; }
            if (MessageBox.Show("Archive this book?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try { await _bookService.ArchiveBookAsync(id); await LoadBooksAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task UnarchiveSelectedAsync()
        {
            var id = GetSelectedBookId();
            if (id == null) { MessageBox.Show("Please select a book to unarchive."); return; }
            if (MessageBox.Show("Unarchive this book? It will become available again.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try { await _bookService.UnarchiveBookAsync(id); await LoadBooksAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task DeleteSelectedAsync()
        {
            var id = GetSelectedBookId();
            if (id == null) { MessageBox.Show("Please select a book to delete."); return; }
            if (MessageBox.Show("Delete this book and all its copies? This cannot be undone.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try { await _bookService.DeleteBookAsync(id); await LoadBooksAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            await LoadBooksAsync(txtSearch.Text.Trim());
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;
            await LoadBooksAsync();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            OpenBookDetails(null);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void BtnCopies_Click(object sender, EventArgs e)
        {
            ManageCopiesSelected();
        }

        private async void BtnCategories_Click(object sender, EventArgs e)
        {
            using var form = new CategoriesForm();
            form.ShowDialog(this);
            await LoadCategoriesAsync();
            await LoadBooksAsync();
        }

        private async void BtnArchive_Click(object sender, EventArgs e)
        {
            await ArchiveSelectedAsync();
        }

        private async void BtnUnarchive_Click(object sender, EventArgs e)
        {
            await UnarchiveSelectedAsync();
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            await DeleteSelectedAsync();
        }

        private void BtnBorrow_Click(object sender, EventArgs e)
        {
            using var form = new BorrowForm();
            form.ShowDialog(this);
            _ = LoadBooksAsync();
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            using var form = new ReturnForm();
            form.ShowDialog(this);
            _ = LoadBooksAsync();
        }

        private void BtnClearance_Click(object sender, EventArgs e)
        {
            using var form = new ClearanceForm();
            form.ShowDialog(this);
        }

        private async void BookForms_Load(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
            await LoadBooksAsync();
        }
        private async void SelectIndex_Changed(object sender, EventArgs e)
        {
            var categoryId = cmbCategory.SelectedValue as string;
            await LoadBooksAsync(null, string.IsNullOrEmpty(categoryId) ? null : categoryId);
        }

    }
}
