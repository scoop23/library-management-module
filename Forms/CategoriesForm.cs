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
            Width = 600;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            dgvCategories = new DataGridView
            {
                Location = new Point(20, 20),
                Width = 540,
                Height = 260,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCategories.SelectionChanged += DgvCategories_SelectionChanged;

            var lblName = new Label { Text = "Name:", Location = new Point(20, 295), Width = 80 };
            txtName = new TextBox { Location = new Point(100, 292), Width = 200 };

            var lblDesc = new Label { Text = "Description:", Location = new Point(20, 330), Width = 80 };
            txtDescription = new TextBox { Location = new Point(100, 327), Width = 200 };

            btnAdd = new Button { Text = "Add", Location = new Point(320, 292), Width = 90, BackColor = Color.FromArgb(46, 139, 87), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "Update", Location = new Point(320, 327), Width = 90, FlatStyle = FlatStyle.Flat };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "Delete", Location = new Point(420, 292), Width = 90, BackColor = Color.FromArgb(178, 34, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button { Text = "Close", Location = new Point(420, 327), Width = 90, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };

            Controls.AddRange(new Control[] { dgvCategories, lblName, txtName, lblDesc, txtDescription, btnAdd, btnUpdate, btnDelete, btnClose });
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
