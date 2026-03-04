using Archi.API.Models;
using Archi.Library.Tests.Helpers;
using FluentAssertions;
using Xunit;
using Archi.Library.Tests.Helpers;

namespace Archi.API.Tests.Data;

/// <summary>Tests sur les timestamps et soft delete liés à la table Tacos.</summary>
public class TacosDbContextTests
{
    [Fact]
    public void AddTacos_SetsCreatedAt()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var tacos = new TacosModel { Name = "Tacos", Meat = "Poulet", Price = 7m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        tacos.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateTacos_SetsUpdatedAt()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var tacos = new TacosModel { Name = "Avant", Meat = "Poulet", Price = 7m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        tacos.Name = "Après";
        ctx.SaveChanges();

        tacos.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DeleteTacos_SetsIsDeletedAndDeletedAt()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var tacos = new TacosModel { Name = "Delete me", Meat = "Boeuf", Price = 8m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        ctx.Tacos.Remove(tacos);
        ctx.SaveChanges();

        var found = ctx.Tacos.Find(tacos.Id)!;
        found.IsDeleted.Should().BeTrue();
        found.DeletedAt.Should().NotBeNull();
        found.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DeleteTacos_EntityRemainsPhysicallyInDatabase()
    {
        using var ctx = TestFactory.CreateArchiInMemoryContext();
        var tacos = new TacosModel { Name = "StillHere", Meat = "Dinde", Price = 6m };
        ctx.Tacos.Add(tacos);
        ctx.SaveChanges();

        ctx.Tacos.Remove(tacos);
        ctx.SaveChanges();

        ctx.Tacos.Find(tacos.Id).Should().NotBeNull();
    }
}