using API_First_Project.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Database Config with lazy loading 
builder.Services.AddDbContext<TestingDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("conn")).UseLazyLoadingProxies();
});

//builder.Services.AddScoped<IUserRepository, UserRepository>();  // Registering the Interface and the corresponding Repository

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
