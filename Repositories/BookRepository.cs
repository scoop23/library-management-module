using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BookRepository : FirebaseRepository<Book>
    {
        public BookRepository() : base("Books") { }

        /// <summary>Prefix search on Title (Firestore range-query trick).</summary>
        public async Task<List<Book>> SearchByTitleAsync(string keyword)
        {
            var snapshot = await Collection
                .WhereGreaterThanOrEqualTo("Title", keyword)
                .WhereLessThanOrEqualTo("Title", keyword + "\uf8ff")
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<Book>()).ToList();
        }

        public async Task<List<Book>> GetByCategoryAsync(string categoryId)
        {
            var snapshot = await Collection.WhereEqualTo("CategoryId", categoryId).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<Book>()).ToList();
        }

        public async Task<Book> GetByIsbnAsync(string isbn)
        {
            var snapshot = await Collection.WhereEqualTo("ISBN", isbn).Limit(1).GetSnapshotAsync();
            return snapshot.Documents.Count > 0 ? snapshot.Documents[0].ConvertTo<Book>() : null;
        }
    }
}
