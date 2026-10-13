using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Implementations;
using Xunit;
using TaskEntity = TaskTrack.Repo.Models.Task;

namespace TaskTrack.Tests;

public class AuditAndProfileTests
{
    private static TaskmanagementDbEgrzContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TaskmanagementDbEgrzContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TaskmanagementDbEgrzContext(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateProfile_ChangesOnlyCurrentAccountName()
    {
        await using var context = CreateContext();
        context.SystemAccounts.Add(new SystemAccount
        {
            AccountId = 7,
            FullName = "Old Name",
            Email = "staff@example.com",
            PasswordHash = "hash",
            Role = 0,
            CreatedDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = new AuthService(context, new JwtSettings { Secret = new string('s', 32) });

        var result = await service.UpdateProfileAsync(7, new UpdateProfileRequestDto { FullName = "  New Name  " });

        Assert.True(result.Success);
        Assert.Equal("New Name", result.Account?.FullName);
        Assert.Equal("New Name", (await context.SystemAccounts.FindAsync(7))?.FullName);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTask_PersistsUpdatedByAuditField()
    {
        await using var context = CreateContext();
        context.Tasks.Add(new TaskEntity { TaskId = 1, Title = "Before", ProjectId = 1, IsActive = true });
        await context.SaveChangesAsync();
        var service = new TaskService(new GenericRepository<TaskEntity>(context), context);

        await service.UpdateTaskAsync(1, new TaskEntity
        {
            Title = "After",
            ProjectId = 1,
            IsActive = true,
            UpdatedById = 42
        }, []);

        var stored = await context.Tasks.FindAsync(1);
        Assert.Equal("After", stored?.Title);
        Assert.Equal(42, stored?.UpdatedById);
    }

    [Fact]
    public async System.Threading.Tasks.Task SoftDeleteTask_PersistsActorAndTimestamp()
    {
        await using var context = CreateContext();
        context.Tasks.Add(new TaskEntity { TaskId = 2, Title = "Delete me", ProjectId = 1, IsActive = true });
        await context.SaveChangesAsync();
        var service = new TaskService(new GenericRepository<TaskEntity>(context), context);

        Assert.True(await service.SoftDeleteTaskAsync(2, 99));

        var stored = await context.Tasks.FindAsync(2);
        Assert.False(stored?.IsActive);
        Assert.Equal(99, stored?.UpdatedById);
        Assert.NotNull(stored?.ModifiedDate);
    }
}
