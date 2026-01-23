using Archi.API.Data;
using Archi.API.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TacosController : ControllerBase
{
    private readonly ArchiDbContext _context;

    public TacosController(ArchiDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TacosModel>> Get()
    {
        return _context.Tacos.ToList();
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
}