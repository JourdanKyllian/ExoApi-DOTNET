using Archi.Library.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Archi.Library.Tests.Controllers;

public class BaseControllerGetByIdTests
{
    private (FakeController c, FakeDbContext ctx) Setup(int count = 0)
    {
        var ctx = count > 0 ? TestFactory.CreateContextWithData(count) : TestFactory.CreateInMemoryContext();
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);
        return (c, ctx);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsOk()
    {
        var (c, ctx) = Setup(3);
        int id = ctx.Fakes.First().Id;
        c.GetById(id).Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetById_ExistingId_ReturnsCorrectEntity()
    {
        var (c, ctx) = Setup(3);
        int id = ctx.Fakes.First().Id;
        var entity = (c.GetById(id).Result as OkObjectResult)!.Value as FakeModel;
        entity!.Id.Should().Be(id);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNotFound()
    {
        var (c, _) = Setup();
        c.GetById(9999).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetById_SoftDeletedEntity_ReturnsNotFound()
    {
        var (c, ctx) = Setup();
        var entity = new FakeModel { Label = "Deleted", Price = 5, IsDeleted = true };
        ctx.Fakes.Add(entity);
        ctx.SaveChanges();
        c.GetById(entity.Id).Result.Should().BeOfType<NotFoundResult>();
    }
}