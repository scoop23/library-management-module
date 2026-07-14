using System.Net.Http.Json;
using System.Text.Json;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    /// <summary>
    /// Fetches student data from an external Student Management System via REST API.
    /// Configure the base URL in Configurations/appsettings.json or pass it in the constructor.
    /// Expected API endpoints:
    ///   GET {baseUrl}/api/students           → list all students
    ///   GET {baseUrl}/api/students/{id}      → get student by ID
    ///   GET {baseUrl}/api/students/search?q= → search by name/number
    /// </summary>
    public class ExternalStudentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ExternalStudentService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public ExternalStudentService() : this("http://localhost:5000") { }

        public async Task<Student> GetStudentByIdAsync(string studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/students/{studentId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Student>();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<Student> GetStudentByNumberAsync(string studentNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/students/search?q={Uri.EscapeDataString(studentNumber)}");
                response.EnsureSuccessStatusCode();
                var students = await response.Content.ReadFromJsonAsync<List<Student>>();
                return students?.FirstOrDefault(s => s.StudentNumber == studentNumber);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<List<Student>> SearchStudentsAsync(string keyword)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/students/search?q={Uri.EscapeDataString(keyword)}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Student>>() ?? new List<Student>();
            }
            catch (HttpRequestException)
            {
                return new List<Student>();
            }
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/students");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Student>>() ?? new List<Student>();
            }
            catch (HttpRequestException)
            {
                return new List<Student>();
            }
        }
    }
}
