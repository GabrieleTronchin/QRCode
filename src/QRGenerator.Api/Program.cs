using QRCodeService;
using QRGenerator.Api.Endpoint;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddQRCodeService();

builder.Services.AddEndpoints([typeof(RequestsEndpoint).Assembly, typeof(IEndpoint).Assembly]);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
