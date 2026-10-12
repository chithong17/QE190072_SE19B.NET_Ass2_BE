using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using TaskTrack.Repo.Models;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _taskService.GetAllActiveTasksAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _taskService.GetTaskByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var data = await _taskService.GetTasksByProjectAsync(projectId);
            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? title, [FromQuery] int? status, [FromQuery] int? priority, [FromQuery] int? projectId, [FromQuery] int? tagId)
        {
            var data = await _taskService.SearchTasksAsync(title, status, priority, projectId, tagId);
            return Ok(data);
        }

        public class CreateTaskDto
        {
            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Task data is required.")]
            public TaskTrack.Repo.Models.Task Task { get; set; } = null!;
            public int[]? TagIds { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            if (dto?.Task == null) return BadRequest(new { message = "Task data is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("AccountId");
            if (int.TryParse(accountIdClaim, out var currentUserId))
            {
                dto.Task.CreatedById = currentUserId;
                dto.Task.UpdatedById = currentUserId;
            }

            var created = await _taskService.CreateTaskAsync(dto.Task, dto.TagIds);
            var result = await _taskService.GetTaskByIdAsync(created.TaskId);
            return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, result ?? (object)created);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTaskDto dto)
        {
            if (dto?.Task == null) return BadRequest(new { message = "Task data is required." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("AccountId");
            if (int.TryParse(accountIdClaim, out var currentUserId))
            {
                dto.Task.UpdatedById = currentUserId;
            }

            var updated = await _taskService.UpdateTaskAsync(id, dto.Task, dto.TagIds);
            if (updated == null) return NotFound(new { message = "Task not found." });
            var result = await _taskService.GetTaskByIdAsync(updated.TaskId);
            return Ok(result ?? (object)updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.SoftDeleteTaskAsync(id);
            if (!result) return NotFound(new { message = "Task not found." });
            return NoContent();
        }
    }
}
