using Archi.Library.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Archi.Library.Tests.Controllers;

public class BaseControllerDeleteTests
{
    private (FakeController c, FakeDbContext ctx) Setup(int count = 1)
    {
        var ctx = count > 0 ? TestFactory.CreateContextWithData(count) : TestFactory.CreateInMemoryContext();
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);
        return (c, ctx);
    }

    [Fact]
    public void Delete_ExistingEntity_ReturnsNoContent()
    {
        var (c, ctx) = Setup();
        c.Delete(ctx.Fakes.First().Id).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_SetsIsDeletedTrue()
    {
        var (c, ctx) = Setup();
        int id = ctx.Fakes.First().Id;
        c.Delete(id);
        ctx.Fakes.Find(id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_SetsDeletedAt()
    {
        var (c, ctx) = Setup();
        int id = ctx.Fakes.First().Id;
        c.Delete(id);
        ctx.Fakes.Find(id)!.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Delete_EntityRemainsPhysicallyInDatabase()
    {
        var (c, ctx) = Setup();
        int id = ctx.Fakes.First().Id;
        c.Delete(id);
        ctx.Fakes.Find(id).Should().NotBeNull();
    }

    [Fact]
    public void Delete_UnknownId_ReturnsNotFound()
    {
        var (c, _) = Setup(0);
        c.Delete(9999).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Delete_ThenGetById_ReturnsNotFound()
    {
        var (c, ctx) = Setup();
        int id = ctx.Fakes.First().Id;
        c.Delete(id);
        c.GetById(id).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Delete_ThenGetAll_ExcludesDeletedItem()
    {
        var (c, ctx) = Setup(3);
        int id = ctx.Fakes.First().Id;
        c.Delete(id);

        var items = ((c.GetAll().Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().NotContain(x => x.Id == id);
        items.Should().HaveCount(2);
    }
}