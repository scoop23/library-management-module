using Google.Cloud.Firestore;

namespace LibraryManagementSystem.Models
{
    [FirestoreData]
    public class Category
    {
        [FirestoreDocumentId]
        public string CategoryId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }
    }
}
