namespace FiorWebApplication;

public static class Program {
    private static void Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddRazorPages();
        builder.Services.AddMvc();

        WebApplication app = builder.Build();
        
        app.UseStaticFiles();

        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        
        app.MapGet("/", () => "Hello World!");
        app.Run();
    }
}