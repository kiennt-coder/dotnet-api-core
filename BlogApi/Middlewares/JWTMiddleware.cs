using System.Net;
using System.Text.Json;
using BlogApi.Models.Response;

namespace BlogApi.Middlewares;

public class JWTMiddleware
{
    private readonly RequestDelegate _next;

    public JWTMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);

        if (httpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            BaseResponse baseResponse = new BaseResponse
            {
                Status = StatusCodes.Status401Unauthorized,
                Message = BaseResponseMessage.Unauthorized()
            };
            await httpContext.Response.WriteAsJsonAsync(baseResponse);
        }

        if (httpContext.Response.StatusCode == (int)HttpStatusCode.Forbidden)
        {
            BaseResponse baseResponse = new BaseResponse
            {
                Status = StatusCodes.Status403Forbidden,
                Message = BaseResponseMessage.Forbidden()
            };

            await httpContext.Response.WriteAsJsonAsync(baseResponse);
        }
    }
}
