using ERMS.Models;

namespace ERMS.Services
{
    // Interface for user management service
    public interface IUserManagementService
    {
        Task<(bool Success, string ErrorMessage)> CreateEmployeeUserAsync(Employee employee, string password);
        Task<(bool Success, string UserId, List<string> Errors)> CreateIdentityUserAsync(Employee employee, string password);
    }
}
