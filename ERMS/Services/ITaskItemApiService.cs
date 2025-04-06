using ERMS.Models;

// Added interfaces for TaskItemApiService
// and TaskItemApiService to keep the code clean and maintainable.
// it also helps me mock the services in the tests. (Avoiding Real HTTP Calls)
namespace ERMS.Services
{
    public interface ITaskItemApiService
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<bool> CreateAsync(TaskItem taskItem);
        Task<bool> UpdateAsync(TaskItem taskItem);
        Task<bool> DeleteAsync(int id);
    }
}
