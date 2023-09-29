using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Framework.AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Framework.AuthServer.Services;
using Framework.AuthServer.Interfaces.Services;
using Framework.Shared.Entities;
using Framework.AuthServer;
using Framework.EF;
using Framework.Domain.Interfaces.Repositories;

var builder = WebApplication.CreateBuilder(args);

JWTConfiguration jwtConfiguration = new();
EFConfiguration efConfiguration = new();
builder.Configuration.Bind("Configuration:JWT", jwtConfiguration);
builder.Configuration.Bind("Configuration:EF", efConfiguration);

var configuration = new Configuration { JWT = jwtConfiguration, EF = efConfiguration };

builder.Services.AddSingleton(configuration); 
builder.Services.AddScoped<ITokenHandlerService, TokenHandlerService>(); 
builder.Services.AddScoped<IGenericRepository<UserRefreshToken, int>, EfCoreRepositoryBase<UserRefreshToken, AuthServerDbContext, int>>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = jwtConfiguration.RequireHttpsMetadata;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = jwtConfiguration.ValidIssuer,
        ValidateAudience = jwtConfiguration.ValidateAudience,
        ValidateLifetime = jwtConfiguration.ValidateLifetime,
        ValidateIssuerSigningKey = jwtConfiguration.ValidateIssuerSigningKey,
        ClockSkew = TimeSpan.Zero,

        ValidAudience = jwtConfiguration.ValidAudience,
        ValidateIssuer = jwtConfiguration.ValidateIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfiguration.Secret)),
        RequireExpirationTime = jwtConfiguration.RequireExpirationTime,
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AuthServerDbContext>(
    options => options.UseSqlServer(configuration.EF.ConnectionString), ServiceLifetime.Scoped);

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
  .AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<AuthServerDbContext>()
  .AddDefaultTokenProviders();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();