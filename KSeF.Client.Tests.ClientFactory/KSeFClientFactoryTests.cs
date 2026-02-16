using KSeF.Client.ClientFactory;
using KSeF.Client.ClientFactory.DI;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace KSeF.Client.Tests.ClientFactory
{
    public class KSeFClientFactoryTests
    {
        [Fact]
        public async Task GivenKSefClientFromFacotry_WhenWeCallForAuthChallenge_ThenShouldReturn200()
        {
            // Arrange
            ServiceCollection services = new ServiceCollection();
            services.RegisterKSeFClientFactory();

            ServiceProvider provider = services.BuildServiceProvider();
            IKSeFClientFactory factory = provider.GetRequiredService<IKSeFClientFactory>();

            IKSeFClient testClient = factory.KSeFClient(Client.ClientFactory.Environment.Test);
            IKSeFClient demoClient = factory.KSeFClient(Client.ClientFactory.Environment.Demo);

            // Act
            AuthenticationChallengeResponse challengeOnTestEnvironmet = await testClient.GetAuthChallengeAsync();
            AuthenticationChallengeResponse challengeOnDemoEnvironmet = await demoClient.GetAuthChallengeAsync();

            // Assert
            Assert.NotNull(challengeOnTestEnvironmet);
            Assert.NotNull(challengeOnDemoEnvironmet);
        }

        [Fact]
        public async Task CertificatesChangeTest_ShouldShowDifference()
        {
            // Arrange
            ServiceCollection services = new ServiceCollection();
            services.RegisterKSeFClientFactory();

            ServiceProvider provider = services.BuildServiceProvider();

            IKSeFFactoryCryptographyServices ksefFactoryCryptographyServices = provider.GetRequiredService<IKSeFFactoryCryptographyServices>();
            ICryptographyClient testCryptoClient = ksefFactoryCryptographyServices.CryptographyClient(Client.ClientFactory.Environment.Test);
            ICryptographyClient demoCryptoClient = ksefFactoryCryptographyServices.CryptographyClient(Client.ClientFactory.Environment.Demo);

            // Act
            ICollection<PemCertificateInfo> testCerts = await testCryptoClient.GetPublicCertificatesAsync();
            ICollection<PemCertificateInfo> demoCerts = await demoCryptoClient.GetPublicCertificatesAsync();

            // Assert
            Assert.NotNull(testCerts);
            Assert.NotNull(demoCerts);
            Assert.Equal(testCerts.First().Certificate, demoCerts.First().Certificate);
            Assert.Equal(testCerts.Last().Certificate, demoCerts.Last().Certificate);

        }
    }
}
