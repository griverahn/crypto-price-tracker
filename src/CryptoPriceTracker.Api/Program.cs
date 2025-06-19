using Microsoft.OpenApi.Models;          
using Microsoft.EntityFrameworkCore;     
using MediatR;                          
using FluentValidation;    
using CryptoPriceTracker.Application.Commands.UpdatePrices;
using CryptoPriceTracker.Application;
using CryptoPriceTracker.Infrastructure;
using CryptoPriceTracker.Infrastructure.Persistence;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC + JSON ----------
builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson();

builder.Services.AddControllersWithViews()
       .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);


// ---------- DbContext (SQLite) ----------
var conn = builder.Configuration.GetConnectionString("Default") ?? "Data Source=crypto.db";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(conn));

// ---------- MediatR & Validation ----------
builder.Services.AddMediatR(typeof(UpdatePricesCommand));   
builder.Services.AddValidatorsFromAssembly(typeof(UpdatePricesCommand).Assembly);

// ---------- Application & Infrastructure helpers ----------
builder.Services.AddApplication();          
builder.Services.AddInfrastructure(builder.Configuration); 

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- Build ----------
var app = builder.Build();

// ---------- Middleware ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ---------- Routes ----------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// ---------- Run ----------
app.Run();
