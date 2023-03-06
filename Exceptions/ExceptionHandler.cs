using Microsoft.AspNetCore.Diagnostics;
using smart_home_server.Auth.Exceptions;

namespace smart_home_server.Exceptions;

public class ExceptionHandler
{
    public static Action<IApplicationBuilder> Handler()
    {
        return app => app.Run(async context =>
       {
           var exceptionHandlerPathFeature =
               context.Features.Get<IExceptionHandlerPathFeature>();
           var error = exceptionHandlerPathFeature?.Error;
           if (error == null) return;
           if (error is ModelNotFoundException)
           {
               context.Response.StatusCode = StatusCodes.Status404NotFound;
               await context.Response.WriteAsJsonAsync(new { message = error.Message });
               return;
           }
           if (error is BadAuthenticationException)
           {
               context.Response.StatusCode = StatusCodes.Status401Unauthorized;
               await context.Response.WriteAsJsonAsync(new { message = error.Message });
               return;
           }
           if (error is BadRequestException)
           {

               context.Response.StatusCode = StatusCodes.Status400BadRequest;
               await context.Response.WriteAsJsonAsync(new { message = error.Message });
               return;
           }
       });
    }
}