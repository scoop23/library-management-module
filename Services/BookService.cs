using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services
{
    public class BookService
    {
        private readonly BookRepository _bookRepository;
        private readonly BookCopyRepository _copyRepository;

        public BookService()
        {
            _bookRepository = new BookRepository();
            _copyRepository = new BookCopyRepository();
        }

        public Task<List<Book>> GetAllBooksAsync() => _bookRepository.GetAllAsync();

        public Task<Book> GetBookByIdAsync(string id) => _bookRepository.GetByIdAsync(id);

        public Task<List<Book>> SearchBooksAsync(string keyword) => _bookRepository.SearchByTitleAsync(keyword);

        public Task<List<Book>> GetBooksByCategoryAsync(string categoryId) => _bookRepository.GetByCategoryAsync(categoryId);

        public async Task<string> AddBookAsync(Book book)
        {
            Validate(book);

            var existing = await _bookRepository.GetByIsbnAsync(book.ISBN);
            if (existing != null)
                throw new InvalidOperationException($"A book with ISBN '{book.ISBN}' already exists.");

            book.AvailableCopies = book.TotalCopies;
            book.Status = "Available";
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;

            var bookId = await _bookRepository.AddAsync(book);

            var copyRepo = new BookCopyRepository();
            for (int i = 0; i < book.TotalCopies; i++)
            {
                var copy = new BookCopy
                {
                    BookId = bookId,
                    Condition = "New",
                    Status = "Available",
                    CreatedAt = DateTime.UtcNow
                };
                await copyRepo.AddAsync(copy);
            }

            return bookId;
        }

        public async Task UpdateBookAsync(Book book)
        {
            Validate(book);

            var existing = await _bookRepository.GetByIsbnAsync(book.ISBN);
            if (existing != null && existing.BookId != book.BookId)
                throw new InvalidOperationException($"Another book already uses ISBN '{book.ISBN}'.");

            var currentBook = await _bookRepository.GetByIdAsync(book.BookId);
            var currentCopies = currentBook?.TotalCopies ?? 0;

            book.UpdatedAt = DateTime.UtcNow;
            await _bookRepository.UpdateAsync(book.BookId, book);

            if (book.TotalCopies > currentCopies)
            {
                var copyRepo = new BookCopyRepository();
                int toAdd = book.TotalCopies - currentCopies;
                for (int i = 0; i < toAdd; i++)
                {
                    var copy = new BookCopy
                    {
                        BookId = book.BookId,
                        Condition = "New",
                        Status = "Available",
                        CreatedAt = DateTime.UtcNow
                    };
                    await copyRepo.AddAsync(copy);
                }
                book.AvailableCopies += toAdd;
                book.UpdatedAt = DateTime.UtcNow;
                await _bookRepository.UpdateAsync(book.BookId, book);
            }
            else if (book.TotalCopies < currentCopies)
            {
                var copies = await _copyRepository.GetByBookIdAsync(book.BookId);
                var available = copies.Where(c => c.Status == "Available").ToList();
                int toRemove = Math.Min(currentCopies - book.TotalCopies, available.Count);
                var copyRepo = new BookCopyRepository();
                for (int i = 0; i < toRemove; i++)
                {
                    await copyRepo.DeleteAsync(available[i].CopyId);
                }
                book.AvailableCopies -= toRemove;
                book.UpdatedAt = DateTime.UtcNow;
                await _bookRepository.UpdateAsync(book.BookId, book);
            }
        }

        public async Task DeleteBookAsync(string bookId)
        {
            var copies = await _copyRepository.GetByBookIdAsync(bookId);
            if (copies.Any(c => c.Status == "Borrowed" || c.Status == "Reserved"))
                throw new InvalidOperationException(
                    "Cannot delete a book that has copies currently borrowed or reserved.");

            foreach (var copy in copies)
                await _copyRepository.DeleteAsync(copy.CopyId);

            await _bookRepository.DeleteAsync(bookId);
        }

        public async Task ArchiveBookAsync(string bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new InvalidOperationException("Book not found.");

            book.Status = "Archived";
            book.UpdatedAt = DateTime.UtcNow;
            await _bookRepository.UpdateAsync(bookId, book);
        }

        public async Task UnarchiveBookAsync(string bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId)
                ?? throw new InvalidOperationException("Book not found.");

            book.Status = "Available";
            book.UpdatedAt = DateTime.UtcNow;
            await _bookRepository.UpdateAsync(bookId, book);
        }

        private static void Validate(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("ISBN is required.");
            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Author is required.");
            if (book.TotalCopies < 0)
                throw new ArgumentException("Total copies cannot be negative.");
            if (book.PublicationYear < 0 || book.PublicationYear > DateTime.Now.Year + 1)
                throw new ArgumentException("Publication year is invalid.");
        }
    }
}
