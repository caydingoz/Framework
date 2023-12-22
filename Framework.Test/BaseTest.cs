using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Framework.Test
{
    public abstract class BaseTest<TDbContext> where TDbContext : DbContext
	{
		IServiceProvider ServiceProvider { get; }
		TDbContext Db { get; set; }

		public BaseTest()
		{
			var services = new ServiceCollection();
			AddServices(services);
			services.AddDbContext<TDbContext>(options =>
			{
				var connString = "DataSource=file::memory:?cache=shared";//"Data Source=:memory:";
				options.UseSqlite(connString);
			}, ServiceLifetime.Singleton);
			ServiceProvider = services.BuildServiceProvider();

			PostServiceCollecting();
		}

		private void PostServiceCollecting()
		{
			CreateDb();
		}

		protected void CreateDb()
		{
			Db = GetService<TDbContext>();
			var x = Db.Database.EnsureDeleted();
			x = Db.Database.EnsureCreated();
		}

		protected TDbContext GetDbContext() => Db;

		protected virtual void AddServices(ServiceCollection services) { }
		protected T GetService<T>() => ServiceProvider.GetService<T>() ?? throw new Exception();

		~BaseTest()
		{
			var db = GetDbContext();
			db.Database.EnsureDeleted();
		}
	}
}
