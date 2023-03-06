using Microsoft.EntityFrameworkCore;
using smart_home_server.Auth.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;
using smart_home_server.Db;
using smart_home_server.Auth.Services;
using smart_home_server.Home.Services;
using smart_home_server.Exceptions;
using smart_home_server.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

Debug.Assert(builder.Configuration["Jwt:Key"] != null);
var tokenValidationParameter = new TokenValidationParameters()
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidAudience = builder.Configuration["Jwt:Audience"],
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "")),
};
builder.Services.AddSingleton(tokenValidationParameter);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameter;
    });

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IRoomService, RoomService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.UseHttpsRedirection();
}


app.UseAuthentication();
app.UseAuthorization();
app.UseWhen(httpContext =>
    !httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/Login")
    && !httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/Register")
    && !httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/RefreshToken"),
    appBuilder => appBuilder.UseJwtBlackListMiddleware());

app.MapControllers();
app.UseExceptionHandler(ExceptionHandler.Handler());

app.Run();
