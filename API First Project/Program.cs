using Core.IUnitOfWork;
using Infrastructure.Data;
using API_First_Project.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Config with lazy loading 
builder.Services.AddDbContext<TestingDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("conn")).UseLazyLoadingProxies();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure Cashing
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
