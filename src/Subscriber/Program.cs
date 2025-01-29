using Microsoft.AspNetCore.Mvc;
using Dapr;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.MapSubscribeHandler();

app.Run();