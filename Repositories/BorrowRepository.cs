using Firebase.Database.Query;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BorrowRepository : FirebaseRepository<Borrow>
    {
        public BorrowRepository() : base("library/borrows") { }

        public async Task<List<Borrow>> GetByStudentIdAsync(string studentId)
        {
            var records = await client.Child(CollectionName)
                .OrderBy("StudentId")
                .EqualTo(studentId)
                .OnceAsync<Borrow>();

            return records.Select(r =>
            {
                r.Object.BorrowId = r.Key;
                return r.Object;
            }).ToList();
        }

        public async Task<List<Borrow>> GetByBookIdAsync(string bookId)
        {
            var records = await client.Child(CollectionName)
                .OrderBy("BookId")
                .EqualTo(bookId)
                .OnceAsync<Borrow>();

            return records.Select(r =>
            {
                r.Object.BorrowId = r.Key;
                return r.Object;
            }).ToList();
        }

        public async Task<List<Borrow>> GetActiveBorrowsByStudentAsync(string studentId)
        {
            var records = await client.Child(CollectionName)
                .OrderBy("StudentId")
                .EqualTo(studentId)
                .OnceAsync<Borrow>();

            return records
                .Where(r => r.Object.Status == "Borrowed")
                .Select(r =>
                {
                    r.Object.BorrowId = r.Key;
                    return r.Object;
                })
                .ToList();
        }

        public async Task<List<Borrow>> GetOverdueBorrowsAsync()
        {
            var all = await GetAllAsync();
            return all.Where(b => b.Status == "Borrowed" && b.DueDate < DateTime.UtcNow).ToList();
        }
    }
}
