using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FiorWebApplication.Pages; 

public class Home : PageModel {
    public void OnGet() {
        Response.WriteAsync("dawdawd");
    }
}
