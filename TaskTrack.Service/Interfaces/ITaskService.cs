using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrack.Repo.Models;
using TaskTrack.Service.DTOs;

namespace TaskTrack.Service.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllActiveTasksAsync();
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskDto>> GetTasksByProjectAsync(int projectId);
        Task<IEnumerable<TaskDto>> SearchTasksAsync(string? title, int? status, int? priority, int? projectId, int? tagId);
        Task<TaskTrack.Repo.Models.Task> CreateTaskAsync(TaskTrack.Repo.Models.Task task, int[]? tagIds);
        Task<TaskTrack.Repo.Models.Task?> UpdateTaskAsync(int id, TaskTrack.Repo.Models.Task task, int[]? tagIds);
        Task<bool> SoftDeleteTaskAsync(int id);
    }
}
