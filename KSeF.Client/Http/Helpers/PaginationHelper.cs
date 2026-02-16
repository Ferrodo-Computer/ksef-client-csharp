using System.Text;

namespace KSeF.Client.Http.Helpers;

/// <summary>
/// Klasa pomocnicza do obsługi paginacji w zapytaniach HTTP.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Dodaje paginacje do żądania HTTP.
    /// </summary>
    /// <param name="pageOffset"></param>
    /// <param name="pageSize"></param>
    /// <param name="urlBuilder"></param>
    public static void AppendPagination(int? pageOffset, int? pageSize, StringBuilder urlBuilder)
    {
        if (pageSize.HasValue && pageSize > 0)
        {
            urlBuilder.Append(urlBuilder.ToString().Contains('?') ? '&' : '?');
            urlBuilder.Append("pageSize=").Append(Uri.EscapeDataString(pageSize.ToString()));
        }
        if (pageOffset.HasValue && pageOffset > 0)
        {
            urlBuilder.Append(urlBuilder.ToString().Contains('?') ? '&' : '?');
            urlBuilder.Append("pageOffset=").Append(Uri.EscapeDataString(pageOffset.ToString()));
        }
    }
}
