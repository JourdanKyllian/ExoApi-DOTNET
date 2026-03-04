using Archi.API.Controllers;
using Archi.API.Data;
using Archi.API.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Archi.Library.Tests.Helpers;

namespace Archi.API.Tests.Controllers;


public class PizzaControllerTests
{
    private static (PizzaController controller, ArchiDbContext ctx) CreateSetup()
    {
        var ctx = TestFactory.CreateArchiInMemoryContext();
        var controller = new PizzaController(ctx);
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
        ((result.Result as OkObjectResult)!.Value as IEnumerable<PizzaModel>)!.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_OnlyReturnsNonDeletedPizzas()
    {
        var (controller, ctx) = CreateSetup();
        ctx.Pizza.AddRange(
            new PizzaModel { Name = "Active", Ingredients = "Mozza", Price = 10m },
            new PizzaModel { Name = "Deleted", Ingredients = "Mozza", Price = 10m, IsDeleted = true }
        );
        ctx.SaveChanges();

        var result = controller.GetAll();
        var items = ((result.Result as OkObjectResult)!.Value as IEnumerable<PizzaModel>)!.ToList();

        items.Should().HaveCount(1);
        items[0].Name.Should().Be("Active");
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsOkWithPizza()
    {
        var (controller, ctx) = CreateSetup();
        var pizza = new PizzaModel { Name = "Napoli", Ingredients = "Tomate, Mozza", Price = 11m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        var result = controller.GetById(pizza.Id);

        result.Result.Should().BeOfType<OkObjectResult>();
        ((result.Result as OkObjectResult)!.Value as PizzaModel)!.Name.Should().Be("Napoli");
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNotFound()
    {
        var (controller, _) = CreateSetup();
        controller.GetById(9999).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetById_SoftDeletedPizza_ReturnsNotFound()
    {
        var (controller, ctx) = CreateSetup();
        var pizza = new PizzaModel { Name = "Dead", Ingredients = "Mozza", Price = 10m, IsDeleted = true };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        controller.GetById(pizza.Id).Result.Should().BeOfType<NotFoundResult>();
    }

    // ── Post ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Post_ValidPizza_ReturnsCreated()
    {
        var (controller, _) = CreateSetup();
        var result = controller.Post(new PizzaModel { Name = "4 Fromages", Ingredients = "Mozza, Gorgonzola, Comté", Price = 13m });
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public void Post_ValidPizza_SavesInDatabase()
    {
        var (controller, ctx) = CreateSetup();
        controller.Post(new PizzaModel { Name = "Sauvegardée", Ingredients = "Mozza", Price = 11m });
        ctx.Pizza.Should().ContainSingle(p => p.Name == "Sauvegardée");
    }

    [Fact]
    public void Post_InvalidModelState_ReturnsBadRequest()
    {
        var (controller, _) = CreateSetup();
        controller.ModelState.AddModelError("Name", "Required");
        var result = controller.Post(new PizzaModel { Name = "", Ingredients = "Mozza", Price = 10m });
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Put ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Put_ValidUpdate_ReturnsNoContent()
    {
        var (controller, ctx) = CreateSetup();
        var pizza = new PizzaModel { Name = "Avant", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        pizza.Name = "Après";
        controller.Put(pizza.Id, pizza).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Put_ValidUpdate_PersistsChanges()
    {
        var (controller, ctx) = CreateSetup();
        var pizza = new PizzaModel { Name = "Ancien", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        pizza.Name = "Nouveau";
        controller.Put(pizza.Id, pizza);

        ctx.Pizza.Find(pizza.Id)!.Name.Should().Be("Nouveau");
    }

    [Fact]
    public void Put_UnknownId_ReturnsNotFound()
    {
        var (controller, _) = CreateSetup();
        var entity = new PizzaModel { Id = 9999, Name = "Ghost", Ingredients = "Mozza", Price = 10m };
        controller.Put(9999, entity).Should().BeOfType<NotFoundResult>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingPizza_ReturnsNoContent()
    {
        var (controller, ctx) = CreateSetup();
        var pizza = new PizzaModel { Name = "À supprimer", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        controller.Delete(pizza.Id).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_SoftDeletesPizza()
    {
        var (controller, ctx) = CreateSetup();
        var pizza = new PizzaModel { Name = "Soft", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        controller.Delete(pizza.Id);

        ctx.Pizza.Find(pizza.Id)!.IsDeleted.Should().BeTrue();
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
        var pizza = new PizzaModel { Name = "Invisible", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        controller.Delete(pizza.Id);
        controller.GetById(pizza.Id).Result.Should().BeOfType<NotFoundResult>();
    }
}