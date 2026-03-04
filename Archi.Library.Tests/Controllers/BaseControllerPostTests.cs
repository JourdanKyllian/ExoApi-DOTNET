using Archi.Library.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Archi.Library.Tests.Controllers;

public class BaseControllerPostTests
{
    private (FakeController c, FakeDbContext ctx) Setup()
    {
        var ctx = TestFactory.CreateInMemoryContext();
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);
        return (c, ctx);
    }

    [Fact]
    public void Post_ValidEntity_ReturnsCreated()
    {
        var (c, _) = Setup();
        c.Post(new FakeModel { Label = "New", Price = 42 }).Result
            .Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public void Post_ValidEntity_SavesInDatabase()
    {
        var (c, ctx) = Setup();
        c.Post(new FakeModel { Label = "Saved", Price = 15 });
        ctx.Fakes.Should().ContainSingle(x => x.Label == "Saved");
    }

    [Fact]
    public void Post_ValidEntity_ReturnsEntityWithGeneratedId()
    {
        var (c, _) = Setup();
        var result = c.Post(new FakeModel { Label = "WithId", Price = 99 });
        ((result.Result as CreatedAtActionResult)!.Value as FakeModel)!.Id
            .Should().BeGreaterThan(0);
    }

    [Fact]
    public void Post_InvalidModelState_ReturnsBadRequest()
    {
        var (c, _) = Setup();
        c.ModelState.AddModelError("Label", "Required");
        c.Post(new FakeModel { Label = "", Price = 0 }).Result
            .Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Post_MultipleEntities_EachGetsUniqueId()
    {
        var (c, _) = Setup();
        var id1 = ((c.Post(new FakeModel { Label = "A", Price = 1 }).Result as CreatedAtActionResult)!.Value as FakeModel)!.Id;
        var id2 = ((c.Post(new FakeModel { Label = "B", Price = 2 }).Result as CreatedAtActionResult)!.Value as FakeModel)!.Id;
        id1.Should().NotBe(id2);
    }
}