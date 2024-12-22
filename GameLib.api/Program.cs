using System.Text;
using GameLib.api.Infrastructure.Authentication;
using GameLib.api.Infrastructure.Cache;
using GameLib.api.Infrastructure.Session;
using GameLib.api.Services;
using GameLib.api.Services.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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

        #region Authorization Services

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

        #endregion

        #region Supabase Services

        builder.Services.AddSingleton<Client>(_ =>
            new Client(
                builder.Configuration["Supabase:Url"]!,
                builder.Configuration["Supabase:Key"],
                new SupabaseOptions()
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                }));

        #endregion

        #region Core Services

        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        #endregion

        #region Cache Services

        builder.Services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(builder.Configuration["Cache:ConnectionString"]!));
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
        builder.Services.AddScoped<IUserSpecificCacheService, CacheService>();

        #endregion

        #region Session Services

        builder.Services.AddTransient<ISessionService, SessionService>();
        builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();

        #endregion

        #region API Services

        builder.Services.AddTransient<ITestService, TestService>();

        #endregion

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