namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public string BookId { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int PublicationYear { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
