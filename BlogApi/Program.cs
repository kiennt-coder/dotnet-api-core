using BlogApi.Models.Auth;
using BlogApi.Models.Database;
using BlogApi.Models.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using BlogApi.Services.Auth;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Logging;
using System.Text;
using BlogApi.Helpers.HashPassword;
using StackExchange.Redis;
using BlogApi.Services.Redis;
using BlogApi.Services.User;
using BlogApi.Services.Role;
using BlogApi.Services.Database.MongoDB;
using BlogApi.Services.Permission;
using BlogApi.Models.Permission;
using BlogApi.Middlewares;
using Serilog;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add PII
IdentityModelEventSource.ShowPII = true;

// Add Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console().MinimumLevel.Information();
    config.WriteTo.File(
        path: AppDomain.CurrentDomain.BaseDirectory + "../../../Logs/blogapi-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true
    ).MinimumLevel.Information();

});
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Add AuthJWT handle
builder.Services.AddSingleton<IAuthJWTService, AuthJWTService>();

// Add Redis
builder.Services.Configure<RedisSetting>(builder.Configuration.GetSection("RedisCache"));
builder.Services.Configure<AuthJWTSetting>(builder.Configuration.GetSection("AuthJWT"));
var RedisEnable = builder.Configuration.GetValue<bool>("RedisCache:Enable");
if (RedisEnable != true)
{
    Console.WriteLine("RedisEnable::", RedisEnable);
}
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(builder.Configuration["RedisCache:ConnectionString"]));
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCache:ConnectionString"];
});

builder.Services.AddControllers();

// Add Config Database
builder.Services.Configure<BlogDatabaseSetting>(builder.Configuration.GetSection("BlogDatabase"));
// Add Connect Database
builder.Services.AddSingleton<IBlogDatabase, BlogDatabase>();

// Add Users Services
builder.Services.AddSingleton<IUsersService, UsersService>();

// Add Roles Services
builder.Services.AddSingleton<IRolesService, RolesService>();

// Add Permissions Service
builder.Services.AddSingleton<IPermissionsService, PermissionsService>();

// Add AuthJWT
// string domain = $"https://{builder.Configuration["AuthJWT:Domain"]}/";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    // options.Authority = domain;
    options.Audience = builder.Configuration["AuthJWT:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AuthJWT:Audience"],
        ValidIssuer = builder.Configuration["AuthJWT:Issuer"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthJWT:Secret"]!)),
        NameClaimType = ClaimTypes.NameIdentifier
    };
});
builder.Services.AddAuthorization(options =>
{
    try
    {
        ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
        var permissionsService = serviceProvider.GetRequiredService<IPermissionsService>();
        List<PermissionModel> permissions = permissionsService.GetAllAsync().GetAwaiter().GetResult();
        foreach (var item in permissions)
        {
            options.AddPolicy(
                item.Key!,
                policy => policy.Requirements.Add(
                    new HasScopeRequirement(item.Key!, builder.Configuration["AuthJWT:Issuer"]!)
                )
            );
        }
    }
    catch (System.Exception)
    {

        throw;
    }
});
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
builder.Services.AddScoped<IHashPassword, HashPassword>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BlogApi", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Vui lòng nhập token!",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseHttpsRedirection();
app.UseMiddleware<JWTMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
