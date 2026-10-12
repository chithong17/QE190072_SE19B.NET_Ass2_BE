using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Repo.Models;

public partial class Project : IValidatableObject
{
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters.")]
    public string ProjectName { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Range(0, 3, ErrorMessage = "Status must be between 0 (Not Started) and 3 (On Hold).")]
    public short Status { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Department is required.")]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate.HasValue && EndDate.Value < StartDate)
        {
            yield return new ValidationResult("End date cannot be earlier than start date.", new[] { nameof(EndDate) });
        }
    }
}
