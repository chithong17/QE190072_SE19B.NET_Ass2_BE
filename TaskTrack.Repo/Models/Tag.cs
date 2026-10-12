using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Repo.Models;

public partial class Tag
{
    public int TagId { get; set; }

    [Required(ErrorMessage = "Tag name is required.")]
    [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters.")]
    public string TagName { get; set; } = null!;

    [StringLength(7, ErrorMessage = "Color code cannot exceed 7 characters.")]
    [RegularExpression(@"^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})$", ErrorMessage = "Color must be a valid hex color code (e.g. #3B82F6).")]
    public string? Color { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
