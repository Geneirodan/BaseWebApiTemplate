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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString));


        services
            .AddAuthorization()
            .Configure<IdentityOptions>(opts => opts.SignIn.RequireConfirmedEmail = bool.Parse(configuration[nameof(opts.SignIn.RequireConfirmedEmail)] ?? string.Empty))
            .ConfigureOptions()
            .AddBusinessLogicServices()
            .AddEndpointsApiExplorer()
            .AddSwagger();
        services
            .AddControllers()
            .AddJsonOptions(options => 
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddBearerAuthentication();

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