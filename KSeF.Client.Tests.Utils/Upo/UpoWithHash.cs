namespace KSeF.Client.Tests.Utils.Upo;

/// <summary>
/// Prosta struktura pomocnicza zawierająca treść UPO oraz wartość hash z nagłówka.
/// </summary>
public sealed class UpoWithHash
{
	public string Xml { get; init; }
	public string HashHeaderBase64 { get; init; }
}
