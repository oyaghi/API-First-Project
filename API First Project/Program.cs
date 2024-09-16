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
using Infrastructure.Services.TenantIdGetter;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#pragma warning disable CS0618 // Type or member is obsolete
builder.Services.AddControllers(options => {
    // Disable the default data annotation validation
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.AddFluentValidation(fv =>
{
    // Register validators from the assembly containing CreateUsersCommand
    fv.RegisterValidatorsFromAssemblyContaining<CreateUsersCommand>();

    // Register validators from the assembly containing UpdateUserCommand
    fv.RegisterValidatorsFromAssemblyContaining<UpdateUserCommand>();
});
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
builder.Services.AddDbContext<TestingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("conn"),
        b => b.MigrationsAssembly("Infrastructure")).UseLazyLoadingProxies());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure Cashing  (IN progress)(Not used yet)
builder.Services.AddMemoryCache();

// IHttpClientFactory Regiser 
builder.Services.AddHttpClient();

// Config HttpClient Service 
builder.Services.AddScoped<CatFactService>();


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
