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
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Routing;
using System.Text.Json;
using smart_home_server.Mqtt.Authentication;
using smart_home_server.Mqtt.Client.Services;
using Microsoft.AspNetCore.Authorization;
using smart_home_server.Scenes.Services;
using smart_home_server.Home.Authorization;
using smart_home_server.SmartDevices.SubDevices.Lights.Service;
using smart_home_server.SmartDevices.Services;
using smart_home_server.SmartDevices.SubDevices.Shades.Services;
using smart_home_server.Processors.Service;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel(
        o =>
            {
                o.ListenAnyIP(5181);
                o.ListenAnyIP(1883, l => l.UseMqtt());
                o.ListenAnyIP(7025, l => l.UseHttps());
            }
    );

builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<AppDbContext>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
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
builder.Services.AddSingleton<IAuthorizationHandler, HomeAuthorizationHandler>();

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
builder.Services.AddScoped<IMqttClientService, MqttClientService>();
builder.Services.AddScoped<ISceneService, SceneService>();
builder.Services.AddScoped<ISceneActionService, SceneActionService>();
builder.Services.AddScoped<ILightActionService, LightActionService>();
builder.Services.AddScoped<IShadeActionService, ShadeActionService>();
builder.Services.AddScoped<ISmartDeviceService, SmartDeviceService>();
builder.Services.AddScoped<ISmartLightService, SmartLightService>();
builder.Services.AddScoped<ISmartShadeService, SmartShadeService>();
builder.Services.AddScoped<IProcessorService, ProcessorService>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMqttControllers();
builder.Services.AddMqttDefaultJsonOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web));
builder.Services.AddMqttAuthentication();

builder.Services
    .AddHostedMqttServerWithServices(mqttServer =>
    {
        mqttServer.WithoutDefaultEndpoint();
    })
    .AddMqttConnectionHandler()
    .AddConnections();

var app = builder.Build();

app.UseRouting();
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

app.MapConnectionHandler<MqttConnectionHandler>(
    "/mqtt",
    httpConenctionDispatcherOptions => httpConenctionDispatcherOptions.WebSockets.SubProtocolSelector =
        protocolList => protocolList.FirstOrDefault() ?? String.Empty);
app.UseMqttServer(server =>
{
    server.WithAuthentication(app.Services);
    server.WithAttributeRouting(app.Services, allowUnmatchedRoutes: false);
});

app.Run();
