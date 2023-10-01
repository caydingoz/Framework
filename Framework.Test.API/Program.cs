using Framework.Domain.Entities;
using Framework.Domain.Interfaces.Repositories;
using Framework.MongoDB;
using Framework.Shared.Entities;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

MongoDbConfiguration mongoDbConfiguration = new();
builder.Configuration.Bind("Configuration:MongoDb", mongoDbConfiguration);
var configuration = new Configuration { MongoDb = mongoDbConfiguration };

builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<IGenericRepositoryWithNonRelation<Log, string>, MongoDbRepositoryBase<Log, string>>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
