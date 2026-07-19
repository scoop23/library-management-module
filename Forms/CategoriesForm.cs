using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class CategoriesForm : Form
    {
        private readonly CategoryService _categoryService = new();

        private DataGridView dgvCategories;
        private TextBox txtName, txtDescription;
        private Button btnAdd, btnUpdate, btnDelete, btnClose;

        public CategoriesForm()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadCategoriesAsync();
        }

        private void InitializeComponent()
        {
            Text = "Category Management";
            Width = 620;
            Height = 550;
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
                Text = "Category Management",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 13),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            dgvCategories = new DataGridView
            {
                Location = new Point(20, 65),
                Width = 570,
                Height = 310,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvCategories.SelectionChanged += DgvCategories_SelectionChanged;

            var pnlInput = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblName = new Label { Text = "Name:", Location = new Point(10, 12), Width = 80, Font = new Font("Segoe UI", 9) };
            txtName = new TextBox { Location = new Point(95, 8), Width = 250 };

            var lblDesc = new Label { Text = "Description:", Location = new Point(10, 48), Width = 80, Font = new Font("Segoe UI", 9) };
            txtDescription = new TextBox { Location = new Point(95, 44), Width = 250 };

            btnAdd = new Button { Text = "Add", Location = new Point(370, 6), Width = 90, Height = 32, BackColor = Color.FromArgb(46, 139, 87), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "Update", Location = new Point(370, 44), Width = 90, Height = 32, FlatStyle = FlatStyle.Flat };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "Delete", Location = new Point(470, 6), Width = 90, Height = 32, BackColor = Color.FromArgb(178, 34, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button { Text = "Close", Location = new Point(470, 44), Width = 90, Height = 32, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            pnlInput.Controls.AddRange(new Control[] { lblName, txtName, lblDesc, txtDescription, btnAdd, btnUpdate, btnDelete, btnClose });

            Controls.AddRange(new Control[] { pnlHeader, lblTitle, dgvCategories, pnlInput });
        }

        private void DgvCategories_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategories.SelectedRows.Count == 0) return;
            txtName.Text = dgvCategories.SelectedRows[0].Cells["Name"].Value?.ToString();
            txtDescription.Text = dgvCategories.SelectedRows[0].Cells["Description"].Value?.ToString();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                dgvCategories.DataSource = categories.Select(c => new { c.CategoryId, c.Name, c.Description }).ToList();
                if (dgvCategories.Columns["CategoryId"] != null)
                    dgvCategories.Columns["CategoryId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSelectedId()
        {
            if (dgvCategories.SelectedRows.Count == 0) return null;
            return dgvCategories.SelectedRows[0].Cells["CategoryId"].Value?.ToString();
        }

        private async Task AddAsync()
        {
            try
            {
                await _categoryService.AddCategoryAsync(new Category { Name = txtName.Text.Trim(), Description = txtDescription.Text.Trim() });
                txtName.Clear(); txtDescription.Clear();
                await LoadCategoriesAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task UpdateAsync()
        {
            var id = GetSelectedId();
            if (id == null) { MessageBox.Show("Please select a category to update."); return; }

            try
            {
                await _categoryService.UpdateCategoryAsync(new Category { CategoryId = id, Name = txtName.Text.Trim(), Description = txtDescription.Text.Trim() });
                await LoadCategoriesAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task DeleteAsync()
        {
            var id = GetSelectedId();
            if (id == null) { MessageBox.Show("Please select a category to delete."); return; }
            if (MessageBox.Show("Delete this category?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                txtName.Clear(); txtDescription.Clear();
                await LoadCategoriesAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            await AddAsync();
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            await UpdateAsync();
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            await DeleteAsync();
        }
    }
}
