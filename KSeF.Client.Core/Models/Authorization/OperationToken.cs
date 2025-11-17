using System;

namespace KSeF.Client.Core.Models.Authorization
{

    public class OperationToken
    {
        /// <summary>
        /// Token uwierzytelniający służący do dostępu do chronionych zasobów API.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Czas wygaśnięcia tokenu.
        /// </summary>
        public DateTime ValidUntil { get; set; }
    }

}
