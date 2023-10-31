using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Framework.AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Framework.AuthServer.Services;
using Framework.AuthServer.Interfaces.Services;
using Framework.Shared.Entities.Configurations;
using Framework.AuthServer;
using Framework.EF;
using Framework.Domain.Interfaces.Repositories;
using Framework.MongoDB;
using Framework.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

var configuration = new Configuration { JWT = new JWTConfiguration(), EF = new EFConfiguration(), MongoDb = new MongoDbConfiguration() };
builder.Configuration.Bind("Configuration:JWT", configuration.JWT);
builder.Configuration.Bind("Configuration:EF", configuration.EF);
builder.Configuration.Bind("Configuration:MongoDb", configuration.MongoDb);

builder.Services.AddSingleton(configuration); 
builder.Services.AddScoped<ITokenHandlerService, TokenHandlerService>(); 
builder.Services.AddScoped<IGenericRepository<UserRefreshToken, int>, EfCoreRepositoryBase<UserRefreshToken, AuthServerDbContext, int>>();
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

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opt => opt.AddPolicy(name: "CorsPolicy", builder => builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

if (configuration.EF is not null)
{
    builder.Services.AddDbContext<AuthServerDbContext>(
        options => options.UseSqlServer(configuration.EF.ConnectionString), ServiceLifetime.Scoped);

    builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<AuthServerDbContext>()
      .AddDefaultTokenProviders();
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

app.Run();