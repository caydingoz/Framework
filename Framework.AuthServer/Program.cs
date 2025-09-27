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
using Framework.AuthServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Framework.Shared.Middlewares;

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
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSingleton<DefaultDataMigration>();
builder.Services.AddSingleton<ConnectedUserService>();

builder.Services.AddScoped<IGenericRepository<User, Guid>, EfCoreRepositoryBase<User, AuthServerDbContext, Guid>>();
builder.Services.AddScoped<IGenericRepository<Role, int>, EfCoreRepositoryBase<Role, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Permission, int>, EfCoreRepositoryBase<Permission, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<WorkItem, int>, EfCoreRepositoryBase<WorkItem, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Activity, int>, EfCoreRepositoryBase<Activity, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Absence, int>, EfCoreRepositoryBase<Absence, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<NotificationUser, int>, EfCoreRepositoryBase<NotificationUser, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Job, int>, EfCoreRepositoryBase<Job, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Applicant, int>, EfCoreRepositoryBase<Applicant, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<ApplicantDocument, int>, EfCoreRepositoryBase<ApplicantDocument, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Interview, int>, EfCoreRepositoryBase<Interview, AuthServerDbContext, int>>();
builder.Services.AddScoped<IGenericRepository<Scorecard, int>, EfCoreRepositoryBase<Scorecard, AuthServerDbContext, int>>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

builder.Services.AddScoped<IGenericRepositoryWithNonRelation<Log, string>, MongoDbRepositoryBase<Log, string>>();

if (configuration.JWT is not null)
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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });
}
builder.Services.AddAuthorization(PermissionHelper.SetPolicies(Enum.GetNames(typeof(Operations))));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
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

var app = builder.Build();

app.UseRouting();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notification");

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
app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();

app.MapControllers();

Migrate(app);

app.Run();
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
