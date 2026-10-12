using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly IGenericRepository<TaskTrack.Repo.Models.Task> _taskRepo;
        private readonly TaskmanagementDbEgrzContext _context;

        public TaskService(IGenericRepository<TaskTrack.Repo.Models.Task> taskRepo, TaskmanagementDbEgrzContext context)
        {
            _taskRepo = taskRepo;
            _context = context;
        }

        public async Task<IEnumerable<TaskDto>> GetAllActiveTasksAsync()
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.IsActive == true)
                .Select(TaskMappings.ToDto)
                .ToListAsync();
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.TaskId == id)
                .Select(TaskMappings.ToDto)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProjectAsync(int projectId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId && t.IsActive == true)
                .Select(TaskMappings.ToDto)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskDto>> SearchTasksAsync(string? title, int? status, int? priority, int? projectId, int? tagId)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Where(t => t.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                query = query.Where(t => EF.Functions.ILike(t.Title, $"%{term}%") || 
                                        (t.Description != null && EF.Functions.ILike(t.Description, $"%{term}%")));
            }
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);
            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);
            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);
            if (tagId.HasValue)
                query = query.Where(t => t.Tags.Any(tag => tag.TagId == tagId.Value));

            return await query
                .Select(TaskMappings.ToDto)
                .ToListAsync();
        }

        public async Task<TaskTrack.Repo.Models.Task> CreateTaskAsync(TaskTrack.Repo.Models.Task task, int[]? tagIds)
        {
            task.IsActive = true;
            task.CreatedDate = DateTime.Now;
            task.ModifiedDate = DateTime.Now;

            if (tagIds != null && tagIds.Any())
            {
                var tags = await _context.Tags.Where(t => tagIds.Contains(t.TagId)).ToListAsync();
                task.Tags = tags;
            }
            
            await _taskRepo.AddAsync(task);
            await _taskRepo.SaveChangesAsync();

            return task;
        }

        public async Task<TaskTrack.Repo.Models.Task?> UpdateTaskAsync(int id, TaskTrack.Repo.Models.Task updatedData, int[]? tagIds)
        {
            var task = await _context.Tasks.Include(t => t.Tags).FirstOrDefaultAsync(t => t.TaskId == id);
            if (task == null) return null;

            task.Title = updatedData.Title;
            task.Description = updatedData.Description;
            task.Status = updatedData.Status;
            task.Priority = updatedData.Priority;
            task.DueDate = updatedData.DueDate;
            task.ProjectId = updatedData.ProjectId;
            task.IsActive = updatedData.IsActive;
            task.ModifiedDate = DateTime.Now;

            if (tagIds != null)
            {
                var tags = await _context.Tags.Where(t => tagIds.Contains(t.TagId)).ToListAsync();
                task.Tags.Clear();
                foreach(var t in tags)
                {
                    task.Tags.Add(t);
                }
            }

            _taskRepo.Update(task);
            await _taskRepo.SaveChangesAsync();
            return task;
        }

        public async Task<bool> SoftDeleteTaskAsync(int id)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null) return false;

            task.IsActive = false;
            task.ModifiedDate = DateTime.Now;
            _taskRepo.Update(task);
            await _taskRepo.SaveChangesAsync();
            return true;
        }
    }
}
