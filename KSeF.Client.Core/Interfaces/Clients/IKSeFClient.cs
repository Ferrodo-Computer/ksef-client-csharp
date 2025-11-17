namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient agregujący wszystkie operacje dostępne w KSeF.
    /// </summary>
    public interface IKSeFClient : IActiveSessionsClient, IAuthorizationClient, IOnlineSessionClient, IBatchSessionClient, ISessionStatusClient, IInvoiceDownloadClient, IGrantPermissionClient, IRevokePermissionClient, ISearchPermissionClient, IPermissionOperationClient, ICertificateClient, IKsefTokenClient, IPeppolClient
    {
    }
}