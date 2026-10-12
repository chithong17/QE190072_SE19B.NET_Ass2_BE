using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Service.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllActiveDepartmentsAsync();
        Task<IEnumerable<Department>> GetAllDepartmentsForManagementAsync();
        Task<Department?> GetDepartmentByIdAsync(int id);
        Task<IEnumerable<Department>> SearchDepartmentsByNameAsync(string name);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<Department?> UpdateDepartmentAsync(int id, Department department);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}
