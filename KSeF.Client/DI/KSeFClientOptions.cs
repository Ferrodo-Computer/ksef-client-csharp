using KSeF.Client.Core.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace KSeF.Client.DI;

/// <summary>
/// Opcje konfiguracyjne dla klienta KSeF.
/// </summary>
public class KSeFClientOptions
{
    [Required(ErrorMessage = "BaseUrl is required.")]
    [Url(ErrorMessage = "BaseUrl must be a valid URL.")]
    public string BaseUrl { get; set; } = "";
    public Dictionary<string, string> CustomHeaders { get; set; }
    public IWebProxy WebProxy { get; set; } = null;

    public string ResourcesPath { get; set; } = null;
    public string[] SupportedUICultures { get; set; }
    public string[] SupportedCultures { get; set; }
    public string DefaultCulture { get; set; }


    public ApiConfiguration ApiConfiguration { get; set; } = new ApiConfiguration();
}