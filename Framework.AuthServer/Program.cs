using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Framework.AuthServer.Models;
using Framework.AuthServer.Services;
using Framework.AuthServer.Interfaces.Services;
using Framework.Shared.Entities.Configurations;
using Framework.AuthServer;
using Framework.Domain.Interfaces.Repositories;
using Framework.MongoDB;
using Framework.Domain.Entities;
using Framework.Shared.Helpers;
using Framework.AuthServer.Enums;
using Framework.EF.Interceptors;
using Framework.EF;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Repositories;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var configuration = new Configuration 
{ 
    JWT = new JWTConfiguration(), 
    EF = new EFConfiguration(), 
    MongoDb = new MongoDbConfiguration(), 
    Redis = new RedisConfiguration() 
};
builder.Configuration.Bind("Configuration:JWT", configuration.JWT);
builder.Configuration.Bind("Configuration:EF", configuration.EF);
builder.Configuration.Bind("Configuration:MongoDb", configuration.MongoDb);
builder.Configuration.Bind("Configuration:Redis", configuration.Redis);

RedisConnectorHelper.Configuration = configuration; //TODO: Look for a pattern
Console.WriteLine("Environment: " + builder.Environment.EnvironmentName);

builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<ITokenHandlerService, TokenHandlerService>(); 
builder.Services.AddSingleton<DefaultDataMigration>();
builder.Services.AddScoped<IGenericRepository<User, Guid>, EfCoreRepositoryBase<User, AuthServerDbContext, Guid>>();
builder.Services.AddScoped<IGenericRepository<Role, int>, EfCoreRepositoryBase<Role, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Permission, int>, EfCoreRepositoryBase<Permission, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<WorkItem, int>, EfCoreRepositoryBase<WorkItem, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Activity, int>, EfCoreRepositoryBase<Activity, AuthServerDbContext, int>>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();

builder.Services.AddScoped<IGenericRepositoryWithNonRelation<Log, string>, MongoDbRepositoryBase<Log, string>>();

if(configuration.JWT is not null)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = configuration.JWT.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = configuration.JWT.ValidIssuer,
            ValidateAudience = configuration.JWT.ValidateAudience,
            ValidateLifetime = configuration.JWT.ValidateLifetime,
            ValidateIssuerSigningKey = configuration.JWT.ValidateIssuerSigningKey,
            ClockSkew = TimeSpan.Zero,

            ValidAudience = configuration.JWT.ValidAudience,
            ValidateIssuer = configuration.JWT.ValidateIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.JWT.Secret)),
            RequireExpirationTime = configuration.JWT.RequireExpirationTime,
        };
    });
}
builder.Services.AddAuthorization(PermissionHelper.SetPolicies(Enum.GetNames(typeof(Operations))));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opt => opt.AddPolicy(name: "CorsPolicy", builder => builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddHealthChecks().AddDbContextCheck<AuthServerDbContext>("Sql Server");
builder.Services.AddHealthChecks().AddRedis(configuration.Redis.ConnectionString, name: "Redis", failureStatus: HealthStatus.Unhealthy);

if (configuration.EF is not null)
{
    builder.Services.AddDbContext<AuthServerDbContext>(options => 
    {
        options.UseSqlServer(configuration.EF.ConnectionString);
        options.AddInterceptors(new SlowQueryInterceptor());
    }, ServiceLifetime.Scoped);
}

static void Migrate(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var migrator = new DefaultDataMigration(serviceProvider);
        migrator.EnsureMigrationAsync().Wait();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Migrate(app);

app.Run();
