using System;
using System.Collections.Generic;

namespace TaskTrack.Repo.Models;

public partial class SystemAccount
{
    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public short Role { get; set; } // 0 = Staff, 1 = Admin

    public DateTime CreatedDate { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<Task> TasksCreated { get; set; } = new List<Task>();

    public virtual ICollection<Task> TasksUpdated { get; set; } = new List<Task>();
}
