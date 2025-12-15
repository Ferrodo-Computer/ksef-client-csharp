using KSeF.Client.ClientFactory;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.ClientFactory.DI;
using Microsoft.Extensions.DependencyInjection;

namespace KSeF.Client.Tests.ClientFactory
{
    public class CertificateFetcherFactoryTests
    {
        private readonly IKSeFFactoryCertificateFetcherServices _factory;

        public CertificateFetcherFactoryTests()
        {
            ServiceCollection services = new ServiceCollection();
            services.RegisterKSeFClientFactory();
            _factory = services.BuildServiceProvider()
                .GetRequiredService<IKSeFFactoryCertificateFetcherServices>();
        }

        [Fact]
        public async Task CertificateFetcher_IsCached_PerEnvironment()
        {
            ICertificateFetcher first = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher second = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);

            Assert.Same(first, second);
        }

        [Fact]
        public async Task CertificateFetcher_IsDifferent_ForDifferentEnvironments()
        {
            ICertificateFetcher demo = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher prod = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Prod);

            Assert.NotSame(demo, prod);
        }

        [Fact]
        public async Task Invalidate_CreatesNewInstance_ForGivenEnvironment()
        {
            // Arrange
            ICertificateFetcher first = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);

            // Act
            _factory.Invalidate(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher second = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);

            // Assert
            Assert.NotSame(first, second);
        }

        [Fact]
        public async Task Invalidate_DoesNotAffectOtherEnvironments()
        {
            // Arrange
            ICertificateFetcher demoFirst = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher prodFirst = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Prod);

            // Act
            _factory.Invalidate(Client.ClientFactory.Environment.Demo);

            ICertificateFetcher demoSecond = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher prodSecond = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Prod);

            // Assert
            Assert.NotSame(demoFirst, demoSecond);
            Assert.Same(prodFirst, prodSecond);   
        }

        [Fact]
        public async Task ClearCache_CreatesNewInstances_ForAllEnvironments()
        {
            // Arrange
            ICertificateFetcher demoFirst = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher prodFirst = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Prod);
            ICertificateFetcher testFirst = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Test);

            // Act
            _factory.ClearCache();

            ICertificateFetcher demoSecond = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            ICertificateFetcher prodSecond = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Prod);
            ICertificateFetcher testSecond = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Test);

            // Assert
            Assert.NotSame(demoFirst, demoSecond);
            Assert.NotSame(prodFirst, prodSecond);
            Assert.NotSame(testFirst, testSecond);
        }

        [Fact]
        public async Task RefreshAsync_CreatesNewInstance_AndCachesIt()
        {
            // Arrange – pierwszy fetcher z cache
            ICertificateFetcher first = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);

            // Act – odświeżamy cache dla Demo (Invalidate + nowy fetcher)
            ICertificateFetcher refreshed = await _factory.RefreshAsync(Client.ClientFactory.Environment.Demo);

            // Assert 1 – RefreshAsync tworzy nową instancję
            Assert.NotSame(first, refreshed);

            // Assert 2 – nowa instancja jest zcache’owana (kolejne GetOrSet zwraca tę samą)
            ICertificateFetcher third = await _factory.GetOrSetCertificateFetcher(Client.ClientFactory.Environment.Demo);
            Assert.Same(refreshed, third);
        }
    }
}