using System;

namespace KSeF.Client.Core.Infrastructure.Rest
{
    public static class RestContentTypeExtensions
    {
        public const string DefaultContentType = "application/json";
        private const string JsonMime = "application/json";
        private const string XmlMime = "application/xml";

        /// <summary>
        /// Czy podany MIME (może zawierać np. "; charset=utf-8") to domyślny?
        /// </summary>
        public static bool IsDefaultType(string contentType)
        {
            return string.Equals(GetBaseMime(contentType), DefaultContentType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Czy typ treści z enum odpowiada domyślnemu MIME?
        /// </summary>
        public static bool IsDefaultType(this RestContentType contentType)
        {
            return contentType == RestContentType.Json;
        }

        /// <summary>
        /// Mapowanie typu treści na MIME.
        /// </summary>
        public static string ToMime(this RestContentType contentType)
        {
            switch (contentType)
            {
                case RestContentType.Json:
                    return JsonMime;
                case RestContentType.Xml:
                    return XmlMime;
                default:
                    return DefaultContentType;
            }
        }

        /// <summary>
        /// Parsuje MIME (ignoruje parametry) do <see cref="RestContentType"/>.
        /// Rzuca wyjątek dla nieobsługiwanych typów – użyj <see cref="TryToRestContentType"/> jeśli nie chcesz wyjątków.
        /// </summary>
        public static RestContentType ToRestContentType(string mime)
        {
            RestContentType result;
            if (TryToRestContentType(mime, out result))
            {
                return result;
            }

            throw new ArgumentException("Nieobsługiwany typ MIME: '" + mime + "'", nameof(mime));
        }

        /// <summary>
        /// Bezpieczne parsowanie MIME do <see cref="RestContentType"/> bez wyjątków.
        /// </summary>
        public static bool TryToRestContentType(string mime, out RestContentType contentType)
        {
            string baseMime = GetBaseMime(mime);

            if (string.Equals(baseMime, JsonMime, StringComparison.OrdinalIgnoreCase))
            {
                contentType = RestContentType.Json;
                return true;
            }

            if (string.Equals(baseMime, XmlMime, StringComparison.OrdinalIgnoreCase))
            {
                contentType = RestContentType.Xml;
                return true;
            }

            contentType = default(RestContentType);
            return false;
        }

        /// <summary>
        /// Zwraca MIME bez parametrów (np. "application/json" z "application/json; charset=utf-8").
        /// Gdy brak lub puste – zwraca <see cref="DefaultContentType"/>.
        /// </summary>
        private static string GetBaseMime(string mime)
        {
            if (string.IsNullOrWhiteSpace(mime))
            {
                return DefaultContentType;
            }

            string trimmed = mime.Trim();
            int semicolonIndex = trimmed.IndexOf(';');
            string withoutParams = (semicolonIndex >= 0) ? trimmed.Substring(0, semicolonIndex) : trimmed;
            return withoutParams.Trim();
        }
    }
}
