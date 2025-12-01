using Domain.Entities;
using Infrastructure.Data; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Application.Services.Interfaces;
using Domain.Repositories.Interfaces;
using Infrastructure.Repositories;
using Application.Services.Auth;
using Application.Maps;
using Microsoft.Extensions.DependencyInjection;
using Application.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
 
builder.Services.AddEndpointsApiExplorer();
 
builder.Services.AddControllers();
 
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

// Configure Identity services (must come before JWT services)
builder.Services.AddIdentityServices(builder.Configuration);

// Configure Swagger/OpenAPI using extension method
builder.Services.AddSwaggerServices();

// Configure Repository services using extension method
builder.Services.AddRepositoryServices();

// Configure Business services using extension method
builder.Services.AddServiceDependencies();

// Configure File services (must be registered before Auth service)
builder.Services.AddFileService();

// Configure Auth services
builder.Services.AddAuthService();

// Registration of JWT services and related authentication.
builder.Services.AddJwtServices(builder.Configuration);

// Configure Email services using extension method.
builder.Services.AddEmailServices(builder.Configuration);

// Adding the AutoMapper service.
builder.Services.AddAutoMapperServices();


// Configure Data Seed services.
builder.Services.AddDataSeedServices();

var app = builder.Build();


// --- Configure (Middleware Pipeline) ---

// Seed database data
await app.SeedDatabaseAsync();


// Configure the Roles using extension
//await app.SeedRolesAsync();

// Configure the Swagger middleware using extension method
app.UseSwaggerMiddleware();

app.UseHttpsRedirection();

// This middleware enables serving static files 
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

 
