using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BookCopyRepository : FirebaseRepository<BookCopy>
    {
        public BookCopyRepository() : base("BookCopies") { }

        public async Task<List<BookCopy>> GetByBookIdAsync(string bookId)
        {
            var snapshot = await Collection.WhereEqualTo("BookId", bookId).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<BookCopy>()).ToList();
        }
    }
}
