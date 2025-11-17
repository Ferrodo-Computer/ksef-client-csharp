namespace KSeF.Client.Core.Models.TestData
{
    /// <summary>Umożliwienie wysyłki faktur z załącznikiem (test).</summary>
    public sealed class AttachmentPermissionGrantRequest
    {
        /// <summary>Podmiot, któremu zezwalamy na wysyłkę z załącznikiem.</summary>
        public string Nip { get; set; }
    }
}
