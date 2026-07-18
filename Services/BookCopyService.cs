using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services
{
    public class BookCopyService
    {
        private readonly BookCopyRepository _copyRepository;
        private readonly BookRepository _bookRepository;

        public BookCopyService()
        {
            _copyRepository = new BookCopyRepository();
            _bookRepository = new BookRepository();
        }

        public Task<List<BookCopy>> GetCopiesByBookAsync(string bookId) => _copyRepository.GetByBookIdAsync(bookId);

        public async Task<string> AddCopyAsync(BookCopy copy)
        {
            if (string.IsNullOrWhiteSpace(copy.BookId))
                throw new ArgumentException("BookId is required.");

            copy.Status = "Available";
            copy.CreatedAt = DateTime.UtcNow;

            var id = await _copyRepository.AddAsync(copy);

            var book = await _bookRepository.GetByIdAsync(copy.BookId);
            if (book != null)
            {
                book.TotalCopies += 1;
                book.AvailableCopies += 1;
                book.UpdatedAt = DateTime.UtcNow;
                await _bookRepository.UpdateAsync(book.BookId, book);
            }

            return id;
        }

        public async Task UpdateCopyAsync(BookCopy copy)
        {
            if (string.IsNullOrWhiteSpace(copy.CopyId))
                throw new ArgumentException("CopyId is required.");

            await _copyRepository.UpdateAsync(copy.CopyId, copy);
        }

        public async Task DeleteCopyAsync(string copyId, string bookId)
        {
            var copy = await _copyRepository.GetByIdAsync(copyId);
            if (copy != null && (copy.Status == "Borrowed" || copy.Status == "Reserved"))
                throw new InvalidOperationException("Cannot delete a copy that is currently borrowed or reserved.");

            await _copyRepository.DeleteAsync(copyId);

            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book != null)
            {
                book.TotalCopies = Math.Max(0, book.TotalCopies - 1);
                if (copy != null && copy.Status == "Available")
                    book.AvailableCopies = Math.Max(0, book.AvailableCopies - 1);
                book.UpdatedAt = DateTime.UtcNow;
                await _bookRepository.UpdateAsync(book.BookId, book);
            }
        }
    }
}
