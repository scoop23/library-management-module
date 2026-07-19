using System.Text.Json;
using Firebase.Database;
using Firebase.Database.Query;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class InternalStudentService
    {
        private readonly string _baseURL = "students";
        private readonly FirebaseClient _firebaseClient;

        public InternalStudentService()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "Configurations", "appsettings.json");
            var configJson = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<JsonElement>(configJson);

            var rtdbUrl = config.GetProperty("FirebaseRealtime").GetProperty("DatabaseUrl").GetString();
            var secret = config.GetProperty("FirebaseRealtime").GetProperty("DatabaseSecret").GetString();

            _firebaseClient = new FirebaseClient(rtdbUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(secret),
                AsAccessToken = false
            });
        }

        public async Task<Student> GetStudentByIdAsync(string studentId)
        {
            var student = await _firebaseClient
                .Child(_baseURL)
                .Child(studentId)
                .OnceSingleAsync<Student>();

            return student;
        }

        public async Task<Student> GetStudentByNumberAsync(string studentNumber)
        {
            var records = await _firebaseClient
                .Child(_baseURL)
                .OrderBy("StudentId")
                .EqualTo(studentNumber)
                .LimitToFirst(1)
                .OnceAsync<Student>();

            var match = records.FirstOrDefault();

            return match?.Object;
        }

        public async Task<List<Student>> SearchStudentAsync(string keyword)
        {
            var records = await _firebaseClient
                .Child(_baseURL)
                .OnceAsync<Student>();

            return records
                .Where(item =>
                    (item.Object.FirstName != null && item.Object.FirstName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (item.Object.LastName != null && item.Object.LastName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (item.Object.StudentId != null && item.Object.StudentId.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)))
                .Select(item => item.Object)
                .ToList();
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var records = await _firebaseClient
                .Child(_baseURL)
                .OnceAsync<Student>();

            return records.Select(item => item.Object).ToList();
        }

        public async Task<ClearanceRecord> GetClearanceAsync(string studentId)
        {
            var record = await _firebaseClient
                .Child("library/clearanceStatuses")
                .Child(studentId)
                .OnceSingleAsync<ClearanceRecord>();

            return record;
        }

        public async Task SaveClearanceAsync(ClearanceRecord record)
        {
            await _firebaseClient
                .Child("library/clearanceStatuses")
                .Child(record.StudentId)
                .PutAsync(record);
        }
    }
}
