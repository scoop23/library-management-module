using Google.Cloud.Firestore;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Condition values: "New", "Good", "Fair", "Damaged"
    /// Status values: "Available", "Borrowed", "Reserved", "Lost", "Damaged"
    /// (Status transitions beyond "Available" are driven by the Borrow/Reservation
    /// modules, which are integrated separately from this Book Management module.)
    /// </summary>
    [FirestoreData]
    public class BookCopy
    {
        [FirestoreDocumentId]
        public string CopyId { get; set; }

        [FirestoreProperty]
        public string BookId { get; set; }

        [FirestoreProperty]
        public string Condition { get; set; } = "New";

        [FirestoreProperty]
        public string Status { get; set; } = "Available";

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }
    }
}
