using System.Linq.Expressions;
using TaskEntity = TaskTrack.Repo.Models.Task;

namespace TaskTrack.Service.DTOs;

public sealed class TaskDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short Status { get; set; }
    public short Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public int ProjectId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public ProjectSummaryDto Project { get; set; } = null!;
    public List<TagSummaryDto> Tags { get; set; } = [];
}

public sealed class ProjectSummaryDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
}

public sealed class TagSummaryDto
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public static class TaskMappings
{
    public static readonly Expression<Func<TaskEntity, TaskDto>> ToDto = task => new TaskDto
    {
        TaskId = task.TaskId,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        ProjectId = task.ProjectId,
        IsActive = task.IsActive,
        CreatedDate = task.CreatedDate,
        ModifiedDate = task.ModifiedDate,
        Project = task.Project == null ? new ProjectSummaryDto { ProjectId = task.ProjectId, ProjectName = string.Empty } : new ProjectSummaryDto
        {
            ProjectId = task.Project.ProjectId,
            ProjectName = task.Project.ProjectName
        },
        Tags = task.Tags.Select(tag => new TagSummaryDto
        {
            TagId = tag.TagId,
            TagName = tag.TagName,
            Color = tag.Color
        }).ToList()
    };
}
