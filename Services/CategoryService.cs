using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService()
        {
            _categoryRepository = new CategoryRepository();
        }

        public Task<List<Category>> GetAllCategoriesAsync() => _categoryRepository.GetAllAsync();

        public Task<Category> GetCategoryByIdAsync(string id) => _categoryRepository.GetByIdAsync(id);

        public async Task<string> AddCategoryAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name is required.");
            return await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name is required.");
            await _categoryRepository.UpdateAsync(category.CategoryId, category);
        }

        public Task DeleteCategoryAsync(string id) => _categoryRepository.DeleteAsync(id);
    }
}
