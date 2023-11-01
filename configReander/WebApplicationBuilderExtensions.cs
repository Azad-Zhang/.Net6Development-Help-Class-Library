using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace configReander
{
    public static class WebApplicationBuilderExtensions
    {
        public static void ConfigureDbConfiguration(this WebApplicationBuilder builder)
        {
            builder.Host.ConfigureAppConfiguration((hostCtx, configBuilder) =>
            {
                //不能使用ConfigureAppConfiguration中的configBuilder去读取配置，否则就循环调用了，因此这里直接自己去读取配置文件
                //var configRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                //string connStr = configRoot.GetValue<string>("DefaultDB:ConnStr");
                string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
                //string connStr = "Server=42.194.206.48;Database=YouzackVNextDB;User Id=sa;Password=z110112zZ;";
                configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
                // ASP.NET Core 的配置系统支持多种配置源，包括：
                //appsettings.json：应用程序的配置文件，通常位于项目根目录。
                //环境变量：可以从环境变量中读取配置信息。
                ///命令行参数：可以从命令行参数中读取配置信息。
                /////用户机密存储：可以使用用户机密存储来存储敏感信息。
                /////默认情况下，ASP.NET Core 会自动加载这些配置源，并合并它们以构建应用程序的配置。这意味着如果某个配置在多个源中都有定义，它将按照一定的优先级顺序进行合并，从而得到最终的配置值。
            });
        }
        public static void ConfigureExtraServices(this WebApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;
            IConfiguration configuration = builder.Configuration;
            var test212312 = configuration.GetChildren().ToList();
            var test1 = configuration.GetSection("Cors").Get<CorsSettings>();
            var test2 = configuration.GetSection("Recaptcha").Get<RecaptchaSettings>();
            var test3 = configuration.GetSection("FileService:SMB").Get<FileServiceSMBSettings>();

            var test4 = configuration.GetSection("Redis:ConnStr");
            var test5 = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
            var test6 = configuration.GetSection("ElasticSearch:Url");
            var test7 = configuration.GetSection("JWT").Get<JWTSettings>();
            
            var test8 = configuration.GetSection("JWT");
            //services.Configure<JWTSettings>((IConfiguration)configuration.GetSection("JWT").Get<JWTSettings>());
            //var test9 = configuration.GetSection("Email").Get<EmailSetting>();
            var test10 = configuration.GetSection("FileService:Endpoint").Get<FileServiceEndpointSettings>();
            var test11 = "";

        }
    }
}
