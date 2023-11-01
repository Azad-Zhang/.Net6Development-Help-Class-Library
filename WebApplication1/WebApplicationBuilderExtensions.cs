

using Zack.EventBus;

namespace configReander
{
    public static class WebApplicationBuilderExtensions
    {
        public static void ConfigureExtraServices(this WebApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;
            IConfiguration configuration = builder.Configuration;
            var test212312 = configuration.GetChildren().ToList();
            //var test1 = configuration.GetSection("Cors").Get<CorsSettings>();
            //var test2 = configuration.GetSection("Recaptcha").Get<RecaptchaSettings>();
            //var test3 = configuration.GetSection("FileService:SMB").Get<FileServiceSMBSettings>();

            //var test4 = configuration.GetSection("Redis:ConnStr");
            //var test5 = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
            //var test6 = configuration.GetSection("ElasticSearch:Url");
            //var test7 = configuration.GetSection("JWT").Get<JWTSettings>();

            //var test8 = configuration.GetSection("JWT");
            ////services.Configure<JWTSettings>((IConfiguration)configuration.GetSection("JWT").Get<JWTSettings>());
            ////var test9 = configuration.GetSection("Email").Get<EmailSetting>();
            //var test10 = configuration.GetSection("FileService:Endpoint").Get<FileServiceEndpointSettings>();
            var test = configuration.GetSection("RabbitMQ");
            var test11 = "";
            services.Configure<IntegrationEventRabbitMQOptions>(configuration.GetSection("RabbitMQ"));
            

        }
    }
}
