using ERMS.Models;

// Added interfaces for EmployeeApiService
// and TaskItemApiService to keep the code clean and maintainable.
// it also helps me mock the services in the tests. (Avoiding Real HTTP Calls)
namespace ERMS.Services
{
    public interface IEmployeeApiService
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Employee employee);
        Task<bool> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(int id);
    }
}
