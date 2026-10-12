using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Repo.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string DepartmentName { get; set; } = null!;

    [Required(ErrorMessage = "Department description is required.")]
    [StringLength(300, ErrorMessage = "Department description cannot exceed 300 characters.")]
    public string DepartmentDescription { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
