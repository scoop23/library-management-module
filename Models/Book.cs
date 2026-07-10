using Google.Cloud.Firestore;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Status values: "Available", "Unavailable", "Archived"
    /// </summary>
    [FirestoreData]
    public class Book
    {
        [FirestoreDocumentId]
        public string BookId { get; set; }

        [FirestoreProperty]
        public string ISBN { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public string Author { get; set; }

        [FirestoreProperty]
        public string Publisher { get; set; }

        [FirestoreProperty]
        public int PublicationYear { get; set; }

        [FirestoreProperty]
        public string CategoryId { get; set; }

        [FirestoreProperty]
        public string CategoryName { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public string CoverImageUrl { get; set; }

        [FirestoreProperty]
        public int TotalCopies { get; set; }

        [FirestoreProperty]
        public int AvailableCopies { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } = "Available";

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty]
        public Timestamp UpdatedAt { get; set; }
    }
}
