
using System;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Sessions
{
    public class SessionsFilter
    {
        /// <summary>
        /// Numer referencyjny sesji.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Data utworzenia sesji (od).
        /// </summary>
        public DateTimeOffset? DateCreatedFrom { get; set; }

        /// <summary>
        /// Data utworzenia sesji (do).
        /// </summary>
        public DateTimeOffset? DateCreatedTo { get; set; }

        /// <summary>
        /// Data zamknięcia sesji (od).
        /// </summary>
        public DateTimeOffset? DateClosedFrom { get; set; }

        /// <summary>
        /// Data zamknięcia sesji (do).
        /// </summary>
        public DateTimeOffset? DateClosedTo { get; set; }

        /// <summary>
        /// Data ostatniej aktywności (od).
        /// </summary>
        public DateTimeOffset? DateModifiedFrom { get; set; }

        /// <summary>
        /// Data ostatniej aktywności (do).
        /// </summary>
        public DateTimeOffset? DateModifiedTo { get; set; }

        /// <summary>
        /// Statusy sesji.
        /// </summary>
        public ICollection<SessionStatus> Statuses { get; set; }
    }
    public enum SessionStatus
    {
        /// <summary>
        /// Sesja przetworzona poprawnie.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Sesja aktywna.
        /// </summary>
        InProgress,

        /// <summary>
        /// Sesja nie przetworzona z powodu błędów.
        /// </summary>
        Failed,

        /// <summary>
        /// Sesja anulowana.
        /// </summary>
        Cancelled
    }

    public enum SessionType
    {
        /// <summary>
        /// Sesja interaktywna.
        /// </summary>
        Online,
        /// <summary>
        /// Sesja wsadowa.
        /// </summary>
        Batch
    }
}