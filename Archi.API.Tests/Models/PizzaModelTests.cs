using Archi.API.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Archi.Library.Tests.Helpers;

namespace Archi.API.Tests.Models;

public class PizzaModelTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidPizza_PassesValidation()
    {
        var pizza = new PizzaModel { Name = "Margherita", Base = "Tomate", Ingredients = "Mozzarella, basilic", Price = 12m };
        Validate(pizza).Should().BeEmpty();
    }

    [Fact]
    public void EmptyName_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "", Ingredients = "Mozza", Price = 10m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Name"));
    }

    [Fact]
    public void NameTooLong_FailsValidation()
    {
        var pizza = new PizzaModel { Name = new string('P', 101), Ingredients = "Mozza", Price = 10m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Name"));
    }

    [Fact]
    public void EmptyIngredients_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "Pizza", Ingredients = "", Price = 10m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Ingredients"));
    }

    [Fact]
    public void IngredientsTooShort_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "Pizza", Ingredients = "AB", Price = 10m }; // MinLength(3)
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Ingredients"));
    }

    [Fact]
    public void IngredientsTooLong_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "Pizza", Ingredients = new string('I', 101), Price = 10m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Ingredients"));
    }

    [Fact]
    public void PriceAboveMax_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "Pizza", Ingredients = "Mozza", Price = 100m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Price"));
    }

    [Fact]
    public void PriceBelowMin_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "Pizza", Ingredients = "Mozza", Price = -5m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Price"));
    }

    [Fact]
    public void BaseTooLong_FailsValidation()
    {
        var pizza = new PizzaModel { Name = "Pizza", Base = new string('B', 91), Ingredients = "Mozza", Price = 10m };
        Validate(pizza).Should().Contain(e => e.MemberNames.Contains("Base"));
    }

    [Fact]
    public void NullBase_IsAllowed()
    {
        var pizza = new PizzaModel { Name = "Pizza", Base = null, Ingredients = "Mozza", Price = 10m };
        Validate(pizza).Should().BeEmpty();
    }
}