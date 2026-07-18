using Firebase.Database.Query;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BookRepository : FirebaseRepository<Book>
    {
        public BookRepository() : base("Books") { }

        public async Task<List<Book>> SearchByTitleAsync(string keyword)
        {
            var all = await GetAllAsync();
            return all.Where(b => b.Title != null &&
                b.Title.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<List<Book>> GetByCategoryAsync(string categoryId)
        {
            var records = await Client.Child(CollectionName)
                .OrderBy("CategoryId")
                .EqualTo(categoryId)
                .OnceAsync<Book>();

            return records.Select(r =>
            {
                r.Object.BookId = r.Key;
                return r.Object;
            }).ToList();
        }

        public async Task<Book> GetByIsbnAsync(string isbn)
        {
            var records = await Client.Child(CollectionName)
                .OrderBy("ISBN")
                .EqualTo(isbn)
                .LimitToFirst(1)
                .OnceAsync<Book>();

            var match = records.FirstOrDefault();
            if (match != null)
                match.Object.BookId = match.Key;

            return match?.Object;
        }
    }
}
