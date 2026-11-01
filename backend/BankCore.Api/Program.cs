using BankCore.Api.Data;
using BankCore.Api.Middleware;
using BankCore.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BankCoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BankCore")));
builder.Services.AddScoped<IBankingService, BankingService>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
