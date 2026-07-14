using Google.Cloud.Firestore;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// Status values: "Active", "Inactive", "Graduated", "Suspended"
    /// </summary>
    [FirestoreData]
    public class Student
    {
        [FirestoreDocumentId]
        public string StudentId { get; set; }

        [FirestoreProperty]
        public string StudentNumber { get; set; }

        [FirestoreProperty]
        public string FullName { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string Department { get; set; }

        [FirestoreProperty]
        public int YearLevel { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } = "Active";

        [FirestoreProperty]
        public Timestamp EnrolledAt { get; set; }

        [FirestoreProperty]
        public Timestamp UpdatedAt { get; set; }
    }
}
