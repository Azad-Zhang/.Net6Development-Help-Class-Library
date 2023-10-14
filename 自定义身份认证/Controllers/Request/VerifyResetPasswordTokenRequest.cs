namespace 自定义身份认证.Controllers.Request
{
    public record VerifyResetPasswordTokenRequest(string email,string token,string newPassword);
}
