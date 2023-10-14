using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using 自定义身份认证;
using 自定义身份认证.Filter;
using 自定义身份认证.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//对Swagger进行设置，添加Authorization报文头
builder.Services.AddSwaggerGen(c =>
{
    var scheme = new OpenApiSecurityScheme()
    {
        Description = "Authorization header. \r\nExample: 'Bearer 12345abcdef'",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Authorization"
        },
        Scheme = "oauth2",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
    };
    c.AddSecurityDefinition("Authorization", scheme);
    var requirement = new OpenApiSecurityRequirement();
    requirement[scheme] = new List<string>();
    c.AddSecurityRequirement(requirement);
});

//添加数据库
builder.Services.AddDbContext<IdDbContext>(opt =>
{
    string connStr = builder.Configuration.GetConnectionString("Default");
    opt.UseSqlServer(connStr);
});
builder.Services.AddDataProtection();

builder.Services.AddIdentityCore<User>(options =>
{
    // 设置密码的复杂度规则
    options.Password.RequireDigit = false; // 不需要包含数字
    options.Password.RequireLowercase = false; // 不需要包含小写字母
    options.Password.RequireNonAlphanumeric = false; // 不需要包含特殊字符
    options.Password.RequireUppercase = false; // 不需要包含大写字母
    options.Password.RequiredLength = 6; // 密码最短长度为6个字符

    // 配置令牌提供程序用于生成密码重置和电子邮件确认的令牌
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
});
var idBuilder = new IdentityBuilder(typeof(User), typeof(Role), builder.Services);
//配置 Entity Framework 存储
//配置默认的令牌提供程序：
//配置角色管理器 (RoleManager<Role>)：
//配置用户管理器 (UserManager<User>)：
idBuilder.AddEntityFrameworkStores<IdDbContext>()
    .AddDefaultTokenProviders()
    .AddRoleManager<RoleManager<Role>>()
    .AddUserManager<UserManager<User>>();
//获取JWT配置
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
//将（JwtBearerDefaults.AuthenticationScheme）配置为默认的身份验证方案。
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        var jwtOpt = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtOpt.SigningKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        x.TokenValidationParameters = new()
        {
            ValidateIssuer = false,  // 是否验证颁发者
            ValidateAudience = false,  // 是否验证受众者
            ValidateLifetime = true,  // 是否验证令牌的有效期
            ValidateIssuerSigningKey = true,  // 是否验证签名密钥

            IssuerSigningKey = secKey  // 设置签名密钥
        };
    });
//添加自定义授权策略
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthorizeHeader", policy =>
    {
        policy.Requirements.Add(new AuthorizeHeaderRequirement());
    });
});
builder.Services.AddSingleton<IAuthorizationHandler, AuthorizeHeaderHandler>();
//添加内存注册
builder.Services.AddMemoryCache();
//添加全局筛选器
builder.Services.Configure<MvcOptions>(opt =>
{
    opt.Filters.Add<JWTValidationFilter>();
});
//添加检测请求头
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//启用身份验证中间件，应保证在（UseAuthorization）身份授权中间件之前
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
