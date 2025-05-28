using Microsoft.AspNetCore.Authentication;

namespace PubMedEdu.SearchApi.Middleware;

public class CustomAuthenticationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        AuthenticateResult result = await context.AuthenticateAsync();
        
        if (result is {Succeeded: true, Principal: not null})
        {
            context.User = result.Principal;
        }

        await next(context);
    }
}