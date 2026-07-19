using System.Text.Json;
using Firebase.Database;
using Firebase.Database.Query;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class FirebaseRepository<T> : IFirebaseRepository<T> where T : class
    {
        protected static FirebaseClient client;
        protected readonly string CollectionName;

        private static string IdPropertyName => typeof(T) switch
        {
            var t when t == typeof(Book) => "BookId",
            var t when t == typeof(BookCopy) => "CopyId",
            var t when t == typeof(Category) => "CategoryId",
            var t when t == typeof(Borrow) => "BorrowId",
            var t when t == typeof(Student) => "StudentId",
            _ => "Id"
        };

        private static string Prefix => typeof(T) switch
        {
            var t when t == typeof(Book) => "BOK",
            var t when t == typeof(BookCopy) => "CPY",
            var t when t == typeof(Category) => "CAT",
            var t when t == typeof(Borrow) => "BRW",
            var t when t == typeof(Student) => "STU",
            _ => "ID"
        };

        public FirebaseRepository(string collectionName)
        {
            CollectionName = collectionName;
            if (client == null)
            {
                var configPath = Path.Combine(AppContext.BaseDirectory, "Configurations", "appsettings.json");
                var configJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<JsonElement>(configJson);

                var rtbdUrl = config.GetProperty("FirebaseRealtime").GetProperty("DatabaseUrl").ToString();
                var secret = config.GetProperty("FirebaseRealtime").GetProperty("DatabaseSecret").GetString();

                client = new FirebaseClient(rtbdUrl, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(secret),
                    AsAccessToken = false
                });
            }
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            var idProp = typeof(T).GetProperty(IdPropertyName);
            var records = await client.Child(CollectionName).OnceAsync<T>();
            return records.Select(item =>
            {
                var obj = item.Object;
                if (idProp != null && idProp.CanWrite)
                    idProp.SetValue(obj, item.Key);
                return obj;
            }).ToList();
        }

        public virtual async Task<T> GetByIdAsync(string id)
        {
            var result = await client.Child(CollectionName).Child(id).OnceSingleAsync<T>();
            return result;
        }

        public async Task<string> AddAsync(T entity)
        {
            int currentYear = DateTime.Now.Year;

            var lastRecordQuery = await client.Child(CollectionName)
                .OrderByKey()
                .LimitToLast(1)
                .OnceAsync<T>();
            int nextSequenceNumber = 1;

            if (lastRecordQuery != null && lastRecordQuery.Any())
            {
                string lastKey = lastRecordQuery.First().Key;
                string[] segments = lastKey.Split('-');
                if (segments.Length >= 3 && int.TryParse(segments[2], out int lastNumber))
                {
                    nextSequenceNumber = lastNumber + 1;
                }
            }

            var idProp = typeof(T).GetProperty(IdPropertyName);
            string structuredKey = idProp?.GetValue(entity)?.ToString();
            if (string.IsNullOrEmpty(structuredKey))
            {
                structuredKey = $"{Prefix}-{currentYear}-{nextSequenceNumber.ToString("D4")}";
                if (idProp != null && idProp.CanWrite)
                    idProp.SetValue(entity, structuredKey);
            }

            await client.Child(CollectionName).Child(structuredKey).PutAsync(entity);
            return structuredKey;
        }

        public async Task DeleteAsync(string id)
        {
            await client.Child(CollectionName).Child(id).DeleteAsync();
        }

        public async Task UpdateAsync(string id, T entity)
        {
            await client.Child(CollectionName).Child(id).PutAsync(entity);
        }
    }
}
