using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using TaskTrack.Repo.Models;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _projectService.GetAllActiveProjectsAsync();
            return Ok(data);
        }

        [Authorize]
        [HttpGet("manage")]
        public async Task<IActionResult> GetAllForManagement()
        {
            var data = await _projectService.GetAllProjectsForManagementAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _projectService.GetProjectByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var data = await _projectService.GetProjectsByDepartmentAsync(departmentId);
            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] int? status, [FromQuery] int? departmentId)
        {
            var data = await _projectService.SearchProjectsAsync(name, status, departmentId);
            return Ok(data);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _projectService.CreateProjectAsync(project);
            return CreatedAtAction(nameof(GetById), new { id = created.ProjectId }, created);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project project)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _projectService.UpdateProjectAsync(id, project);
            if (updated == null) return NotFound(new { message = "Project not found." });
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);
                if (!result) return NotFound(new { message = "Project not found." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
