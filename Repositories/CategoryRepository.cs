using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class CategoryRepository : FirebaseRepository<Category>
    {
        public CategoryRepository() : base("lib_categories") { }
    }
}
