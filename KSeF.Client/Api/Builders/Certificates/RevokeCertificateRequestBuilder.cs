using KSeF.Client.Core.Models.Certificates;

namespace KSeF.Client.Api.Builders.Certificates;

/// <summary>
/// Buduje żądanie unieważnienia certyfikatu w KSeF.
/// </summary>
public interface IRevokeCertificateRequestBuilder
{
    /// <summary>
    /// Ustawia powód unieważnienia certyfikatu.
    /// </summary>
    /// <param name="revocationReason">
    /// Powód unieważnienia, zgodny z wartościami <see cref="CertificateRevocationReason"/>.
    /// </param>
    /// <returns>Ten sam builder, umożliwiający zbudowanie żądania.</returns>
    IRevokeCertificateRequestBuilder WithRevocationReason(CertificateRevocationReason revocationReason);

    /// <summary>
    /// Tworzy finalne żądanie unieważnienia certyfikatu.
    /// </summary>
    /// <returns>
    /// Obiekt <see cref="CertificateRevokeRequest"/> gotowy do wysłania do KSeF.
    /// </returns>
    CertificateRevokeRequest Build();
}

/// <inheritdoc />
internal sealed class RevokeCertificateRequestBuilderImpl : IRevokeCertificateRequestBuilder
{
    private CertificateRevocationReason _revocationReason = CertificateRevocationReason.Unspecified;
    private bool _revocationReasonSet;

    /// <summary>
    /// Tworzy nową instancję buildera żądania unieważnienia certyfikatu.
    /// </summary>
    /// <returns>Interfejs buildera.</returns>
    public static IRevokeCertificateRequestBuilder Create() => new RevokeCertificateRequestBuilderImpl();

    /// <inheritdoc />
    public IRevokeCertificateRequestBuilder WithRevocationReason(CertificateRevocationReason revocationReason)
    {
        _revocationReason = revocationReason;
        _revocationReasonSet = true;
        return this;
    }

    /// <inheritdoc />
    public CertificateRevokeRequest Build()
    {
        return new CertificateRevokeRequest
        {
            RevocationReason = _revocationReasonSet ? _revocationReason : CertificateRevocationReason.Unspecified
        };
    }
}

/// <summary>
/// Udostępnia metodę pomocniczą do tworzenia buildera żądania unieważnienia certyfikatu.
/// </summary>
public static class RevokeCertificateRequestBuilder
{
    /// <summary>
    /// Tworzy nowy builder żądania unieważnienia certyfikatu.
    /// </summary>
    /// <returns>Interfejs buildera.</returns>
    public static IRevokeCertificateRequestBuilder Create() =>
        RevokeCertificateRequestBuilderImpl.Create();
}