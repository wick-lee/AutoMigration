using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wick.AutoMigration;

namespace AutoMigration.WebTest.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly NpgsqlDbContext _dbContext;
    private readonly AutoMigration<NpgsqlDbContext> _autoMigration;
    private readonly AutoMigration<MysqlDbContext> _mysqlAutoMigration;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, NpgsqlDbContext dbContext,
        AutoMigration<NpgsqlDbContext> autoMigration, AutoMigration<MysqlDbContext> mysqlAutoMigration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _autoMigration = autoMigration;
        _mysqlAutoMigration = mysqlAutoMigration;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        await _mysqlAutoMigration.MigrationDbAsync();
        await _autoMigration.MigrationDbAsync();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}