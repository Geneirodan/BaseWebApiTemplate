using AutoFilterer.Swagger;
using BusinessLogic.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Filters;

namespace WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static AuthenticationBuilder AddBearerAuthentication(this IServiceCollection services)
    {
        var jwtOptions = services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>().Value;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        services.AddSingleton(tokenValidationParameters);

        return services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers.Append("IS-TOKEN-EXPIRED", "true");
                        return Task.CompletedTask;
                    }
                };
            });
    }


    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("oauth2",
                new OpenApiSecurityScheme
                {
                    Description = "Authorization using Bearer scheme 'Bearer <token>'",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
            c.OperationFilter<SecurityRequirementsOperationFilter>();
            c.UseAutoFiltererParameters();
        });

        return services;
    }
    public static IServiceCollection ConfigureOptions(this IServiceCollection services)
    {

        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.Section);
        services.AddOptions<GoogleAuthOptions>().BindConfiguration(GoogleAuthOptions.Section);
        services.AddOptions<AdminOptions>().BindConfiguration(AdminOptions.Section);
        services.AddOptions<MailOptions>().BindConfiguration(MailOptions.Section);
        return services;
    }

}
