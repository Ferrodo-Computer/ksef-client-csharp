using KSeF.Client.Api.Builders.Auth;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Token;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Authorization;

public class AuthorizationE2ETests : TestBase
{
    private const string OwnerRole = "owner";
    private const string ChallengeToken = "samplechallengeToken!@#1231234567890";

    /// <summary>
    /// Uwierzytelnia klienta KSeF i sprawdza, czy zwrócony token dostępu jest poprawny
    /// </summary>
    [Theory]
    [InlineData(EncryptionMethodEnum.Rsa)]
    [InlineData(EncryptionMethodEnum.ECDsa)]
    public async Task AuthAsyncFullIntegrationFlowReturnsAccessToken(EncryptionMethodEnum encryptionMethodEnum)
    {
        // Arrange & Act
        AuthenticationOperationStatusResponse authResult =
            await AuthenticationUtils.AuthenticateAsync(AuthorizationClient, default, encryptionMethodEnum);

        // Assert
        Assert.NotNull(authResult);
        Assert.NotNull(authResult.AccessToken);
        Assert.NotNull(authResult.RefreshToken);

        PersonToken personToken = TokenService.MapFromJwt(authResult.AccessToken!.Token!);
        Assert.NotNull(personToken);
        Assert.Null(personToken.IpPolicy);
        Assert.True(!string.IsNullOrWhiteSpace(personToken.Issuer));
        Assert.NotNull(personToken.Audiences);
        Assert.NotNull(personToken.IssuedAt);
        Assert.NotNull(personToken.ExpiresAt);
        Assert.NotNull(personToken.Roles);
        Assert.True(!string.IsNullOrWhiteSpace(personToken.TokenType));
        Assert.NotNull(personToken.ContextIdType);
        Assert.NotNull(personToken.ContextIdValue);
        Assert.NotNull(personToken.AuthMethod);
        Assert.True(!string.IsNullOrWhiteSpace(personToken.AuthRequestNumber));
        Assert.NotNull(personToken.SubjectDetails);
        Assert.NotNull(personToken.Permissions);
        Assert.NotNull(personToken.PermissionsExcluded);
        Assert.NotNull(personToken.RolesRaw);
        Assert.NotNull(personToken.PermissionsEffective);
        Assert.Contains(OwnerRole, personToken.Roles, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Budowanie żądania tokenu autoryzacyjnego.
    /// </summary>
    [Fact]
    public void AuthTokenRequestBuilderCreateShouldReturnObject()
    {
        AuthenticationTokenContextIdentifierType contextIdentifierType = 
            AuthenticationTokenContextIdentifierType.Nip;
        string nip = MiscellaneousUtils.GetRandomNip();
        AuthenticationTokenSubjectIdentifierTypeEnum subjectIdentifierTypeEnum = AuthenticationTokenSubjectIdentifierTypeEnum.CertificateSubject;

        AuthenticationTokenRequest authTokenRequest = AuthTokenRequestBuilder
           .Create()
           .WithChallenge(ChallengeToken)
           .WithContext(contextIdentifierType, nip)
           .WithIdentifierType(subjectIdentifierTypeEnum)
           .WithAuthorizationPolicy(null)
           .Build();

        Assert.NotNull(authTokenRequest);
        Assert.Equal(ChallengeToken, authTokenRequest.Challenge);
        Assert.Equal(contextIdentifierType, authTokenRequest.ContextIdentifier.Type);
        Assert.Equal(nip, authTokenRequest.ContextIdentifier.Value);
        Assert.Equal(subjectIdentifierTypeEnum, authTokenRequest.SubjectIdentifierType);
        Assert.Null(authTokenRequest.AuthorizationPolicy);
    }
}

