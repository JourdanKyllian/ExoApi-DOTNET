using Archi.Library.Controllers;
using Archi.Library.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Archi.Library.Tests.Helpers;

[ApiController]
[Route("api/v1/[controller]")]
public class FakeController : BaseController<FakeDbContext, FakeModel>
{
    public FakeController(FakeDbContext context) : base(context) { }
}
