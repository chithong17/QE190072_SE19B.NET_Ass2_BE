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
    public class DepartmentService : IDepartmentService
    {
        private readonly IGenericRepository<Department> _departmentRepo;
        private readonly TaskmanagementDbEgrzContext _context;

        public DepartmentService(IGenericRepository<Department> departmentRepo, TaskmanagementDbEgrzContext context)
        {
            _departmentRepo = departmentRepo;
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllActiveDepartmentsAsync()
        {
            return await _context.Departments.Where(d => d.IsActive == true).ToListAsync();
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsForManagementAsync()
        {
            return await _context.Departments
                .OrderBy(d => d.DepartmentId)
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Projects.Where(p => p.IsActive == true))
                    .ThenInclude(p => p.Tasks.Where(t => t.IsActive == true))
                        .ThenInclude(t => t.Tags)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
        }

        public async Task<IEnumerable<Department>> SearchDepartmentsByNameAsync(string name)
        {
            var term = name?.Trim();
            return await _context.Departments
                .Where(d => d.IsActive == true && (string.IsNullOrEmpty(term) || EF.Functions.ILike(d.DepartmentName, $"%{term}%")))
                .ToListAsync();
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            department.IsActive = true;
            await _departmentRepo.AddAsync(department);
            await _departmentRepo.SaveChangesAsync();
            return department;
        }

        public async Task<Department?> UpdateDepartmentAsync(int id, Department updatedData)
        {
            var dept = await _departmentRepo.GetByIdAsync(id);
            if (dept == null) return null;

            dept.DepartmentName = updatedData.DepartmentName;
            dept.DepartmentDescription = updatedData.DepartmentDescription;
            dept.IsActive = updatedData.IsActive;

            _departmentRepo.Update(dept);
            await _departmentRepo.SaveChangesAsync();
            return dept;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var dept = await _context.Departments.Include(d => d.Projects).FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (dept == null) return false;

            if (dept.Projects != null && dept.Projects.Any())
            {
                throw new InvalidOperationException("Cannot delete department because it has linked projects.");
            }

            _departmentRepo.Delete(dept);
            await _departmentRepo.SaveChangesAsync();
            return true;
        }
    }
}
