using Firebase.Database.Query;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BookCopyRepository : FirebaseRepository<BookCopy>
    {
        public BookCopyRepository() : base("library/bookcopies") { }

        public async Task<List<BookCopy>> GetByBookIdAsync(string bookId)
        {
            var records = await client.Child(CollectionName)
                .OrderBy("BookId")
                .EqualTo(bookId)
                .OnceAsync<BookCopy>();

            return records.Select(r =>
            {
                r.Object.CopyId = r.Key;
                return r.Object;
            }).ToList();
        }
    }
}
