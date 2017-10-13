using System;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Session
{
    public class GetJustIdTests : IntegrationTest
    {
        public GetJustIdTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetReferenceId()
        {
            Guid modelId;
            Guid rltnId;
            using (var session = SessionFactory.CreateSession())
            {
                var model = await session.Create<ComplexModel>();
                modelId = model.Id;

                model.PrimaryBasicModel = new BasicModel();

                await session.Update(model);
                rltnId = model.PrimaryBasicModel.Id;
            }

            using (var session = SessionFactory.CreateSession())
            {
                var persistedModel = await session.Get<ComplexModel>(modelId);

                Assert.Equal(rltnId, persistedModel.PrimaryBasicModelId);
            }

            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete(rltnId);
                await session.Delete(modelId);
            }
        }
    }
}
