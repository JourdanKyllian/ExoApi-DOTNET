using Archi.API.Controllers;
using Archi.API.Data;
using Archi.API.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Archi.Library.Tests.Helpers;

namespace Archi.API.Tests.Controllers;

public class TacosControllerTests
{
    private static (TacosController controller, ArchiDbContext ctx) CreateSetup()
    {
        var ctx = TestFactory.CreateArchiInMemoryContext();
        var controller = new TacosController(ctx);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return (controller, ctx);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_EmptyDatabase_ReturnsOkWithEmptyList()
    {
        var (controller, _) = CreateSetup();
        var result = controller.GetAll();
        result.Result.Should().BeOfType<OkObjectResult>();
        var items = ((result.Result as OkObjectResult)!.Value as IEnumerable<TacosModel>)!;
        items.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_OnlyReturnsNonDeletedTacos()
    {
        var (controller, ctx) = CreateSetup();
        ctx.Tacos.AddRange(
            new TacosModel { Name = "Visible", Meat = "Poulet", Price = 7m },
            new TacosModel { Name = "Supprimé", Meat = "Boeuf", Price = 8m, IsDeleted = true }
        );
        ctx.SaveChanges();

        var result = controller.GetAll();
        var items = ((result.Result as OkObjectResult)!.Value as IEnumerable<TacosModel>)!.ToList();

        items.Should().HaveCount(1);
        items[0].Name.Should().Be("Visible");
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsOkWithTacos()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "Royal Cheese", Meat = "Poulet", Price = 8.5m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        var result = controller.GetById(tacos.Id);

        result.Result.Should().BeOfType<OkObjectResult>();
        ((result.Result as OkObjectResult)!.Value as TacosModel)!.Name.Should().Be("Royal Cheese");
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNotFound()
    {
        var (controller, _) = CreateSetup();
        controller.GetById(9999).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetById_SoftDeletedTacos_ReturnsNotFound()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "Dead", Meat = "Poulet", Price = 5m, IsDeleted = true };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        controller.GetById(tacos.Id).Result.Should().BeOfType<NotFoundResult>();
    }

    // ── Post ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Post_ValidTacos_ReturnsCreated()
    {
        var (controller, _) = CreateSetup();
        var result = controller.Post(new TacosModel { Name = "Tacos Géant", Meat = "Boeuf", Price = 9m });
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public void Post_ValidTacos_SavesInDatabase()
    {
        var (controller, ctx) = CreateSetup();
        controller.Post(new TacosModel { Name = "Sauvegardé", Meat = "Dinde", Price = 6m });
        ctx.Tacos.Should().ContainSingle(t => t.Name == "Sauvegardé");
    }

    [Fact]
    public void Post_InvalidModelState_ReturnsBadRequest()
    {
        var (controller, _) = CreateSetup();
        controller.ModelState.AddModelError("Name", "Required");
        var result = controller.Post(new TacosModel { Name = "", Meat = "Poulet", Price = 7m });
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Put ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Put_ValidUpdate_ReturnsNoContent()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "Avant", Meat = "Poulet", Price = 6m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        tacos.Name = "Après";
        controller.Put(tacos.Id, tacos).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Put_ValidUpdate_PersistsChanges()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "Ancien", Meat = "Poulet", Price = 6m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        tacos.Name = "Nouveau";
        controller.Put(tacos.Id, tacos);

        ctx.Tacos.Find(tacos.Id)!.Name.Should().Be("Nouveau");
    }

    [Fact]
    public void Put_UnknownId_ReturnsNotFound()
    {
        var (controller, _) = CreateSetup();
        var entity = new TacosModel { Id = 9999, Name = "Ghost", Meat = "Poulet", Price = 5m };
        controller.Put(9999, entity).Should().BeOfType<NotFoundResult>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingTacos_ReturnsNoContent()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "À supprimer", Meat = "Dinde", Price = 7m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        controller.Delete(tacos.Id).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_SoftDeletesTacos()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "Soft", Meat = "Boeuf", Price = 7m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        controller.Delete(tacos.Id);

        ctx.Tacos.Find(tacos.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_UnknownId_ReturnsNotFound()
    {
        var (controller, _) = CreateSetup();
        controller.Delete(9999).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Delete_ThenGetById_ReturnsNotFound()
    {
        var (controller, ctx) = CreateSetup();
        var tacos = new TacosModel { Name = "Invisible", Meat = "Poulet", Price = 7m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        controller.Delete(tacos.Id);
        controller.GetById(tacos.Id).Result.Should().BeOfType<NotFoundResult>();
    }
}