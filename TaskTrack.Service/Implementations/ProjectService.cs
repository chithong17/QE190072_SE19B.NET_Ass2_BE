using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IGenericRepository<Project> _projectRepo;
        private readonly TaskmanagementDbEgrzContext _context;

        public ProjectService(IGenericRepository<Project> projectRepo, TaskmanagementDbEgrzContext context)
        {
            _projectRepo = projectRepo;
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllActiveProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.Department)
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsForManagementAsync()
        {
            return await _context.Projects
                .Include(p => p.Department)
                .OrderBy(p => p.ProjectId)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.ProjectId == id);
        }

        public async Task<IEnumerable<Project>> GetProjectsByDepartmentAsync(int departmentId)
        {
            return await _context.Projects
                .Include(p => p.Department)
                .Where(p => p.DepartmentId == departmentId && p.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> SearchProjectsAsync(string? name, int? status, int? departmentId)
        {
            var query = _context.Projects.Include(p => p.Department).Where(p => p.IsActive == true).AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = name.Trim();
                query = query.Where(p => EF.Functions.ILike(p.ProjectName, $"%{term}%"));
            }
            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);
            if (departmentId.HasValue)
                query = query.Where(p => p.DepartmentId == departmentId.Value);

            return await query.ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.IsActive = true;
            project.CreatedDate = DateTime.Now;
            await _projectRepo.AddAsync(project);
            await _projectRepo.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> UpdateProjectAsync(int id, Project updatedData)
        {
            var proj = await _projectRepo.GetByIdAsync(id);
            if (proj == null) return null;

            proj.ProjectName = updatedData.ProjectName;
            proj.Description = updatedData.Description;
            proj.StartDate = updatedData.StartDate;
            proj.EndDate = updatedData.EndDate;
            proj.Status = updatedData.Status;
            proj.DepartmentId = updatedData.DepartmentId;
            proj.IsActive = updatedData.IsActive;

            _projectRepo.Update(proj);
            await _projectRepo.SaveChangesAsync();
            return proj;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var proj = await _context.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.ProjectId == id);
            if (proj == null) return false;

            if (proj.Tasks != null && proj.Tasks.Any())
            {
                throw new InvalidOperationException("Cannot delete project because it has linked tasks.");
            }

            _projectRepo.Delete(proj);
            await _projectRepo.SaveChangesAsync();
            return true;
        }
    }
}
