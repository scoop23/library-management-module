using System.Text.Json;
using Firebase.Database;
using Firebase.Database.Query;

namespace LibraryManagementSystem.Repositories
{
    public class FirebaseRepository<T> : IFirebaseRepository<T> where T : class
    {
        protected static FirebaseClient Client;
        protected readonly string CollectionName;

        public FirebaseRepository(string collectionName)
        {
            CollectionName = collectionName;

            if (Client == null)
            {
                var configPath = Path.Combine(AppContext.BaseDirectory, "Configurations", "appsettings.json");
                var configJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<JsonElement>(configJson);

                var rtdbUrl = config.GetProperty("FirebaseRealtime").GetProperty("DatabaseUrl").GetString();
                var secret = config.GetProperty("FirebaseRealtime").GetProperty("DatabaseSecret").GetString();

                Client = new FirebaseClient(rtdbUrl, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(secret),
                    AsAccessToken = false
                });
            }
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            var result = await Client.Child(CollectionName).Child(id).OnceSingleAsync<T>();
            return result;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            var records = await Client.Child(CollectionName).OnceAsync<T>();
            return records.Select(r =>
            {
                var obj = r.Object;
                var idProp = typeof(T).GetProperty("BookId")
                    ?? typeof(T).GetProperty("CopyId")
                    ?? typeof(T).GetProperty("CategoryId")
                    ?? typeof(T).GetProperty("BorrowId")
                    ?? typeof(T).GetProperty("StudentId");

                if (idProp != null && idProp.CanWrite)
                    idProp.SetValue(obj, r.Key);

                return obj;
            }).ToList();
        }

        public virtual async Task<string> AddAsync(T entity)
        {
            var result = await Client.Child(CollectionName).PostAsync(entity);
            return result.Key;
        }

        public virtual async Task UpdateAsync(string id, T entity)
        {
            await Client.Child(CollectionName).Child(id).PutAsync(entity);
        }

        public virtual async Task DeleteAsync(string id)
        {
            await Client.Child(CollectionName).Child(id).DeleteAsync();
        }
    }
}
