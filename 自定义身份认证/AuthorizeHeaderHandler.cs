using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace 自定义身份认证
{
    public class AuthorizeHeaderHandler : AuthorizationHandler<AuthorizeHeaderRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizeHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeHeaderRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (!context.User.Identity.IsAuthenticated)
            {
                // 用户未通过身份验证，可以返回自定义的未授权响应
                SetUnauthorizedResponse(httpContext);
                context.Fail();
                return Task.CompletedTask;
            }

            if (!context.User.HasClaim(ClaimTypes.Role, "Admin"))
            {
                // 用户没有 "Admin" 角色，可以返回自定义的未授权响应
                context.Fail();
                return Task.CompletedTask;
            }

            // 用户通过了授权验证
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        private void SetUnauthorizedResponse(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            httpContext.Response.ContentType = "application/json";

            var response = new
            {
                message = "token错误"
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            httpContext.Response.WriteAsync(jsonResponse);
        }
    }


}
