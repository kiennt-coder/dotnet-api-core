using Microsoft.AspNetCore.Authorization;

namespace BlogApi.Services.Auth;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
    {
        // Nếu user không có trong scope của Claims, thoát ra ngoài
        if (!context.User.Claims.Any(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        // Cắt chuỗi scopes thành mảng
        var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer)!.Value.Split(' ');

        // Thành công nếu mảng scope có chứa scope cần tìm
        if (scopes.Any(s => s == requirement.Scope))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
