using Archi.API.Data;
using Archi.API.Models;
using Archi.library.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Archi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : BaseController<ArchiDbContext, PizzaModel>
{
    //private readonly ArchiDbContext _context;

    public PizzaController(ArchiDbContext context) : base(context)
    {
        //_context = context;
    }

    /*[HttpGet]
    public ActionResult<IEnumerable<PizzaModel>> Get()
    {
        return Ok(_context.Pizzas.Where(x => x.IsActive).ToList());

    }*/

    [HttpGet("{id}")]
    public ActionResult<PizzaModel> GetById([FromRoute]int id)
    {
        var pizza = _context.Pizza.Find(id);
        if (pizza == null)
        {
            return NotFound();
        }
        return Ok(pizza);
    }

    [HttpPost]
    public ActionResult<PizzaModel> Post(PizzaModel pizza)
    {
        if (ModelState.IsValid)
        {
            _context.Pizza.Add(pizza);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = pizza.Id }, pizza);
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPut("{id}")]
    public ActionResult Put([FromRoute]int id, [FromBody]PizzaModel pizza)
    {
        if (id != pizza.Id)
        {
            return BadRequest();
        }
        var existingPizza = _context.Pizza.Find(id);
        if (existingPizza == null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            _context.Entry(existingPizza).CurrentValues.SetValues(pizza);
            _context.SaveChanges();
            return NoContent();
        }
        return BadRequest(ModelState);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute]int id)
    {
        var pizza = _context.Pizza.Find(id);
        if (pizza == null)
        {
            return NotFound();
        }
        _context.Pizza.Remove(pizza);
        //_context.Entry(pizza).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
        _context.SaveChanges();
        return NoContent();
    }
}
