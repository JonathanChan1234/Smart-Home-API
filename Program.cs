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
using smart_home_server.SmartDevices.SubDevices.AirConditioner.Service;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

#region builder

#region Kestrel Pipline
builder.WebHost
    .UseKestrel(
        o =>
            {
                o.ListenAnyIP(1883, l => l.UseMqtt());
                o.ListenAnyIP(5181);
            }
    );
#endregion

#region Forwarded Header Option
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
});
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
        policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
#endregion

#region HTTP Context Accessor
builder.Services.AddHttpContextAccessor();
#endregion

#region Identity Core
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<AppDbContext>();
#endregion

#region Db Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
#endregion

#region JWT Authentication
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
#endregion

#region Dependency Injection
builder.Services.AddSingleton<IAuthorizationHandler, HomeAuthorizationHandler>();
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
builder.Services.AddScoped<IAirConditionerActionService, AirConditionerActionService>();
builder.Services.AddScoped<ISmartDeviceService, SmartDeviceService>();
builder.Services.AddScoped<ISmartLightService, SmartLightService>();
builder.Services.AddScoped<ISmartShadeService, SmartShadeService>();
builder.Services.AddScoped<IProcessorService, ProcessorService>();
builder.Services.AddScoped<IAirConditionerService, AirConditionerService>();
#endregion

#region Controller & Swagger Endpoint
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region MQTT
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
#endregion
#endregion

#region app
var app = builder.Build();
app.UseRouting();

#region MQTT
app.MapConnectionHandler<MqttConnectionHandler>(
    "/mqtt",
    httpConenctionDispatcherOptions => httpConenctionDispatcherOptions.WebSockets.SubProtocolSelector =
        protocolList => protocolList.FirstOrDefault() ?? String.Empty);
app.UseMqttServer(server =>
{
    server.WithAuthentication(app.Services);
    server.WithAttributeRouting(app.Services, allowUnmatchedRoutes: false);
});
#endregion

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
if (app.Environment.IsDevelopment())
{
    app.UseCors(MyAllowSpecificOrigins);
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseForwardedHeaders();
    // app.UseHttpsRedirection();
}
else
{
    app.UseForwardedHeaders();
}

#region Authentication
app.UseAuthentication();
app.UseAuthorization();
app.UseWhen(httpContext =>
{
    System.Console.WriteLine(httpContext.Request.Path);
    return !httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/Login")
        && !httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/Register")
        && !httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/RefreshToken");
}, appBuilder => appBuilder.UseJwtBlackListMiddleware());
#endregion

app.MapControllers();
app.UseExceptionHandler(ExceptionHandler.Handler());
app.Run();
#endregion