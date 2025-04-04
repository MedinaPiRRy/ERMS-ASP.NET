using ERMS.Models;

namespace ERMS.Services
{
    public class EmployeeApiService
    {
        private readonly HttpClient _httpClient;

        public EmployeeApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Employee>>("api/EmployeesApi");
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Employee>($"api/EmployeesApi/{id}");
        }

        public async Task<bool> CreateAsync(Employee employee)
        {
            var response = await _httpClient.PostAsJsonAsync("api/EmployeesApi", employee);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/EmployeesApi/{employee.Id}", employee);

            // For debugging as PUT is giving the most issues
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API PUT Error: " + body);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/EmployeesApi/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
