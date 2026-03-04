using Archi.Library.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Archi.Library.Tests.Controllers;
using Xunit;

public class BaseControllerPutTests
{
    private (FakeController c, FakeDbContext ctx) Setup(int count = 1)
    {
        var ctx = count > 0 ? TestFactory.CreateContextWithData(count) : TestFactory.CreateInMemoryContext();
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);
        return (c, ctx);
    }

    [Fact]
    public void Put_ValidUpdate_ReturnsNoContent()
    {
        var (c, ctx) = Setup();
        var entity = ctx.Fakes.First();
        entity.Label = "Updated";
        c.Put(entity.Id, entity).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Put_ValidUpdate_PersistsChanges()
    {
        var (c, ctx) = Setup();
        var entity = ctx.Fakes.First();
        entity.Label = "Persisted";
        c.Put(entity.Id, entity);
        ctx.Fakes.Find(entity.Id)!.Label.Should().Be("Persisted");
    }

    [Fact]
    public void Put_MismatchedId_ReturnsBadRequest()
    {
        var (c, ctx) = Setup();
        var entity = ctx.Fakes.First();
        c.Put(9999, entity).Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public void Put_UnknownId_ReturnsNotFound()
    {
        var (c, _) = Setup(0);
        var entity = new FakeModel { Id = 9999, Label = "Ghost", Price = 0 };
        c.Put(9999, entity).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Put_InvalidModelState_ReturnsBadRequest()
    {
        var (c, ctx) = Setup();
        var entity = ctx.Fakes.First();
        c.ModelState.AddModelError("Label", "Required");
        c.Put(entity.Id, entity).Should().BeOfType<BadRequestObjectResult>();
    }
}