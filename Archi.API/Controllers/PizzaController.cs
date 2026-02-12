using Archi.API.Data;
using Archi.API.Models;
using Archi.Library.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Archi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : BaseController<ArchiDbContext, PizzaModel>
{
    public PizzaController(ArchiDbContext context)
        : base(context)
    {
    }
}