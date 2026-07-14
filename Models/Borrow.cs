using Google.Cloud.Firestore;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Status values: "Borrowed", "Returned", "Overdue", "Lost"
    /// </summary>
    [FirestoreData]
    public class Borrow
    {
        [FirestoreDocumentId]
        public string BorrowId { get; set; }

        [FirestoreProperty]
        public string StudentId { get; set; }

        [FirestoreProperty]
        public string StudentName { get; set; }

        [FirestoreProperty]
        public string BookId { get; set; }

        [FirestoreProperty]
        public string BookTitle { get; set; }

        [FirestoreProperty]
        public string CopyId { get; set; }

        [FirestoreProperty]
        public Timestamp BorrowDate { get; set; }

        [FirestoreProperty]
        public Timestamp DueDate { get; set; }

        [FirestoreProperty]
        public Timestamp? ReturnDate { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } = "Borrowed";

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty]
        public Timestamp UpdatedAt { get; set; }
    }
}
