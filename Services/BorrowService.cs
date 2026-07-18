using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services
{
    public class BorrowService
    {
        private readonly BorrowRepository _borrowRepository;
        private readonly InternalStudentService _studentService;
        private readonly BookRepository _bookRepository;
        private readonly BookCopyRepository _copyRepository;

        private const int MaxBorrowsPerStudent = 5;
        private const int DefaultBorrowDays = 14;

        public BorrowService()
        {
            _borrowRepository = new BorrowRepository();
            _studentService = new InternalStudentService();
            _bookRepository = new BookRepository();
            _copyRepository = new BookCopyRepository();
        }

        public Task<List<Borrow>> GetAllBorrowsAsync() => _borrowRepository.GetAllAsync();

        public Task<Borrow> GetBorrowByIdAsync(string id) => _borrowRepository.GetByIdAsync(id);

        public Task<List<Borrow>> GetBorrowsByStudentAsync(string studentId) => _borrowRepository.GetByStudentIdAsync(studentId);

        public Task<List<Borrow>> GetActiveBorrowsByStudentAsync(string studentId) => _borrowRepository.GetActiveBorrowsByStudentAsync(studentId);

        public Task<List<Borrow>> GetOverdueBorrowsAsync() => _borrowRepository.GetOverdueBorrowsAsync();

        public async Task<string> BorrowBookAsync(string studentId, string bookId, string copyId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId)
                ?? throw new InvalidOperationException("Student not found.");

            if (student.Status != "Active")
                throw new InvalidOperationException($"Student is not active (status: {student.Status}).");

            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new InvalidOperationException("Book not found.");

            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No copies available for this book.");

            var copy = await _copyRepository.GetByIdAsync(copyId)
                ?? throw new InvalidOperationException("Book copy not found.");

            if (copy.BookId != bookId)
                throw new InvalidOperationException("This copy does not belong to the selected book.");

            if (copy.Status != "Available")
                throw new InvalidOperationException($"This copy is not available (status: {copy.Status}).");

            var activeBorrows = await _borrowRepository.GetActiveBorrowsByStudentAsync(studentId);
            if (activeBorrows.Count >= MaxBorrowsPerStudent)
                throw new InvalidOperationException($"Student has reached the maximum of {MaxBorrowsPerStudent} active borrows.");

            var now = DateTime.UtcNow;
            var borrow = new Borrow
            {
                StudentId = studentId,
                StudentName = $"{student.LastName}. {student.FirstName}",
                BookId = bookId,
                BookTitle = book.Title,
                CopyId = copyId,
                BorrowDate = now,
                DueDate = now.AddDays(DefaultBorrowDays),
                Status = "Borrowed",
                CreatedAt = now,
                UpdatedAt = now
            };

            var borrowId = await _borrowRepository.AddAsync(borrow);

            copy.Status = "Borrowed";
            await _copyRepository.UpdateAsync(copyId, copy);

            book.AvailableCopies = Math.Max(0, book.AvailableCopies - 1);
            book.UpdatedAt = now;
            await _bookRepository.UpdateAsync(bookId, book);

            return borrowId;
        }

        public async Task ReturnBookAsync(string borrowId)
        {
            var borrow = await _borrowRepository.GetByIdAsync(borrowId)
                ?? throw new InvalidOperationException("Borrow record not found.");

            if (borrow.Status == "Returned")
                throw new InvalidOperationException("This book has already been returned.");

            var now = DateTime.UtcNow;

            borrow.ReturnDate = now;
            borrow.Status = "Returned";
            borrow.UpdatedAt = now;
            await _borrowRepository.UpdateAsync(borrowId, borrow);

            var copy = await _copyRepository.GetByIdAsync(borrow.CopyId);
            if (copy != null)
            {
                copy.Status = "Available";
                await _copyRepository.UpdateAsync(borrow.CopyId, copy);
            }

            var book = await _bookRepository.GetByIdAsync(borrow.BookId);
            if (book != null)
            {
                book.AvailableCopies += 1;
                book.UpdatedAt = now;
                await _bookRepository.UpdateAsync(borrow.BookId, book);
            }
        }

        public async Task MarkAsLostAsync(string borrowId)
        {
            var borrow = await _borrowRepository.GetByIdAsync(borrowId)
                ?? throw new InvalidOperationException("Borrow record not found.");

            if (borrow.Status == "Returned")
                throw new InvalidOperationException("Cannot mark a returned book as lost.");

            var now = DateTime.UtcNow;

            borrow.Status = "Lost";
            borrow.UpdatedAt = now;
            await _borrowRepository.UpdateAsync(borrowId, borrow);

            var copy = await _copyRepository.GetByIdAsync(borrow.CopyId);
            if (copy != null)
            {
                copy.Status = "Lost";
                await _copyRepository.UpdateAsync(borrow.CopyId, copy);
            }
        }

        public async Task<List<Borrow>> GetActiveBorrowsAsync()
        {
            var all = await _borrowRepository.GetAllAsync();
            return all.Where(b => b.Status == "Borrowed").ToList();
        }
    }
}
