using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using �Զ��������֤;
using �Զ��������֤.Filter;
using �Զ��������֤.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//��Swagger�������ã����Authorization����ͷ
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

//������ݿ�
builder.Services.AddDbContext<IdDbContext>(opt =>
{
    string connStr = builder.Configuration.GetConnectionString("Default");
    opt.UseSqlServer(connStr);
});
builder.Services.AddDataProtection();

builder.Services.AddIdentityCore<User>(options =>
{
    // ��������ĸ��Ӷȹ���
    options.Password.RequireDigit = false; // ����Ҫ��������
    options.Password.RequireLowercase = false; // ����Ҫ����Сд��ĸ
    options.Password.RequireNonAlphanumeric = false; // ����Ҫ���������ַ�
    options.Password.RequireUppercase = false; // ����Ҫ������д��ĸ
    options.Password.RequiredLength = 6; // ������̳���Ϊ6���ַ�

    // ���������ṩ�������������������ú͵����ʼ�ȷ�ϵ�����
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
});
var idBuilder = new IdentityBuilder(typeof(User), typeof(Role), builder.Services);
//���� Entity Framework �洢
//����Ĭ�ϵ������ṩ����
//���ý�ɫ������ (RoleManager<Role>)��
//�����û������� (UserManager<User>)��
idBuilder.AddEntityFrameworkStores<IdDbContext>()
    .AddDefaultTokenProviders()
    .AddRoleManager<RoleManager<Role>>()
    .AddUserManager<UserManager<User>>();
//��ȡJWT����
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
//����JwtBearerDefaults.AuthenticationScheme������ΪĬ�ϵ������֤������
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        var jwtOpt = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtOpt.SigningKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        x.TokenValidationParameters = new()
        {
            ValidateIssuer = false,  // �Ƿ���֤�䷢��
            ValidateAudience = false,  // �Ƿ���֤������
            ValidateLifetime = true,  // �Ƿ���֤���Ƶ���Ч��
            ValidateIssuerSigningKey = true,  // �Ƿ���֤ǩ����Կ

            IssuerSigningKey = secKey  // ����ǩ����Կ
        };
    });
//����Զ�����Ȩ����
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthorizeHeader", policy =>
    {
        policy.Requirements.Add(new AuthorizeHeaderRequirement());
    });
});
builder.Services.AddSingleton<IAuthorizationHandler, AuthorizeHeaderHandler>();
//����ڴ�ע��
builder.Services.AddMemoryCache();
//���ȫ��ɸѡ��
builder.Services.Configure<MvcOptions>(opt =>
{
    opt.Filters.Add<JWTValidationFilter>();
});
//��Ӽ������ͷ
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//���������֤�м����Ӧ��֤�ڣ�UseAuthorization�������Ȩ�м��֮ǰ
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
