using Core.IUnitOfWork;
using Infrastructure.Data;
using API_First_Project.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using Core.Models;
using API_First_Project.Commands;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
#pragma warning disable CS0618 // Type or member is obsolete
builder.Services.AddControllers(options=> {
    // Disable the default data annotation validation
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateUsersCommand>());
#pragma warning restore CS0618 // Type or member is obsolete

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true, 
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Add Authorization
builder.Services.AddAuthorization();


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

// Configure JWT
//builder.Services.Configure<JWT>(Configuration.GetSection("JWT"));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
