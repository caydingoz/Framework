using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Framework.Test.Repository
{
    public class GenericRepositoryTests : BaseTest<TestDbContext>
	{
		protected override void AddServices(ServiceCollection services)
		{
			services.AddTransient<IUpdateTestRepository, UpdateTestRepository>();
		}
		[Fact]
		public async Task Update_Second_Layer_Child_Prop_Should_Work()
		{
			var repo = GetService<IUpdateTestRepository>();

			var data = new Parent
			{
				FirstLayerChilds = new List<FirstLayerChild>
				{
					new FirstLayerChild
					{
						SecondLayerChilds = new List<SecondLayerChild>()
						{
							new SecondLayerChild
							{
								ChildProp = "inserted1"
							},
							new SecondLayerChild
							{
								ChildProp = "inserted4"
							},
						}
					}
				}
			};

			var updatedChild = new List<SecondLayerChild>()
			{
				new SecondLayerChild
				{
					ChildProp = "updated1"
				},
			};

			try
			{
				await repo.InsertOneAsync(data);

				var existingData = await repo.GetByIdAsync(data.Id);

				existingData.FirstLayerChilds.First().SecondLayerChilds = updatedChild;

				await repo.UpdateOneAsync(existingData);

				var updatedData = await repo.GetByIdAsync(data.Id);

				Assert.Equal(updatedChild.Count, updatedData.FirstLayerChilds.First().SecondLayerChilds.Count);
				Assert.Equal(updatedChild.FirstOrDefault().ChildProp, updatedData.FirstLayerChilds.First().SecondLayerChilds.First().ChildProp);
			}
			finally
			{
				await repo.DeleteManyAsync((await repo.GetAllAsync()).Select(x => x.Id));
			}
		}
	}
}
