using Microsoft.AspNetCore.Mvc;

namespace FiorWebService.Controllers; 

[Controller]
[Route("[controller]/[action]")]
public class SearchController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] String search)
    {
        
        
        
        return Ok();
    }
}
