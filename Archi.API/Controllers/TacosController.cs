using Archi.API.Data;
using Archi.API.Models;
using Archi.library.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Archi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TacosController : BaseController<ArchiDbContext, TacosModel>
{

    public TacosController(ArchiDbContext context) : base(context)
    {
    }

    [HttpGet("{id}")]
    public ActionResult<TacosModel> GetById([FromRoute]int id)
    {
        var tacos = _context.Tacos.Find(id);
        if (tacos == null)
        {
            return NotFound();
        }
        return Ok(tacos);
    }

    [HttpPost]
    public ActionResult<TacosModel> Post(TacosModel tacos)
    {
        if (ModelState.IsValid)
        {
            _context.Tacos.Add(tacos);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = tacos.Id }, tacos);
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPut("{id}")]
    public ActionResult Put([FromRoute]int id, [FromBody]TacosModel tacos)
    {
        if (id != tacos.Id)
        {
            return BadRequest();
        }
        var existingTacos = _context.Tacos.Find(id);
        if (existingTacos == null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            _context.Entry(existingTacos).CurrentValues.SetValues(tacos);
            _context.SaveChanges();
            return NoContent();
        }
        return BadRequest(ModelState);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute]int id)
    {
        var tacos = _context.Tacos.Find(id);
        if (tacos == null)
        {
            return NotFound();
        }
        _context.Tacos.Remove(tacos);
        _context.SaveChanges();
        return NoContent();
    }
}
