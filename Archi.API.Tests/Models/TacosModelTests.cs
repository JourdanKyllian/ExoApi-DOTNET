using Archi.API.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Archi.Library.Tests.Helpers;

namespace Archi.API.Tests.Models;

public class TacosModelTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidTacos_PassesValidation()
    {
        var tacos = new TacosModel { Name = "Tacos Royal", Sauce = "Algérienne", Meat = "Poulet", Price = 8.50m };
        Validate(tacos).Should().BeEmpty();
    }

    [Fact]
    public void EmptyName_FailsValidation()
    {
        var tacos = new TacosModel { Name = "", Meat = "Boeuf", Price = 7m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Name"));
    }

    [Fact]
    public void NameTooLong_FailsValidation()
    {
        var tacos = new TacosModel { Name = new string('A', 101), Meat = "Boeuf", Price = 7m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Name"));
    }

    [Fact]
    public void EmptyMeat_FailsValidation()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "", Price = 7m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Meat"));
    }

    [Fact]
    public void MeatTooShort_FailsValidation()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "AB", Price = 7m }; // MinLength(3)
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Meat"));
    }

    [Fact]
    public void MeatTooLong_FailsValidation()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = new string('X', 101), Price = 7m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Meat"));
    }

    [Fact]
    public void PriceAboveMax_FailsValidation()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "Poulet", Price = 999m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Price"));
    }

    [Fact]
    public void PriceBelowMin_FailsValidation()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "Poulet", Price = -1m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Price"));
    }

    [Fact]
    public void SauceTooLong_FailsValidation()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "Poulet", Sauce = new string('S', 91), Price = 7m };
        Validate(tacos).Should().Contain(e => e.MemberNames.Contains("Sauce"));
    }

    [Fact]
    public void NullSauce_IsAllowed()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "Boeuf", Sauce = null, Price = 5m };
        Validate(tacos).Should().BeEmpty();
    }

    [Fact]
    public void IsVegan_DefaultsToFalse()
    {
        new TacosModel().IsVegan.Should().BeFalse();
    }

    [Fact]
    public void IsVegan_CanBeSetToTrue()
    {
        var tacos = new TacosModel { Name = "Tacos", Meat = "Soja", Price = 7m, IsVegan = true };
        Validate(tacos).Should().BeEmpty();
        tacos.IsVegan.Should().BeTrue();
    }
}