using System.Text.Json;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;
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
            var credentialsPath = config.GetProperty("FirebaseRealtime").GetProperty("CredentialsPath").GetString();

            var fullPath = Path.Combine(AppContext.BaseDirectory, credentialsPath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"it332 service account not found at '{fullPath}'.");

            var credential = GoogleCredential.FromFile(fullPath)
                .CreateScoped("https://www.googleapis.com/auth/firebase.database");

            var accessToken = credential.UnderlyingCredential
                .GetAccessTokenForRequestAsync().GetAwaiter().GetResult();

            _firebaseClient = new FirebaseClient(rtdbUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(accessToken),
                AsAccessToken = false
            });
        }

        public async Task<Student> GetStudentByIdAsync(string studentId)
        {
            var student = await _firebaseClient
                .Child(_baseURL)
                .Child(studentId)
                .OnceSingleAsync<Student>();

            if (student != null)
                student.StudentId = studentId;

            return student;
        }

        public async Task<Student> GetStudentByNumberAsync(string studentNumber)
        {
            var records = await _firebaseClient
                .Child(_baseURL)
                .OrderBy("StudentNumber")
                .EqualTo(studentNumber)
                .LimitToFirst(1)
                .OnceAsync<Student>();

            var match = records.FirstOrDefault();
            if (match != null)
                match.Object.StudentId = match.Key;

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
                    (item.Object.StudentNumber != null && item.Object.StudentNumber.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)))
                .Select(item =>
                {
                    item.Object.StudentId = item.Key;
                    return item.Object;
                })
                .ToList();
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var records = await _firebaseClient
                .Child(_baseURL)
                .OnceAsync<Student>();

            return records.Select(item =>
            {
                var student = item.Object;
                student.StudentId = item.Key;
                return student;
            }).ToList();
        }
    }
}
