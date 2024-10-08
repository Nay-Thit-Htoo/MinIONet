using MinIONet.Service.IServices;
using MinIONet.Service.Models;
using MinIONet.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMinIONetService>(min =>
{
    var config = min.GetRequiredService<IConfiguration>();   
    MinIONetServiceRequest serviceRequest = new MinIONetServiceRequest()
    {
        EndPoint = config["MinIOConfiguration:Endpoint"],
        AccessKey = config["MinIOConfiguration:AccessKey"],
        SecretKey = config["MinIOConfiguration:SecretKey"],
        BucketName= config["MinIOConfiguration:BucketName"],
        IsAcceptHttps= bool.Parse(config["MinIOConfiguration:Secure"]),
        MaxByteFileSize= long.Parse(config["MinIOConfiguration:MaxByteFileSize"]),
        AccessFileContentType = config.GetSection("MinIOConfiguration:AcceptFileContentType").Get<List<string>>()
    };    
    return new MinIONetService(serviceRequest);
});

var app = builder.Build();

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
