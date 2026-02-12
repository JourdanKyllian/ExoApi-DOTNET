using Archi.API.Data;
using Archi.API.Models;
using Archi.Library.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Archi.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TacosController : BaseController<ArchiDbContext, TacosModel>
{
    public TacosController(ArchiDbContext context)
        : base(context)
    {
    }
}