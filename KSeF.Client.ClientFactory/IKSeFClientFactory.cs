
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Http;

namespace KSeF.Client.ClientFactory
{
    /// <summary>
    /// Fabryka odpowiedzialna za tworzenie klientów komunikujących się z usługami KSeF.
    /// </summary>
    /// <remarks>
    /// Implementacja umożliwia tworzenie instancji <see cref="IKSeFClient"/> 
    /// na podstawie wskazanego środowiska systemu KSeF (Test, Demo, Prod).
    /// Dzięki temu możliwa jest łatwa konfiguracja komunikacji przy użyciu
    /// różnych adresów bazowych oraz ustawień sieciowych.
    /// </remarks>
    public interface IKSeFClientFactory
    {
        /// <summary>
        /// Tworzy instancję klienta KSeF dostosowaną do podanego środowiska.
        /// </summary>
        /// <param name="environment">
        /// Środowisko KSeF, dla którego ma zostać utworzony klient 
        /// (<see cref="Environment.Test"/>, <see cref="Environment.Demo"/>, <see cref="Environment.Prod"/>).
        /// </param>
        /// <returns>
        /// Nowa instancja <see cref="IKSeFClient"/> skonfigurowana dla wskazanego środowiska.
        /// </returns>
        /// <example>
        /// Przykład użycia:
        /// <code>
        /// IKSeFClient client = factory.KSeFClient(Environment.Test);
        /// </code>
        /// </example>
        IKSeFClient KSeFClient(Environment environment);
    }

    /// <summary>
    /// Implementacja fabryki tworzącej klientów KSeF w oparciu o wstrzykniętą fabrykę <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <remarks>
    /// Fabryka korzysta z nazwanego <c>HttpClient</c>, którego nazwa jest zgodna z nazwą środowiska.
    /// </remarks>
    public class KSeFClientFactory(IHttpClientFactory factory) : IKSeFClientFactory
    {
        /// <summary>
        /// Tworzy instancję klienta KSeF dla podanego środowiska.
        /// </summary>
        /// <param name="environment">Środowisko KSeF, którego klient ma zostać utworzony.</param>
        /// <returns>Nowo utworzony klient <see cref="IKSeFClient"/>.</returns>
        /// <remarks>
        /// Instancja <see cref="RestClient"/> jest tworzona na podstawie klienta HTTP
        /// o nazwie odpowiadającej wybranemu środowisku.
        /// </remarks>
        public IKSeFClient KSeFClient(Environment environment)
        {
            RestClient restClient = new RestClient(factory.CreateClient(environment.ToString()));   
            return new global::KSeF.Client.Clients.KSeFClient(restClient);
        }
    }
}
