using KSeF.Client.Core.Models.Certificates;
using KSeF.Client.Validation;

namespace KSeF.Client.Api.Builders.Certificates;

/// <summary>
/// Buduje żądanie złożenia wniosku o wystawienie certyfikatu w KSeF.
/// </summary>
public interface ISendCertificateEnrollmentRequestBuilder
{
    /// <summary>
    /// Ustawia nazwę certyfikatu, która będzie widoczna w KSeF.
    /// </summary>
    /// <param name="certificateName">
    /// Nazwa certyfikatu. Musi spełniać wymagania wzorca <see cref="RegexPatterns.CertificateName"/>
    /// oraz długości określonej przez <see cref="ValidValues.CertificateNameMinLength"/>
    /// i <see cref="ValidValues.CertificateNameMaxLength"/>.
    /// </param>
    /// <returns>Interfejs pozwalający ustawić CSR i typ certyfikatu.</returns>
    ISendCertificateEnrollmentRequestBuilderWithCertificateName WithCertificateName(string certificateName);
}

/// <summary>
/// Etap budowy żądania, w którym ustawiana jest nazwa certyfikatu.
/// </summary>
public interface ISendCertificateEnrollmentRequestBuilderWithCertificateName
{
    /// <summary>
    /// Ustawia CSR (Certificate Signing Request), który zostanie przesłany do KSeF.
    /// </summary>
    /// <param name="csr">Treść CSR w formacie wymaganym przez KSeF. Nie może być pusta.</param>
    /// <returns>Interfejs pozwalający opcjonalnie ustawić datę ważności certyfikatu.</returns>
    ISendCertificateEnrollmentRequestBuilderWithCsr WithCsr(string csr);

    /// <summary>
    /// Określa typ wystawianego certyfikatu.
    /// </summary>
    /// <param name="certificateType">Typ certyfikatu zdefiniowany w KSeF.</param>
    /// <returns>Interfejs pozwalający ustawić CSR i datę ważności certyfikatu.</returns>
    ISendCertificateEnrollmentRequestBuilderWithCertificateType WithCertificateType(CertificateType certificateType);
}

/// <summary>
/// Etap budowy żądania, w którym wybrano już typ certyfikatu.
/// </summary>
public interface ISendCertificateEnrollmentRequestBuilderWithCertificateType
{
    /// <summary>
    /// Ustawia CSR (Certificate Signing Request), który zostanie przesłany do KSeF.
    /// </summary>
    /// <param name="csr">Treść CSR w formacie wymaganym przez KSeF. Nie może być pusta.</param>
    /// <returns>Interfejs pozwalający ustawić datę ważności certyfikatu i zbudować żądanie.</returns>
    ISendCertificateEnrollmentRequestBuilderWithCsr WithCsr(string csr);
}

/// <summary>
/// Etap budowy żądania, w którym ustawiono już nazwę, typ i CSR.
/// </summary>
public interface ISendCertificateEnrollmentRequestBuilderWithCsr
{
    /// <summary>
    /// Ustawia datę rozpoczęcia ważności certyfikatu.
    /// </summary>
    /// <param name="validFrom">Data, od której certyfikat ma być ważny.</param>
    /// <returns>Ten sam interfejs, umożliwiający zbudowanie żądania.</returns>
    ISendCertificateEnrollmentRequestBuilderWithCsr WithValidFrom(DateTimeOffset validFrom);

    /// <summary>
    /// Tworzy finalne żądanie złożenia wniosku o certyfikat.
    /// </summary>
    /// <returns>
    /// Obiekt <see cref="SendCertificateEnrollmentRequest"/> gotowy do wysłania do KSeF.
    /// </returns>
    SendCertificateEnrollmentRequest Build();
}

/// <inheritdoc />
internal class SendCertificateEnrollmentRequestBuilderImpl :
    ISendCertificateEnrollmentRequestBuilder,
    ISendCertificateEnrollmentRequestBuilderWithCertificateName,
    ISendCertificateEnrollmentRequestBuilderWithCertificateType,
    ISendCertificateEnrollmentRequestBuilderWithCsr
{
    private string _certificateName;
    private string _csr;
    private DateTimeOffset? _validFrom;
    private CertificateType _certificateType;
    private bool _certificateTypeSet;

    /// <summary>
    /// Tworzy nową instancję buildera wniosku o certyfikat.
    /// </summary>
    /// <returns>Interfejs startowy buildera.</returns>
    public static ISendCertificateEnrollmentRequestBuilder Create() =>
        new SendCertificateEnrollmentRequestBuilderImpl();

    /// <inheritdoc />
    public ISendCertificateEnrollmentRequestBuilderWithCertificateName WithCertificateName(string certificateName)
    {
        if (!RegexPatterns.CertificateName.IsMatch(certificateName))
        {
            throw new ArgumentException("Nazwa certyfikatu zawiera niedozwolone znaki", nameof(certificateName));
        }
        if (certificateName.Length < ValidValues.CertificateNameMinLength)
        {
            throw new ArgumentException($"Nazwa certyfikatu za krótka, minimalna długość: {ValidValues.CertificateNameMinLength} znaków.", nameof(certificateName));
        }
        if (certificateName.Length > ValidValues.CertificateNameMaxLength)
        {
            throw new ArgumentException($"Nazwa certyfikatu za długa, maksymalna długość: {ValidValues.CertificateNameMaxLength} znaków.", nameof(certificateName));
        }

        _certificateName = certificateName;
        return this;
    }

    /// <inheritdoc />
    public ISendCertificateEnrollmentRequestBuilderWithCertificateType WithCertificateType(CertificateType certificateType)
    {
        _certificateType = certificateType;
        _certificateTypeSet = true;
        return this;
    }

    /// <inheritdoc />
    public ISendCertificateEnrollmentRequestBuilderWithCsr WithCsr(string csr)
    {
        if (string.IsNullOrWhiteSpace(csr))
        {
            throw new ArgumentNullException(nameof(csr));
        }

        _csr = csr;
        return this;
    }

    /// <inheritdoc />
    public ISendCertificateEnrollmentRequestBuilderWithCsr WithValidFrom(DateTimeOffset validFrom)
    {
        _validFrom = validFrom;
        return this;
    }

    /// <inheritdoc />
    public SendCertificateEnrollmentRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_certificateName))
        {
            throw new InvalidOperationException("Najpierw należy wywołać WithCertificateName(...).");
        }
        if (!_certificateTypeSet)
        {
            throw new InvalidOperationException("Najpierw należy wywołać WithCertificateType(...).");
        }
        if (string.IsNullOrWhiteSpace(_csr))
        {
            throw new InvalidOperationException("Najpierw należy wywołać WithCsr(...).");
        }

        return new SendCertificateEnrollmentRequest
        {
            CertificateName = _certificateName,
            CertificateType = _certificateType,
            Csr = _csr,
            ValidFrom = _validFrom
        };
    }
}

/// <summary>
/// Udostępnia metodę pomocniczą do tworzenia buildera wniosku o certyfikat.
/// </summary>
public static class SendCertificateEnrollmentRequestBuilder
{
    /// <summary>
    /// Tworzy nowy builder żądania złożenia wniosku o certyfikat.
    /// </summary>
    /// <returns>Interfejs startowy buildera.</returns>
    public static ISendCertificateEnrollmentRequestBuilder Create() =>
        SendCertificateEnrollmentRequestBuilderImpl.Create();
}