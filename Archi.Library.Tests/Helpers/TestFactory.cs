using Archi.Api.Models;
using Archi.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Archi.Library.Tests.Helpers;

public class FakeModel : BaseModel
{
    [Required]
    public string Label { get; set; } = string.Empty;

    [Range(0, 1000)]
    public decimal Price { get; set; }
}

public class FakeDbContext : BaseDbContext
{
    public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options) { }
    public DbSet<FakeModel> Fakes { get; set; }
}

public static class TestFactory
{
    public static FakeDbContext CreateInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<FakeDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new FakeDbContext(options);
    }

    public static ArchiDbContext CreateArchiInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ArchiDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new ArchiDbContext(options);
    }

    public static void SetupHttpContext(ControllerBase controller)
    {
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    public static FakeDbContext CreateContextWithData(int count = 5, string? dbName = null)
    {
        var ctx = CreateInMemoryContext(dbName ?? Guid.NewGuid().ToString());
        for (int i = 1; i <= count; i++)
        {
            ctx.Fakes.Add(new FakeModel { Label = $"Item {i}", Price = i * 10 });
        }
        ctx.SaveChanges();
        return ctx;
    }
}
