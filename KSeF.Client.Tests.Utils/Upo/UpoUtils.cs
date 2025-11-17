using KSeF.Client.Core.Interfaces.Clients;
using System.Xml.Serialization;

namespace KSeF.Client.Tests.Utils.Upo;

public static class UpoUtils
{
    /// <summary>
    /// Deserializuje XML UPO do obiektu typu T, gdzie T implementuje IUpoParsable.
    /// </summary>
    /// <param name="xml">Pełna reprezentacja XML zgodna ze schematem UPO.</param>
    /// <returns>Wynik deserializacji.</returns>
    /// <exception cref="ArgumentNullException">Gdy parametr xml jest null.</exception>
    /// <exception cref="InvalidOperationException">Gdy deserializacja się nie powiedzie (np. niezgodność elementów/namespaces).</exception>
    public static T UpoParse<T>(string xml) where T : IUpoParsable
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using StringReader reader = new StringReader(xml);

        return (T)serializer.Deserialize(reader)!;
    }

    /// <summary>
    /// Pobiera UPO faktury z sesji na podstawie jej numeru KSeF.
    /// </summary>
    public static async Task<string> GetSessionInvoiceUpoAsync(IKSeFClient ksefClient, string sessionReferenceNumber, string ksefNumber, string accessToken)
    {
        return await ksefClient.GetSessionInvoiceUpoByKsefNumberAsync(sessionReferenceNumber, ksefNumber, accessToken, CancellationToken.None);
    }

    /// <summary>
    /// Pobiera zbiorcze UPO dla sesji na podstawie numeru referencyjnego UPO.
    /// </summary>
    public static async Task<string> GetSessionUpoAsync(IKSeFClient ksefClient, string sessionReferenceNumber, string upoReferenceNumber, string accessToken)
    {
        return await ksefClient.GetSessionUpoAsync(sessionReferenceNumber, upoReferenceNumber, accessToken, CancellationToken.None);
    }

    /// <summary>
    /// Pobiera UPO z adresu Uri.
    /// </summary>
    public static async Task<string> GetUpoAsync(IKSeFClient ksefClient, Uri uri)
    {
        return await ksefClient.GetUpoAsync(uri, CancellationToken.None);
    }
}