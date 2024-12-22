using System.Text;
using GameLib.api.Infrastructure.Session;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Supabase;

namespace GameLib.api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        builder.Services.AddAuthorization();
        byte[] bytes = Encoding.UTF8.GetBytes(builder.Configuration["Authentication:JwtBearer:SigningKey"]!);
        builder.Services.AddAuthentication().AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(bytes),
                ValidAudience = builder.Configuration["Authentication:JwtBearer:Audience"],
                ValidIssuer = builder.Configuration["Authentication:JwtBearer:Issuer"],
            };
        });

        builder.Services.AddSingleton<Client>(_ =>
            new Client(
                builder.Configuration["Supabase:Url"]!,
                builder.Configuration["Supabase:Key"],
                new SupabaseOptions()
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                }));
        
        builder.Services.AddTransient<ISessionService, SessionService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}