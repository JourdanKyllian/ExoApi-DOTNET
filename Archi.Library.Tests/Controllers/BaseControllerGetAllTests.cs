using Archi.Library.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Archi.Library.Tests.Controllers;

public class BaseControllerGetAllTests
{
    private FakeController CreateController(int itemCount = 10)
    {
        var ctx = TestFactory.CreateContextWithData(itemCount);
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);
        return c;
    }

    // ── Statut HTTP ───────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_ReturnsOk()
    {
        CreateController(5).GetAll().Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetAll_EmptyDatabase_ReturnsOkWithEmptyList()
    {
        var ctx = TestFactory.CreateInMemoryContext();
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);
        ((c.GetAll().Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.Should().BeEmpty();
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_DefaultRange_Returns26Items_When30Exist()
    {
        var items = ((CreateController(30).GetAll().Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().HaveCount(26); // 0 à 25 inclus
    }

    [Fact]
    public void GetAll_DefaultRange_ReturnsAll_WhenLessThan26()
    {
        var items = ((CreateController(5).GetAll().Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().HaveCount(5);
    }

    [Fact]
    public void GetAll_CustomRange_ReturnsCorrectSlice()
    {
        var items = ((CreateController(20).GetAll("5-9").Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().HaveCount(5);
    }

    [Fact]
    public void GetAll_RangeExceedsMax_ClampsTo50()
    {
        var items = ((CreateController(100).GetAll("0-99").Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().HaveCountLessThanOrEqualTo(50);
    }

    [Fact]
    public void GetAll_ReturnsItemsOrderedById()
    {
        var items = ((CreateController(5).GetAll().Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().BeInAscendingOrder(x => x.Id);
    }

    [Fact]
    public void GetAll_ExcludesSoftDeletedItems()
    {
        var ctx = TestFactory.CreateInMemoryContext();
        ctx.Fakes.AddRange(
            new FakeModel { Label = "Visible", Price = 1 },
            new FakeModel { Label = "Deleted", Price = 2, IsDeleted = true }
        );
        ctx.SaveChanges();
        var c = new FakeController(ctx);
        TestFactory.SetupHttpContext(c);

        var items = ((c.GetAll().Result as OkObjectResult)!.Value as IEnumerable<FakeModel>)!.ToList();
        items.Should().HaveCount(1);
        items[0].Label.Should().Be("Visible");
    }

    // ── Headers ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_SetsContentRangeHeader()
    {
        var c = CreateController(10);
        c.GetAll("0-9");
        c.Response.Headers.Should().ContainKey("Content-Range");
        c.Response.Headers["Content-Range"].ToString().Should().MatchRegex(@"^\d+-\d+/\d+$");
    }

    [Fact]
    public void GetAll_SetsAcceptRangeHeader_WithModelNameAndLimit()
    {
        var c = CreateController(5);
        c.GetAll();
        var header = c.Response.Headers["Accept-Range"].ToString();
        header.Should().Contain("FakeModel");
        header.Should().Contain("50");
    }

    [Fact]
    public void GetAll_SetsLinkHeader_WithFirstAndLast()
    {
        var c = CreateController(20);
        c.GetAll("0-9");
        var link = c.Response.Headers["Link"].ToString();
        link.Should().Contain("rel=\"first\"");
        link.Should().Contain("rel=\"last\"");
    }

    [Fact]
    public void GetAll_HasNextLink_WhenMoreItemsExist()
    {
        var c = CreateController(20);
        c.GetAll("0-4");
        c.Response.Headers["Link"].ToString().Should().Contain("rel=\"next\"");
    }

    [Fact]
    public void GetAll_HasNoPrevLink_OnFirstPage()
    {
        var c = CreateController(20);
        c.GetAll("0-4");
        c.Response.Headers["Link"].ToString().Should().NotContain("rel=\"prev\"");
    }

    [Fact]
    public void GetAll_HasPrevLink_WhenNotOnFirstPage()
    {
        var c = CreateController(20);
        c.GetAll("5-9");
        c.Response.Headers["Link"].ToString().Should().Contain("rel=\"prev\"");
    }
}