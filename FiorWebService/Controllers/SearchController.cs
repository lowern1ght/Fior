using Microsoft.AspNetCore.Mvc;

namespace FiorWebService.Controllers; 

[Controller]
[Route("api/v1/[controller]")]
public class SearchController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }
}
