using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Service.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllActiveProjectsAsync();
        Task<IEnumerable<Project>> GetAllProjectsForManagementAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task<IEnumerable<Project>> GetProjectsByDepartmentAsync(int departmentId);
        Task<IEnumerable<Project>> SearchProjectsAsync(string? name, int? status, int? departmentId);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project?> UpdateProjectAsync(int id, Project project);
        Task<bool> DeleteProjectAsync(int id);
    }
}
