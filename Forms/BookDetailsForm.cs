using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class BookDetailsForm : Form
    {
        private readonly BookService _bookService = new();
        private readonly CategoryService _categoryService = new();
        private readonly string _bookId;
        private Book _existingBook;
        private string _selectedCoverPath;

        private TextBox txtIsbn, txtTitle, txtAuthor, txtPublisher;
        private TextBox txtDescription;
        private NumericUpDown numYear, numCopies;
        private ComboBox cmbCategory;
        private PictureBox picCover;
        private Label lblCoverPath;
        private Button btnBrowseCover, btnSave, btnCancel;

        public BookDetailsForm(string bookId)
        {
            _bookId = bookId;
            InitializeComponent();
            Load += async (s, e) =>
            {
                await LoadCategoriesAsync();
                if (_bookId != null) await LoadBookAsync();
            };
        }

        private void InitializeComponent()
        {
            Text = _bookId == null ? "Add Book" : "Edit Book";
            Width = 480;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            int y = 20;
            const int labelWidth = 110, fieldWidth = 300, rowHeight = 34;

            Label MakeLabel(string text) => new() { Text = text, Location = new Point(20, y + 4), Width = labelWidth };

            Controls.Add(MakeLabel("ISBN *"));
            txtIsbn = new TextBox { Location = new Point(140, y), Width = fieldWidth };
            Controls.Add(txtIsbn);
            y += rowHeight;

            Controls.Add(MakeLabel("Title *"));
            txtTitle = new TextBox { Location = new Point(140, y), Width = fieldWidth };
            Controls.Add(txtTitle);
            y += rowHeight;

            Controls.Add(MakeLabel("Author *"));
            txtAuthor = new TextBox { Location = new Point(140, y), Width = fieldWidth };
            Controls.Add(txtAuthor);
            y += rowHeight;

            Controls.Add(MakeLabel("Publisher"));
            txtPublisher = new TextBox { Location = new Point(140, y), Width = fieldWidth };
            Controls.Add(txtPublisher);
            y += rowHeight;

            Controls.Add(MakeLabel("Category"));
            cmbCategory = new ComboBox { Location = new Point(140, y), Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "CategoryId" };
            Controls.Add(cmbCategory);
            y += rowHeight;

            Controls.Add(MakeLabel("Publication Year"));
            numYear = new NumericUpDown { Location = new Point(140, y), Width = 120, Minimum = 0, Maximum = DateTime.Now.Year + 1, Value = DateTime.Now.Year };
            Controls.Add(numYear);
            y += rowHeight;

            Controls.Add(MakeLabel("Total Copies *"));
            numCopies = new NumericUpDown { Location = new Point(140, y), Width = 120, Minimum = 0, Maximum = 10000, Value = 1 };
            Controls.Add(numCopies);
            y += rowHeight;

            Controls.Add(MakeLabel("Description"));
            txtDescription = new TextBox { Location = new Point(140, y), Width = fieldWidth, Height = 70, Multiline = true, ScrollBars = ScrollBars.Vertical };
            Controls.Add(txtDescription);
            y += 80;

            Controls.Add(MakeLabel("Cover Image"));
            picCover = new PictureBox { Location = new Point(140, y), Width = 60, Height = 60, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            Controls.Add(picCover);
            btnBrowseCover = new Button { Text = "Browse...", Location = new Point(210, y + 18), Width = 90, FlatStyle = FlatStyle.Flat };
            btnBrowseCover.Click += BtnBrowseCover_Click;
            Controls.Add(btnBrowseCover);
            lblCoverPath = new Label { Location = new Point(310, y + 22), Width = 130, Text = "No file selected", ForeColor = Color.Gray, AutoEllipsis = true };
            Controls.Add(lblCoverPath);
            y += 80;

            btnSave = new Button { Text = "Save", Location = new Point(140, y), Width = 100, BackColor = Color.FromArgb(21, 67, 140), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            btnCancel = new Button { Text = "Cancel", Location = new Point(250, y), Width = 100, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void BtnBrowseCover_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _selectedCoverPath = dialog.FileName;
                lblCoverPath.Text = Path.GetFileName(dialog.FileName);
                picCover.Image = Image.FromFile(dialog.FileName);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                cmbCategory.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadBookAsync()
        {
            try
            {
                _existingBook = await _bookService.GetBookByIdAsync(_bookId);
                if (_existingBook == null) return;

                txtIsbn.Text = _existingBook.ISBN;
                txtTitle.Text = _existingBook.Title;
                txtAuthor.Text = _existingBook.Author;
                txtPublisher.Text = _existingBook.Publisher;
                txtDescription.Text = _existingBook.Description;
                numYear.Value = Math.Clamp(_existingBook.PublicationYear, (int)numYear.Minimum, (int)numYear.Maximum);
                numCopies.Value = Math.Max(_existingBook.TotalCopies, numCopies.Minimum);
                if (!string.IsNullOrEmpty(_existingBook.CategoryId))
                    cmbCategory.SelectedValue = _existingBook.CategoryId;
                if (!string.IsNullOrEmpty(_existingBook.CoverImageUrl))
                    lblCoverPath.Text = "Existing cover set";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load book: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var book = _existingBook ?? new Book();
                book.ISBN = txtIsbn.Text.Trim();
                book.Title = txtTitle.Text.Trim();
                book.Author = txtAuthor.Text.Trim();
                book.Publisher = txtPublisher.Text.Trim();
                book.Description = txtDescription.Text.Trim();
                book.PublicationYear = (int)numYear.Value;
                book.TotalCopies = (int)numCopies.Value;
                book.CategoryId = cmbCategory.SelectedValue as string;
                book.CategoryName = (cmbCategory.SelectedItem as Category)?.Name;

                // Cover image upload is optional and only attempted if Firebase Storage
                // has been configured (see README). Uses FirebaseStorageService.
                if (_selectedCoverPath != null)
                {
                    try
                    {
                        var storage = new FirebaseStorageService(
                            LibraryManagementSystem.Firebase.FirebaseConfig.ProjectId + ".appspot.com",
                            LibraryManagementSystem.Firebase.FirebaseConfig.CredentialsPath);
                        var tempId = book.BookId ?? Guid.NewGuid().ToString();
                        book.CoverImageUrl = await storage.UploadCoverImageAsync(_selectedCoverPath, tempId);
                    }
                    catch (Exception storageEx)
                    {
                        MessageBox.Show($"Book will be saved, but the cover image upload failed: {storageEx.Message}",
                            "Cover Upload Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                if (_bookId == null)
                    await _bookService.AddBookAsync(book);
                else
                    await _bookService.UpdateBookAsync(book);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
