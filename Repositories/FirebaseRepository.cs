using Google.Cloud.Firestore;
using LibraryManagementSystem.Firebase;

namespace LibraryManagementSystem.Repositories
{
    /// <summary>
    /// Generic Firestore CRUD repository. Concrete repositories (BookRepository,
    /// BookCopyRepository, CategoryRepository, ...) derive from this and add
    /// collection-specific queries.
    /// </summary>
    public class FirebaseRepository<T> : IFirebaseRepository<T> where T : class
    {
        protected readonly FirestoreDb Db;
        protected readonly CollectionReference Collection;

        public FirebaseRepository(string collectionName)
        {
            Db = FirebaseConfig.GetFirestoreDb();
            Collection = Db.Collection(collectionName);
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            var snapshot = await Collection.Document(id).GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<T>() : null;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            var snapshot = await Collection.GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
        }

        public virtual async Task<string> AddAsync(T entity)
        {
            var docRef = await Collection.AddAsync(entity);
            return docRef.Id;
        }

        public virtual async Task UpdateAsync(string id, T entity)
        {
            await Collection.Document(id).SetAsync(entity, SetOptions.Overwrite);
        }

        public virtual async Task DeleteAsync(string id)
        {
            await Collection.Document(id).DeleteAsync();
        }
    }
}
