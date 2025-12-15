using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.OnlineSession;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Features
{
    [CollectionDefinition("Invoice.feature")]
    [Trait("Category", "Features")]
    [Trait("Features", "Invoice.feature")]
    public partial class InvoiceTests : KsefIntegrationTestBase
    {
        private string authToken { get; set; }
        private string nip { get; set; }

        public InvoiceTests()
        {
            nip = MiscellaneousUtils.GetRandomNip();
            Core.Models.Authorization.AuthenticationOperationStatusResponse authInfo = AuthenticationUtils.AuthenticateAsync(AuthorizationClient, nip).GetAwaiter().GetResult();
            authToken = authInfo.AccessToken.Token;
        }

        [Theory]
        [InlineData(SystemCode.FA2, "invoice-template-fa-2.xml")]
        [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
        [Trait("Scenario", "Posiadając uprawnienie właścicielskie pytamy o fakturę wysłaną")]
        public async Task GivenNewInvoiceSendedToKsefThenReturnsNewKsefNumber(SystemCode systemCode, string invoiceTemplatePath)
        {
            // authenticated in constructor
            Assert.NotNull(authToken);

            // proceed with invoice online session
            Core.Models.Sessions.EncryptionData encryptionData = CryptographyService.GetEncryptionData();

            Core.Models.Sessions.OnlineSession.OpenOnlineSessionResponse openSessionRequest = await OnlineSessionUtils.OpenOnlineSessionAsync(KsefClient,
                encryptionData,
                authToken,
                systemCode);
            Assert.NotNull(openSessionRequest);
            Assert.NotNull(openSessionRequest.ReferenceNumber);

            Core.Models.Sessions.OnlineSession.SendInvoiceResponse sendInvoiceResponse = await OnlineSessionUtils.SendInvoiceAsync(KsefClient,
                openSessionRequest.ReferenceNumber,
                authToken,
                nip,
                invoiceTemplatePath,
                encryptionData,
                CryptographyService);
            Assert.NotNull(sendInvoiceResponse);
            Assert.NotNull(sendInvoiceResponse.ReferenceNumber);

            try
            {
                Core.Models.Sessions.SessionInvoice sendInvoiceStatus = await OnlineSessionUtils.GetSessionInvoiceStatusAsync(KsefClient,
                    openSessionRequest.ReferenceNumber,
                    sendInvoiceResponse.ReferenceNumber,
                    authToken);
                Assert.NotNull(sendInvoiceStatus);
                Assert.Equal(200, sendInvoiceStatus.Status.Code);

                Core.Models.Sessions.SessionInvoicesResponse sessionInvoices = await OnlineSessionUtils.GetSessionInvoicesMetadataAsync(KsefClient,
                openSessionRequest.ReferenceNumber,
                authToken);
                Assert.NotNull(sessionInvoices);
                Assert.NotEmpty(sessionInvoices.Invoices);

                foreach (Core.Models.Sessions.SessionInvoice? item in sessionInvoices.Invoices)
                {
                    string invoiceMetadata = await OnlineSessionUtils.GetSessionInvoiceUpoAsync(KsefClient,
                    openSessionRequest.ReferenceNumber,
                    item.KsefNumber,
                    authToken);
                    Assert.NotNull(invoiceMetadata);

                    await Task.Delay(60000);

                    string invoice = await KsefClient.GetInvoiceAsync(item.KsefNumber, authToken);
                    Assert.NotNull(invoice);
                }
            }
            finally
            {
                await OnlineSessionUtils.CloseOnlineSessionAsync(KsefClient, openSessionRequest.ReferenceNumber, authToken);
            }
        }

        [Theory]
        [InlineData(SystemCode.FA2, "invoice-template-fa-2.xml")]
        [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
        [Trait("Scenario", "Posiadając uprawnienie właścicielskie wysyłamy szyfrowaną fakturę z nieprawidłowym numerem NIP sprzedawcy")]
        public async Task GivenInvalidNewInvoiceSendedToKsefThenReturnsErrorInvalidKsefNumber(SystemCode systemCode, string invoiceTemplatePath)
        {
            string wrongNIP = MiscellaneousUtils.GetRandomNip();

            // authenticated in constructor
            Assert.NotNull(authToken);

            // proceed with invoice online session
            Core.Models.Sessions.EncryptionData encryptionData = CryptographyService.GetEncryptionData();

            Core.Models.Sessions.OnlineSession.OpenOnlineSessionResponse openSessionRequest = await OnlineSessionUtils.OpenOnlineSessionAsync(KsefClient,
                encryptionData,
                authToken,
                systemCode);

            Assert.NotNull(openSessionRequest);
            Assert.NotNull(openSessionRequest.ReferenceNumber);

            Core.Models.Sessions.OnlineSession.SendInvoiceResponse sendInvoiceResponse = await OnlineSessionUtils.SendInvoiceAsync(KsefClient,
            openSessionRequest.ReferenceNumber,
            authToken,
            wrongNIP,
            invoiceTemplatePath,
            encryptionData,
            CryptographyService);
            Assert.NotNull(sendInvoiceResponse);
            Assert.NotNull(sendInvoiceResponse.ReferenceNumber);

            try
            {
                Core.Models.Sessions.SessionInvoice sendInvoiceStatus;

                do
                {
                    sendInvoiceStatus = await OnlineSessionUtils.GetSessionInvoiceStatusAsync(KsefClient,
                    openSessionRequest.ReferenceNumber,
                    sendInvoiceResponse.ReferenceNumber,
                    authToken);

                    await Task.Delay(SleepTime);
                }
                while (sendInvoiceStatus.Status.Code == 150);

                Assert.NotNull(sendInvoiceStatus);
                Assert.Equal(InvoiceInSessionStatusCodeResponse.InvalidPermissions, sendInvoiceStatus.Status.Code); // CODE 410, Insufficient permissions
            }
            finally
            {
                await OnlineSessionUtils.CloseOnlineSessionAsync(KsefClient, openSessionRequest.ReferenceNumber, authToken);
            }
        }

        [Theory]
        [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
        [Trait("Category", "SchemaValidationError")]
        [Trait("Field", "DataWytworzeniaFa")]
        [Trait("Condition", "< 2025-09-01")]
        [Trait("Scenario", "Invoice rejected when DataWytworzeniaFa < 2025-09-01")]
        public async Task GivenInvoiceWithDataWytworzeniaFaBeforeCutoffShouldFail(
            SystemCode systemCode, string templatePath)
        {
            DateTime cutoffUtc = new(2025, 8, 31);

            Core.Models.Sessions.EncryptionData encryptionData = CryptographyService.GetEncryptionData();

            Core.Models.Sessions.OnlineSession.OpenOnlineSessionResponse openSessionRequest = await OnlineSessionUtils.OpenOnlineSessionAsync(KsefClient, encryptionData, authToken, systemCode);
            Assert.False(string.IsNullOrWhiteSpace(openSessionRequest?.ReferenceNumber));

            string template = InvoiceHelpers.GetTemplateText(templatePath, nip);

            string invalidXml = InvoiceHelpers.SetElementValue(
                template, "DataWytworzeniaFa", InvoiceHelpers.SetDateForElement("DataWytworzeniaFa", cutoffUtc));

            try
            {
                Core.Models.Sessions.OnlineSession.SendInvoiceResponse sendInvoiceResponse = await OnlineSessionUtils.SendInvoiceFromXmlAsync(
                KsefClient, openSessionRequest.ReferenceNumber, authToken, invalidXml, encryptionData, CryptographyService);
                Assert.False(string.IsNullOrWhiteSpace(sendInvoiceResponse?.ReferenceNumber));

                Core.Models.Sessions.SessionInvoice sendInvoiceStatus;
                int attemtp = 0;
                do
                {
                    sendInvoiceStatus = await OnlineSessionUtils.GetSessionInvoiceStatusAsync(KsefClient,
                    openSessionRequest.ReferenceNumber,
                    sendInvoiceResponse.ReferenceNumber,
                    authToken);
                    attemtp++;
                    await Task.Delay(SleepTime);
                } while (sendInvoiceStatus.Status.Code != 450 || attemtp < 5);
                
                Assert.NotNull(sendInvoiceStatus);
                Assert.Equal(InvoiceInSessionStatusCodeResponse.InvoiceSemanticValidationError, sendInvoiceStatus.Status.Code); // CODE 450, Semantic validation error of the invoice document
            }
            finally
            {
                await OnlineSessionUtils.CloseOnlineSessionAsync(KsefClient, openSessionRequest.ReferenceNumber, authToken);
            }
        }

        [Theory]
        [InlineData(SystemCode.FA2, "invoice-template-fa-2.xml")]
        [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
        [Trait("ErrorType", "SchemaValidationError")]
        [Trait("Field", "P_1")]
        [Trait("Condition", "> today")]
        [Trait("Scenario", "Invoice rejected when P_1 is set to a future date")]
        public async Task GivenInvoiceWithP1InFutureShouldFail(
            SystemCode systemCode, string templatePath)
        {
            Core.Models.Sessions.EncryptionData encryptionData = CryptographyService.GetEncryptionData();

            Core.Models.Sessions.OnlineSession.OpenOnlineSessionResponse openSessionRequest = await OnlineSessionUtils.OpenOnlineSessionAsync(KsefClient, encryptionData, authToken, systemCode);
            Assert.False(string.IsNullOrWhiteSpace(openSessionRequest?.ReferenceNumber));

            string template = InvoiceHelpers.GetTemplateText(templatePath, nip);

            // Tomorrow UTC
            DateTime tomorrowUtc = DateTime.UtcNow.Date.AddDays(1);
            string invalidXml = InvoiceHelpers.SetElementValue(template, "P_1", InvoiceHelpers.SetDateForElement("P_1", tomorrowUtc));

            try
            {
                Core.Models.Sessions.OnlineSession.SendInvoiceResponse sendInvoiceResponse = await OnlineSessionUtils.SendInvoiceFromXmlAsync(
                KsefClient, openSessionRequest.ReferenceNumber, authToken, invalidXml, encryptionData, CryptographyService);
                Assert.False(string.IsNullOrWhiteSpace(sendInvoiceResponse?.ReferenceNumber));

                Core.Models.Sessions.SessionInvoice sendInvoiceStatus;

                do
                {
                    sendInvoiceStatus = await OnlineSessionUtils.GetSessionInvoiceStatusAsync(KsefClient,
                    openSessionRequest.ReferenceNumber,
                    sendInvoiceResponse.ReferenceNumber,
                    authToken);

                    await Task.Delay(SleepTime);
                }
                while (sendInvoiceStatus.Status.Code == 150);

                Assert.NotNull(sendInvoiceStatus);
                Assert.Equal(InvoiceInSessionStatusCodeResponse.InvoiceSemanticValidationError, sendInvoiceStatus.Status.Code); // CODE 450, Semantic validation error of the invoice document
            }
            finally
            {
                await OnlineSessionUtils.CloseOnlineSessionAsync(KsefClient, openSessionRequest.ReferenceNumber, authToken);
            }
        }

        [Theory]
        [InlineData(9)]
        [InlineData(251)]
        [InlineData(999)]
        [Trait("Category", "InvoiceMetadata")]
        [Trait("ErrorType", "ValidationError")]
        [Trait("Field", "pageSize")]
        [Trait("Condition", "outside 10-250")]
        [Trait("Scenario", "QueryInvoiceMetadataAsync throws when pageSize is out of allowed range")]
        public async Task GivenInvoiceMetadataQueryWithInvalidPageSizeShouldThrowError(int pageSize)
        {
            // authenticated in constructor
            Assert.NotNull(authToken);

            InvoiceQueryFilters invoiceMetadataQueryRequest = new()
            {
                SubjectType = InvoiceSubjectType.Subject1,
                DateRange = new DateRange
                {
                    From = DateTime.UtcNow.AddDays(-30),
                    To = DateTime.UtcNow,
                    DateType = DateType.Issue
                }
            };

            await Assert.ThrowsAnyAsync<KsefApiException>(() =>
                KsefClient.QueryInvoiceMetadataAsync(
                    requestPayload: invoiceMetadataQueryRequest,
                    accessToken: authToken,
                    cancellationToken: CancellationToken.None,
                    pageOffset: 0,
                    pageSize: pageSize));
        }

        [Fact]
        [Trait("Scenario", "Podmiot 3 może wyszukać fakturę, w której został wskazany jako ThirdSubject")]
        public async Task GivenNewInvoiceWithP3_SendedToKsef_ThenP3ShouldFoundInvoice()
        {
            // Arrange
            EncryptionData encryptionData = CryptographyService.GetEncryptionData();
            string invoiceCreatorNip = MiscellaneousUtils.GetRandomNip();
            string thirdSubjectIdentifier = MiscellaneousUtils.GetRandomNip();

            // Wystawienie faktury przez wykonawcę dla jednostki podrzędnej identyfikującej sie numerem NIP  ---
            string invoiceCreatorAuthToken = (await AuthenticationUtils.AuthenticateAsync(
                AuthorizationClient, invoiceCreatorNip)).AccessToken.Token;

            OpenOnlineSessionResponse openSessionResponse = await OnlineSessionUtils.OpenOnlineSessionAsync(
                KsefClient,
                encryptionData,
                invoiceCreatorAuthToken,
                SystemCode.FA3);

            Assert.NotNull(openSessionResponse?.ReferenceNumber);

            SendInvoiceResponse sendInvoiceResponse = await OnlineSessionUtils.SendInvoiceAsync(
                KsefClient,
                openSessionResponse.ReferenceNumber,
                invoiceCreatorAuthToken,
                invoiceCreatorNip,
                thirdSubjectIdentifier,
                "invoice-template-fa-3-with-custom-Subject3.xml",
                encryptionData,
                CryptographyService);

            Assert.NotNull(sendInvoiceResponse);

            SessionStatusResponse sessionStatus = await AsyncPollingUtils.PollAsync(
                async () => await OnlineSessionUtils.GetOnlineSessionStatusAsync(
                    KsefClient,
                    openSessionResponse.ReferenceNumber,
                    invoiceCreatorAuthToken).ConfigureAwait(false),
                result => result is not null && result.InvoiceCount == result.SuccessfulInvoiceCount,
                delay: TimeSpan.FromMilliseconds(2 * SleepTime),
                maxAttempts: 60);

            Assert.NotNull(sessionStatus);
            Assert.Equal(sessionStatus.InvoiceCount, sessionStatus.SuccessfulInvoiceCount);

            SessionInvoicesResponse invoices = await KsefClient.GetSessionInvoicesAsync(
                openSessionResponse.ReferenceNumber,
                invoiceCreatorAuthToken,
                pageSize: 10);

            Assert.NotEmpty(invoices.Invoices);

            // wyszukiwanie faktury jako podmiot3---
            AuthenticationOperationStatusResponse autResult = await AuthenticationUtils.AuthenticateAsOrganizationAsync(KsefClient, thirdSubjectIdentifier, AuthenticationTokenContextIdentifierType.Nip, EncryptionMethodEnum.ECDsa);
            string thirdSubjectToken = autResult.AccessToken.Token;

            await Task.Delay(10 * SleepTime);

            InvoiceQueryFilters invoiceQuery = new()
            {
                SubjectType = InvoiceSubjectType.Subject3,
                DateRange = new DateRange
                {
                    From = DateTime.UtcNow.AddMonths(-2),
                    To = DateTime.UtcNow.AddMonths(1),
                    DateType = DateType.PermanentStorage
                }
            };


            PagedInvoiceResponse invoiceQueryResponse = await KsefClient.QueryInvoiceMetadataAsync(
                requestPayload: invoiceQuery,
                accessToken: thirdSubjectToken,
                cancellationToken: CancellationToken.None,
                pageOffset: 0,
                pageSize: 30);

            // Assert
            Assert.Contains(invoiceQueryResponse.Invoices, x => x.ThirdSubjects.Any(y => y.Identifier.Value == thirdSubjectIdentifier));
        }
    }
}