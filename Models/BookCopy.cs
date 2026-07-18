namespace LibraryManagementSystem.Models
{
    public class BookCopy
    {
        public string CopyId { get; set; }
        public string BookId { get; set; }
        public string Condition { get; set; } = "New";
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; }
    }
}
