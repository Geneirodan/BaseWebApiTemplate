using Domain.Extensions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using API.Extensions;
using Infrastructure;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var configuration = builder.Configuration;

        var connectionString = configuration["CONNECTION_STRING"];
        services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString));


        services
            .AddAuthorization()
            .Configure<IdentityOptions>(opts =>
                opts.SignIn.RequireConfirmedEmail = bool.Parse(configuration[nameof(opts.SignIn.RequireConfirmedEmail)] ?? string.Empty))
            .ConfigureOptions()
            .AddBusinessLogicServices()
            .AddEndpointsApiExplorer()
            .AddSwagger()
            .AddControllersWithViews(options => options.UseGeneralRoutePrefix("api/v{version:apiVersion}"))
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddBearerAuthentication();
        services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(options =>
        {
            // ReSharper disable once StringLiteralTypo
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddCors(c =>
        {
            c.AddPolicy("DefaultPolicy",
                p =>
                {
                    p.AllowAnyMethod();
                    p.AllowAnyOrigin();
                    p.AllowAnyHeader();
                });
        });

        var app = builder.Build();

        await app.Services.CreateScope().ServiceProvider.GetRequiredService<ISeeder>().SeedAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("DefaultPolicy");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
