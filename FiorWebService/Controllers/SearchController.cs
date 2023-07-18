using Microsoft.AspNetCore.Mvc;

namespace FiorWebService.Controllers; 

[Controller]
public class SearchController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }
}
