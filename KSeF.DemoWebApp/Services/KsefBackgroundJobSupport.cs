using System.Security.Cryptography.X509Certificates;
using KSeF.Client.Api.Builders.Auth;
using KSeF.Client.Api.Builders.X509Certificates;
using KSeF.Client.Api.Services;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Authorization;

namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Wspólne helpery dla jobów BackgroundService (auth, NIP).
/// </summary>
internal static class KsefBackgroundJobSupport
{
    private const int AuthInProgressCode = 100;
    private const int AuthSuccessCode = 200;
    private const int MaxAuthPollAttempts = 60;

    public static async Task<string> AuthenticateAsync(
        IKSeFClient client,
        string nip,
        CancellationToken cancellationToken)
    {
        AuthenticationChallengeResponse challengeResponse = await client
            .GetAuthChallengeAsync(cancellationToken)
            .ConfigureAwait(false);

        AuthenticationTokenRequest authTokenRequest = AuthTokenRequestBuilder
            .Create()
            .WithChallenge(challengeResponse.Challenge)
            .WithContext(AuthenticationTokenContextIdentifierType.Nip, nip)
            .WithIdentifierType(AuthenticationTokenSubjectIdentifierTypeEnum.CertificateSubject)
            .WithAuthorizationPolicy(null!)
            .Build();

        string unsignedXml = AuthenticationTokenRequestSerializer.SerializeToXmlString(authTokenRequest);

        using X509Certificate2 certificate = SelfSignedCertificateForSignatureBuilder
            .Create()
            .WithGivenName("A")
            .WithSurname("R")
            .WithSerialNumber($"TINPL-{nip}")
            .WithCommonName("A R")
            .AndEncryptionType(EncryptionMethodEnum.Rsa)
            .Build();

        string signedXml = SignatureService.Sign(unsignedXml, certificate);

        SignatureResponse authSubmission = await client
            .SubmitXadesAuthRequestAsync(signedXml, false, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        AuthStatus authStatus = await WaitForAuthCompletionAsync(client, authSubmission, cancellationToken)
            .ConfigureAwait(false);
        if (authStatus.Status.Code != AuthSuccessCode)
        {
            throw new InvalidOperationException(
                $"Uwierzytelnienie nie powiodło się. Kod={authStatus.Status.Code}, opis={authStatus.Status.Description}.");
        }

        AuthenticationOperationStatusResponse tokens = await client
            .GetAccessTokenAsync(authSubmission.AuthenticationToken.Token, cancellationToken)
            .ConfigureAwait(false);

        return tokens.AccessToken.Token;
    }

    private static async Task<AuthStatus> WaitForAuthCompletionAsync(
        IKSeFClient client,
        SignatureResponse authSubmission,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MaxAuthPollAttempts; attempt++)
        {
            AuthStatus status = await client
                .GetAuthStatusAsync(authSubmission.ReferenceNumber, authSubmission.AuthenticationToken.Token, cancellationToken)
                .ConfigureAwait(false);

            if (status.Status.Code != AuthInProgressCode)
            {
                return status;
            }

            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException("Timeout oczekiwania na zakończenie uwierzytelnienia.");
    }
}