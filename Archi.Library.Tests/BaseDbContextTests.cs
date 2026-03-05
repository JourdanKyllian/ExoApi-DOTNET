using Archi.Api.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Archi.Library.Tests.DbContext
{
    public class BaseDbContextTests
    {
        private class TestModel : BaseModel { public string Label { get; set; } = ""; }

        private class TestDbContext : BaseDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options) { }
            public DbSet<TestModel> TestModels => Set<TestModel>();
        }

        private BaseDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BaseDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public void SaveChanges_ShouldSetCreatedAt_ForNewEntity()
        {
            var ctx = (TestDbContext)CreateContext();
            var entity = new TestModel { Label = "New" };
            ctx.TestModels.Add(entity);
            ctx.SaveChanges();
            entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void SaveChanges_ShouldSetUpdatedAt_ForModifiedEntity()
        {
            var ctx = (TestDbContext)CreateContext();
            var entity = new TestModel { Label = "Old" };
            ctx.TestModels.Add(entity);
            ctx.SaveChanges();

            entity.Label = "Updated";
            ctx.SaveChanges();
            entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void SaveChanges_ShouldSoftDeleteEntity()
        {
            var ctx = (TestDbContext)CreateContext();
            var entity = new TestModel { Label = "ToDelete" };
            ctx.TestModels.Add(entity);
            ctx.SaveChanges();

            ctx.TestModels.Remove(entity);
            ctx.SaveChanges();

            entity.IsDeleted.Should().BeTrue();
            entity.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            ctx.TestModels.Find(entity.Id).Should().NotBeNull();
        }
    }
}