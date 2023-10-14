using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using 自定义身份认证.Controllers.Request;
using 自定义身份认证.Options;

namespace 自定义身份认证.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> logger;
        private readonly RoleManager<Role> roleManager;
        private readonly UserManager<User> userManager;
        public TestController(ILogger<TestController> logger,
            RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            this.logger = logger;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }
        /// <summary>
        /// 创建Token
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string BuildToken(IEnumerable<Claim> claims, JWTOptions options)
        {
            DateTime expires = DateTime.Now.AddSeconds(options.ExpireSeconds);
            byte[] keyBytes = Encoding.UTF8.GetBytes(options.SigningKey);
            var secKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(secKey,
                SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(expires: expires,
                signingCredentials: credentials, claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        /// <summary>
        /// 初始化用户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateUserRole()
        {
            bool roleExists = await roleManager.RoleExistsAsync("Admin");
            if (!roleExists)
            {
                Role role = new Role { Name = "Admin" };
                var r = await roleManager.CreateAsync(role);
                if (!r.Succeeded)
                {
                    return BadRequest(r.Errors);
                }
            }
            User user = await this.userManager.FindByNameAsync("yzk");
            if (user == null)
            {
                user = new User { UserName = "yzk", Email = "yangzhongke8@gmail.com", EmailConfirmed = true };
                var r = await userManager.CreateAsync(user, "123456");
                if (!r.Succeeded)
                {
                    return BadRequest(r.Errors);
                }
                r = await userManager.AddToRoleAsync(user, "Admin");
                if (!r.Succeeded)
                {
                    return BadRequest(r.Errors);
                }
            }
            var successResponse = new { status = "success", message = "User and role created successfully" };
            return Ok(successResponse);
        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="req"></param>
        /// <param name="jwtOptions"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Login(LoginRequest req, [FromServices] IOptions<JWTOptions> jwtOptions)
        {
            string userName = req.UserName;
            string password = req.Password;
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                var successResponse = new { status = "Failed", message = $"用户名不存在{userName}" };
                return NotFound(successResponse);
            }
            if (await userManager.IsLockedOutAsync(user))
            {
                var successResponse = new { status = "Failed", message = $"用户已锁定{userName}" };
                return NotFound(successResponse);
            }
            var success = await userManager.CheckPasswordAsync(user, password);
            if (!success)
            {
                var r = await userManager.AccessFailedAsync(user);
                if (!r.Succeeded)
                {
                    return BadRequest(new { status = "Failed", message = "AccessFailed failed" });
                }
                return BadRequest(new { status = "Failed", message = "账号或者密码错误" });

            }
            else
            {
                user.JWTVersion++;
                await userManager.UpdateAsync(user);
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                claims.Add(new Claim(ClaimTypes.Version, user.JWTVersion.ToString()));
                var roles = await userManager.GetRolesAsync(user);
                foreach (string role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                string jwtToken = BuildToken(claims, jwtOptions.Value);
                var successResponse = new { status = "success", message = "登录成功",token= jwtToken };
                return Ok(successResponse);
            }
        }

        /// <summary>
        /// 测试身份验证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin",Policy = "RequireAuthorizeHeader")]
        public async Task<IActionResult> TestAuthentication()
        {
            string id = this.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            string userName = this.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            IEnumerable<Claim> roleClaims = this.User.FindAll(ClaimTypes.Role);
            string roleNames = string.Join(',', roleClaims.Select(c => c.Value));
            return Ok(new { id=id,userName=userName,roleNames =roleNames});
        }
        [HttpGet]
        public async Task<IActionResult> test()
        {
            return Ok();
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> VerifyResetPasswordToken(
            VerifyResetPasswordTokenRequest req)
        {
            string email = req.email;
            var user = await userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return NotFound(new { status = "Failed", message = $"用户名不存在{req.email}" });
            }
            else
            {
                string token = req.token;
                string password = req.newPassword;
                var r = await userManager.ResetPasswordAsync(user, token, password);
                if(r.Succeeded)
                {
                    return Ok(new {status = "Successed",message="密码修改成功"});
                }
                else
                {
                    return BadRequest(new { status = "Successed", message = "密码修改失败" });
                }
            }
        }

        /// <summary>
        /// 发送重置密码的token
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SendResetPasswordToken(
                    SendResetPasswordTokenRequest req)
        {
            string email = req.Email;
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"邮箱不存在{email}");
            }
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            logger.LogInformation($"向邮箱{user.Email}发送Token={token}");
            return Ok();
        }
    }
}
