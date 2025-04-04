using System.Net.Http.Json;
using ERMS.Models;

namespace ERMS.Services
{
    public class TaskItemApiService
    {
        private readonly HttpClient _httpClient;

        public TaskItemApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TaskItem>>("api/TaskItemsApi");
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TaskItem>($"api/TaskItemsApi/{id}");
        }

        public async Task<bool> CreateAsync(TaskItem task)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TaskItemsApi", task);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(TaskItem task)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/TaskItemsApi/{task.Id}", task);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API PUT Error: " + body);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/TaskItemsApi/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
