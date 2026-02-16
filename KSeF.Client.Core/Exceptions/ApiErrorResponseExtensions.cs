using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KSeF.Client.Core.Exceptions
{
    public static class ApiErrorResponseExtensions
    {
        /// <summary>
        /// Składa komunikat zgodny z testami:
        /// "kod: opis. - detale; kod2: opis2. - ..." lub meta.
        /// </summary>
        public static string ToUserMessage(this ApiErrorResponse error, Func<string> fallback)
        {
            if (error != null &&
                error.Exception != null &&
                error.Exception.ExceptionDetailList != null &&
                error.Exception.ExceptionDetailList.Count > 0)
            {
                IEnumerable<string> items = error.Exception.ExceptionDetailList
                    .Select(detail =>
                    {
                        string head = detail.ExceptionCode.ToString(CultureInfo.InvariantCulture);
                        if (!string.IsNullOrWhiteSpace(detail.ExceptionDescription))
                        {
                            head += ": " + EnsureTrailingDot(detail.ExceptionDescription.Trim());
                        }
                        if (detail.Details != null && detail.Details.Count > 0)
                        {
                            string details = string.Join(
                                " - ",
                                detail.Details.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
                            if (!string.IsNullOrWhiteSpace(details))
                            {
                                head += " - " + details;
                            }
                        }
                        return head;
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s));

                return string.Join("; ", items);
            }

            if (error != null && error.Exception != null)
            {
                List<string> meta = new List<string>(5);
                if (!string.IsNullOrWhiteSpace(error.Exception.ServiceName))
                {
                    meta.Add("service=" + error.Exception.ServiceName);
                }

                if (!string.IsNullOrWhiteSpace(error.Exception.ServiceCode))
                {
                    meta.Add("serviceCode=" + error.Exception.ServiceCode);
                }

                if (!string.IsNullOrWhiteSpace(error.Exception.ReferenceNumber))
                {
                    meta.Add("ref=" + error.Exception.ReferenceNumber);
                }

                if (error.Exception.Timestamp != default)
                {
                    meta.Add("ts=" + error.Exception.Timestamp.ToString("O"));
                }

                if (!string.IsNullOrWhiteSpace(error.Exception.ServiceCtx))
                {
                    meta.Add("ctx=" + error.Exception.ServiceCtx);
                }

                if (meta.Count > 0)
                {
                    return string.Join(", ", meta);
                }
            }

            return fallback.Invoke();
        }

        private static string EnsureTrailingDot(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            return text.EndsWith(".", StringComparison.Ordinal) ? text : text + ".";
        }
    }
}
