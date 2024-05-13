using Markos.TicTacToe.Game;
using Markos.TicTacToe.MVC;

namespace Markos.TicTacToe;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(c =>
            {
                c.FeatureProviders.Add(new InternalControllerProvider());
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<IGameManager<TicTacToeGame>, TicTacToeGameManager>();

        var app = builder.Build();

        app.UseWebSockets(new()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(10),
            AllowedOrigins = { "*" },
        });

        //app.UseDefaultFiles();
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Map("api/{**rest}", (HttpContext context, string rest) =>
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        });

        {
            RouteValueDictionary fallbackRoute = new()
            {
                ["rest"] = "index.html",
            };

            app.RemapMissingFile("/a/{**rest}", fallbackRoute);
            app.RemapMissingFile("/r/{**rest}", fallbackRoute);
        }

        app.MapGet("/", ctx =>
        {
            ctx.Response.Redirect("/a/");
            return Task.CompletedTask;
        });

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
