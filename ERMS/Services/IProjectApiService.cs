using ERMS.Models;

// Added interfaces for ProjectApiService
// and TaskItemApiService to keep the code clean and maintainable.
// it also helps me mock the services in the tests. (Avoiding Real HTTP Calls)
namespace ERMS.Services
{
    public interface IProjectApiService
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Project project);
        Task<bool> UpdateAsync(Project project);
        Task<bool> DeleteAsync(int id);
    }
}
