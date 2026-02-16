using KSeF.Client.ClientFactory;
using KSeF.Client.ClientFactory.DI;
using KSeF.Client.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KSeF.Client.Tests.ClientFactory
{
    public class CryptographyServiceFactoryTests
    {
        [Fact]
        public async Task CryptographyService_IsWarmedUp()
        {
            ServiceCollection services = new ServiceCollection();
            services.RegisterKSeFClientFactory();

            IKSeFFactoryCryptographyServices factory = services.BuildServiceProvider()
                .GetRequiredService<IKSeFFactoryCryptographyServices>();

            ICryptographyService cyptographyService = await factory.CryprographyService(Client.ClientFactory.Environment.Test);

            bool isWarmedUp = cyptographyService.IsWarmedUp();

            Assert.True(isWarmedUp);
        }
    }
}