using AutoMigration.WebTest;
using AutoMigration.WebTest.Migration;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Wick.AutoMigration;
using Wick.AutoMigration.Extensions;
using Wick.AutoMigration.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("NpgsqlCon");
builder.Services.AddDbContext<MyDbContext>(options => { options.UseNpgsql(connectionString); });
builder.Services.AddAutoMigration<MyDbContext>(typeof(NpgsqlMigrationSqlProvider), typeof(NpgsqlMigrationDbOperation));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();