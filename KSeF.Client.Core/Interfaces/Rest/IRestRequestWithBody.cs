namespace KSeF.Client.Core.Interfaces.Rest
{
    public interface IRestRequestWithBody<T> : IRestRequest
    {
        T Body { get; }
    }
}
