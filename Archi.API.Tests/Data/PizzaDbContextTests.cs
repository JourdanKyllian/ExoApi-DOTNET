using Archi.API.Models;
using Archi.Library.Controllers;
using FluentAssertions;
using Xunit;
using Archi.Library.Tests.Helpers;

namespace Archi.API.Tests.Data;

/// <summary>Tests sur les timestamps et soft delete liés à la table Pizza.</summary>
public class PizzaDbContextTests
{
    [Fact]
    public void AddPizza_SetsCreatedAt()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var pizza = new PizzaModel { Name = "Reine", Ingredients = "Jambon, Champignons", Price = 11m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        pizza.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdatePizza_SetsUpdatedAt()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var pizza = new PizzaModel { Name = "Avant", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        pizza.Name = "Après";
        ctx.SaveChanges();

        pizza.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DeletePizza_SetsIsDeletedAndDeletedAt()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var pizza = new PizzaModel { Name = "Del Pizza", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        ctx.Pizza.Remove(pizza);
        ctx.SaveChanges();

        var found = ctx.Pizza.Find(pizza.Id)!;
        found.IsDeleted.Should().BeTrue();
        found.DeletedAt.Should().NotBeNull();
        found.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DeletePizza_EntityRemainsPhysicallyInDatabase()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var pizza = new PizzaModel { Name = "StillHere", Ingredients = "Mozza", Price = 10m };
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        ctx.Pizza.Remove(pizza);
        ctx.SaveChanges();

        ctx.Pizza.Find(pizza.Id).Should().NotBeNull();
    }

    [Fact]
    public void DeletePizza_DoesNotAffectTacos()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var tacos = new TacosModel { Name = "Tacos X", Meat = "Poulet", Price = 7m };
        var pizza = new PizzaModel { Name = "Pizza Y", Ingredients = "Mozza", Price = 10m };
        ctx.Tacos.Add(tacos);
        ctx.Pizza.Add(pizza);
        ctx.SaveChanges();

        ctx.Pizza.Remove(pizza);
        ctx.SaveChanges();

        ctx.Tacos.Find(tacos.Id)!.IsDeleted.Should().BeFalse();
    }
}