using Framework.Domain.Interfaces.Repositories;
using Framework.Domain.Interfaces.UnitOfWork;
using Framework.EF;
using Framework.MongoDB;
using Framework.Shared.Entities.Configurations;
using Framework.Test.API;
using Framework.Test.API.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var configuration = new Configuration
{
    EF = new EFConfiguration(),
    MongoDb = new MongoDbConfiguration(),
    Redis = new RedisConfiguration()
};

builder.Configuration.Bind("Configuration:EF", configuration.EF);
builder.Configuration.Bind("Configuration:MongoDb", configuration.MongoDb);
builder.Configuration.Bind("Configuration:Redis", configuration.Redis);
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<IGenericRepositoryWithNonRelation<NoSqlTestModel, string>, MongoDbRepositoryBase<NoSqlTestModel, string>>();
builder.Services.AddScoped<IGenericRepositoryWithNonRelation<CachableTestModel, int>, MongoDbRepositoryBase<CachableTestModel, int>>();
builder.Services.AddScoped<IGenericRepository<SqlTestModel, int>, EfCoreRepositoryBase<SqlTestModel, TestDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<CachableTestModel, int>, EfCoreRepositoryBase<CachableTestModel, TestDbContext, int>>();
builder.Services.AddScoped<IUnitOfWork<TestDbContext>, UnitOfWork<TestDbContext>>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opt => opt.AddPolicy(name: "CorsPolicy", builder => builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

if (configuration.EF is not null)
{
    builder.Services.AddDbContext<TestDbContext>(
        options => options.UseSqlServer(configuration.EF.ConnectionString), ServiceLifetime.Scoped);
}

static void Migrate(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var context = serviceProvider.GetRequiredService<TestDbContext>();
        context.Database.MigrateAsync().Wait();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw new Exception("An error occurred while migrating the database. Err: " + ex.Message);
    }
}

var app = builder.Build();

app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials());
}

app.MapControllers();

Migrate(app);

app.Run();
