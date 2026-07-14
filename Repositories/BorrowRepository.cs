using Google.Cloud.Firestore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BorrowRepository : FirebaseRepository<Borrow>
    {
        public BorrowRepository() : base("Borrows") { }

        public async Task<List<Borrow>> GetByStudentIdAsync(string studentId)
        {
            var snapshot = await Collection.WhereEqualTo("StudentId", studentId).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<Borrow>()).ToList();
        }

        public async Task<List<Borrow>> GetByBookIdAsync(string bookId)
        {
            var snapshot = await Collection.WhereEqualTo("BookId", bookId).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<Borrow>()).ToList();
        }

        public async Task<List<Borrow>> GetActiveBorrowsByStudentAsync(string studentId)
        {
            var snapshot = await Collection
                .WhereEqualTo("StudentId", studentId)
                .WhereEqualTo("Status", "Borrowed")
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<Borrow>()).ToList();
        }

        public async Task<List<Borrow>> GetOverdueBorrowsAsync()
        {
            var now = Timestamp.GetCurrentTimestamp();
            var snapshot = await Collection
                .WhereEqualTo("Status", "Borrowed")
                .WhereLessThan("DueDate", now)
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ConvertTo<Borrow>()).ToList();
        }
    }
}
