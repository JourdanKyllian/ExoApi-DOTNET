using Archi.API.Data;
using Archi.API.Models;
using Archi.Library.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Archi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TacosController : BaseController<ArchiDbContext, TacosModel>
{
    public TacosController(ArchiDbContext context)
        : base(context)
    {
    }
}