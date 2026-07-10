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
        private Button btnSearch, btnAdd, btnEdit, btnDelete, btnArchive, btnCopies, btnRefresh, btnCategories;
        private Label lblTitle;

        public BooksForm()
        {
            InitializeComponent();
            Load += BookForms_Load;
        }

        private void InitializeComponent()
        {
            Text = "Book Management";
            Width = 1020;
            Height = 620;
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "Book Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 67, 140),
                Location = new Point(20, 15),
                AutoSize = true
            };

            txtSearch = new TextBox { Location = new Point(20, 55), Width = 240, PlaceholderText = "Search by title..." };

            btnSearch = new Button { Text = "Search", Location = new Point(270, 53), Width = 80, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSearch.Click += BtnSearch_Click;

            cmbCategory = new ComboBox { Location = new Point(360, 55), Width = 190, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "CategoryId" };
            cmbCategory.SelectedIndexChanged += SelectIndex_Changed;

            btnRefresh = new Button { Text = "Refresh", Location = new Point(560, 53), Width = 80, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += BtnRefresh_Click;

            btnAdd = new Button { Text = "Add Book", Location = new Point(670, 53), Width = 90, BackColor = Color.FromArgb(46, 139, 87), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button { Text = "Edit", Location = new Point(770, 53), Width = 70, FlatStyle = FlatStyle.Flat };
            btnEdit.Click += BtnEdit_Click;

            btnCopies = new Button { Text = "Copies", Location = new Point(850, 53), Width = 90, FlatStyle = FlatStyle.Flat };
            btnCopies.Click += BtnCopies_Click;

            btnCategories = new Button { Text = "Categories...", Location = new Point(20, 550), Width = 110, FlatStyle = FlatStyle.Flat };
            btnCategories.Click += BtnCategories_Click;

            dgvBooks = new DataGridView
            {
                Location = new Point(20, 95),
                Width = 960,
                Height = 410,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnArchive = new Button { Text = "Archive", Location = new Point(20, 520), Width = 100, FlatStyle = FlatStyle.Flat };
            btnArchive.Click += BtnArchive_Click;

            btnDelete = new Button { Text = "Delete", Location = new Point(130, 520), Width = 100, BackColor = Color.FromArgb(178, 34, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            Controls.AddRange(new Control[]
            {
                lblTitle, txtSearch, btnSearch, cmbCategory, btnRefresh, btnAdd, btnEdit, btnCopies,
                dgvBooks, btnArchive, btnDelete, btnCategories
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

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            await DeleteSelectedAsync();
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
