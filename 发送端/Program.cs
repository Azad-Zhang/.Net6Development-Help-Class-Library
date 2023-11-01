using System.Reflection;
using Zack.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//ע�� ����Rabbitmq��EventBus���� (�޷������˺�����)
builder.Services.Configure<IntegrationEventRabbitMQOptions>(u =>
{
    u.HostName = "42.194.206.48";
    u.ExchangeName = "ypfExchange1";
    u.UserName = "admin";
    u.Password = "z110112zZ";
});
builder.Services.AddEventBus("ypfQueue1", Assembly.GetExecutingAssembly());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//����
app.UseEventBus();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
