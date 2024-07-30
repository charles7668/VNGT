using GameManager.DB;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
});

var app = builder.Build();


app.Run();