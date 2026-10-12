using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Repo.Models;

public partial class Task
{
    public int TaskId { get; set; }

    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(300, ErrorMessage = "Task title cannot exceed 300 characters.")]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Range(0, 3, ErrorMessage = "Status must be between 0 (To Do) and 3 (Cancelled).")]
    public short Status { get; set; }

    [Range(0, 3, ErrorMessage = "Priority must be between 0 (Low) and 3 (Critical).")]
    public short Priority { get; set; }

    public DateOnly? DueDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Project is required.")]
    public int ProjectId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? CreatedById { get; set; }

    public int? UpdatedById { get; set; }

    public virtual Project? Project { get; set; }

    public virtual SystemAccount? CreatedBy { get; set; }

    public virtual SystemAccount? UpdatedBy { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
