using System.Net.Http.Json;
using ERMS.Models;

namespace ERMS.Services
{
    public class ProjectApiService
    {
        private readonly HttpClient _httpClient;

        public ProjectApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Project>>("api/ProjectsApi");
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Project>($"api/ProjectsApi/{id}");
        }

        public async Task<bool> CreateAsync(Project project)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ProjectsApi", project);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/ProjectsApi/{project.Id}", project);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API PUT Error: " + body);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ProjectsApi/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"DELETE failed: {response.StatusCode} - {error}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}
