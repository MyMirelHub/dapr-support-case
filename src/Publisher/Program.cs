using Microsoft.AspNetCore.Mvc;
using Dapr;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddDapr(); 

var app = builder.Build();

app.MapControllers();

app.Run();
