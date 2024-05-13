using Markos.TicTacToe.Controllers;
using Markos.TicTacToe.Game;
using Markos.TicTacToe.MVC;
using System.Reflection;

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

        builder.Services.AddSwaggerGen(c =>
        {
            if (Assembly.GetEntryAssembly() is Assembly a && Path.GetDirectoryName(a.Location) is string baseDir)
            {
                foreach (var item in from assemblyPath in Directory.EnumerateFiles(baseDir, "*.dll", SearchOption.AllDirectories)
                                     let xmlDocPath = Path.ChangeExtension(assemblyPath, "xml")
                                     where File.Exists(xmlDocPath)
                                     select xmlDocPath)
                {
                    c.IncludeXmlComments(item, true);
                }
            }
        });

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
