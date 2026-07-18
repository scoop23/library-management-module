namespace LibraryManagementSystem.Models
{
    public class Borrow
    {
        public string BorrowId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public string CopyId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Borrowed";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
