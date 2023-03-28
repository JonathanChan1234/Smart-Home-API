using smart_home_server.Auth.Services;
using smart_home_server.Db;

namespace smart_home_server.Middleware;
public class JwtBlackListMiddleware
{
    private readonly RequestDelegate _next;

    public JwtBlackListMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IJwtService jwtService, AppDbContext context)
    {
        var jti = httpContext.User.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
        if (jti == null)
        {
            await HandleInvalidToken(httpContext, "invalid jti");
            return;
        }
        var jwtInBlackList = await jwtService.CheckIfJwtInBlackList(jti);
        if (jwtInBlackList)
        {
            await HandleInvalidToken(httpContext, "invalid jwt");
            return;
        }
        await _next(httpContext);
    }

    public async Task HandleInvalidToken(HttpContext httpContext, string message)
    {
        httpContext.Response.ContentType = "text/json";
        httpContext.Response.StatusCode = 401; //UnAuthorized
        await httpContext.Response.WriteAsJsonAsync(new { message = message });
    }
}

public static class JwtBlackListMiddlwareExtension
{
    public static IApplicationBuilder UseJwtBlackListMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtBlackListMiddleware>();
    }
}