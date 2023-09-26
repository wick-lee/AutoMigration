using AutoMigration.WebTest;
using AutoMigration.WebTest.Migration;
using Microsoft.EntityFrameworkCore;
using Wick.AutoMigration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.Bind("NpgsqlAutoMigration", RunTimeConfig.NpgsqlConfig);
builder.Configuration.Bind("MysqlAutoMigration", RunTimeConfig.MysqlConfig);

var connectionString = builder.Configuration.GetConnectionString("NpgsqlConnection");
var mysqlConnectionString = builder.Configuration.GetConnectionString("MysqlConnection");

builder.Services.AddDbContext<NpgsqlDbContext>(options => { options.UseNpgsql(connectionString); });

builder.Services.AddDbContext<MysqlDbContext>(options =>
    options.UseMySql(mysqlConnectionString, new MySqlServerVersion(new Version(8, 0, 27))));

builder.Services.AddAutoMigration<NpgsqlDbContext>(typeof(NpgsqlMigrationSqlProvider),
    typeof(NpgsqlMigrationDbOperation), builder.Configuration, "NpgsqlAutoMigration");

builder.Services.AddAutoMigration<MysqlDbContext>(typeof(MysqlMigrationSqlProvider), typeof(MysqlMigrationDbOperation),
    builder.Configuration, "MysqlAutoMigration");

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