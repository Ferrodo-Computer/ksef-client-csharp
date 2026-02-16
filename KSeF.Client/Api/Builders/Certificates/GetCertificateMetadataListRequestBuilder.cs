using KSeF.Client.Core.Models.Certificates;
using KSeF.Client.Validation;

namespace KSeF.Client.Api.Builders.Certificates;

/// <summary>
/// Buduje żądanie pobrania listy certyfikatów wraz z metadanymi z KSeF.
/// </summary>
public interface IGetCertificateMetadataListListRequestBuilder
{
    /// <summary>
    /// Filtruje wynik po numerze seryjnym certyfikatu.
    /// </summary>
    /// <param name="serialNumber">Numer seryjny certyfikatu.</param>
    /// <returns>Ten sam builder, umożliwiający dalsze filtrowanie lub zbudowanie żądania.</returns>
    IGetCertificateMetadataListListRequestBuilder WithCertificateSerialNumber(string serialNumber);

    /// <summary>
    /// Filtruje wynik po nazwie certyfikatu.
    /// </summary>
    /// <param name="name">
    /// Nazwa certyfikatu. Musi spełniać wymagania wzorca <see cref="RegexPatterns.CertificateName"/>
    /// oraz długości określonej przez <see cref="ValidValues.CertificateNameMinLength"/>
    /// i <see cref="ValidValues.CertificateNameMaxLength"/>.
    /// </param>
    /// <returns>Ten sam builder, umożliwiający dalsze filtrowanie lub zbudowanie żądania.</returns>
    IGetCertificateMetadataListListRequestBuilder WithName(string name);

    /// <summary>
    /// Filtruje wynik po statusie certyfikatu.
    /// </summary>
    /// <param name="status">Status certyfikatu (np. aktywny, unieważniony).</param>
    /// <returns>Ten sam builder, umożliwiający dalsze filtrowanie lub zbudowanie żądania.</returns>
    IGetCertificateMetadataListListRequestBuilder WithStatus(CertificateStatusEnum status);

    /// <summary>
    /// Uwzględnia tylko certyfikaty, które wygasają po wskazanej dacie.
    /// </summary>
    /// <param name="expiresAfter">Data graniczna ważności certyfikatu.</param>
    /// <returns>Ten sam builder, umożliwiający dalsze filtrowanie lub zbudowanie żądania.</returns>
    IGetCertificateMetadataListListRequestBuilder WithExpiresAfter(DateTimeOffset expiresAfter);

    /// <summary>
    /// Filtruje wynik po typie certyfikatu.
    /// </summary>
    /// <param name="certificateType">Typ certyfikatu zdefiniowany w KSeF.</param>
    /// <returns>Ten sam builder, umożliwiający dalsze filtrowanie lub zbudowanie żądania.</returns>
    IGetCertificateMetadataListListRequestBuilder WithCertificateType(CertificateType certificateType);

    /// <summary>
    /// Tworzy finalne żądanie pobrania listy certyfikatów z ustawionymi filtrami.
    /// </summary>
    /// <returns>
    /// Obiekt <see cref="CertificateMetadataListRequest"/> gotowy do wysłania do KSeF.
    /// </returns>
    CertificateMetadataListRequest Build();
}

/// <inheritdoc />
internal class GetCertificateMetadataListRequestBuilderImpl : IGetCertificateMetadataListListRequestBuilder
{
    private readonly CertificateMetadataListRequest _request = new();

    /// <summary>
    /// Tworzy nową instancję buildera żądania listy certyfikatów.
    /// </summary>
    /// <returns>Interfejs buildera.</returns>
    public static IGetCertificateMetadataListListRequestBuilder Create() => new GetCertificateMetadataListRequestBuilderImpl();

    /// <inheritdoc />
    public IGetCertificateMetadataListListRequestBuilder WithCertificateSerialNumber(string serialNumber)
    {
        _request.CertificateSerialNumber = serialNumber;
        return this;
    }

    /// <inheritdoc />
    public IGetCertificateMetadataListListRequestBuilder WithName(string name)
    {
        if (!RegexPatterns.CertificateName.IsMatch(name))
        {
            throw new ArgumentException("Nazwa certyfikatu zawiera niedozwolone znaki", nameof(name));
        }
        if (name.Length < ValidValues.CertificateNameMinLength)
        {
            throw new ArgumentException($"Nazwa certyfikatu za krótka, minimalna długość: {ValidValues.CertificateNameMinLength} znaków.", nameof(name));
        }
        if (name.Length > ValidValues.CertificateNameMaxLength)
        {
            throw new ArgumentException($"Nazwa certyfikatu za długa, maksymalna długość: {ValidValues.CertificateNameMaxLength} znaków.", nameof(name));
        }

        _request.Name = name;
        return this;
    }

    /// <inheritdoc />
    public IGetCertificateMetadataListListRequestBuilder WithStatus(CertificateStatusEnum status)
    {
        _request.Status = status;
        return this;
    }

    /// <inheritdoc />
    public IGetCertificateMetadataListListRequestBuilder WithExpiresAfter(DateTimeOffset expiresAfter)
    {
        _request.ExpiresAfter = expiresAfter;
        return this;
    }

    /// <inheritdoc />
    public IGetCertificateMetadataListListRequestBuilder WithCertificateType(CertificateType certificateType)
    {
        _request.Type = certificateType;
        return this;
    }

    /// <inheritdoc />
    public CertificateMetadataListRequest Build()
    {
        return _request;
    }
}

/// <summary>
/// Udostępnia metodę pomocniczą do tworzenia buildera zapytania o listę certyfikatów.
/// </summary>
public static class GetCertificateMetadataListRequestBuilder
{
    /// <summary>
    /// Tworzy nowy builder żądania pobrania listy certyfikatów.
    /// </summary>
    /// <returns>Interfejs buildera.</returns>
    public static IGetCertificateMetadataListListRequestBuilder Create() =>
        GetCertificateMetadataListRequestBuilderImpl.Create();
}