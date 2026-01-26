using System;

namespace KSeF.Client.Core.Models.Lighthouse
{
    /// <summary>
    /// Komunikat z Latarni (Lighthouse)
    /// </summary>
    public sealed class Message
    {
        /// <summary>
        /// Identyfikator komunikatu.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identyfikator zdarzenia (grupy komunikatów), pozwalający powiązać komunikaty (np. start i koniec tej samej awarii).
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Kategoria komunikatu.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Typ komunikatu.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Tytuł komunikatu.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Treść komunikatu.
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Początek okresu obowiązywania komunikatu.
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// Koniec okresu obowiązywania komunikatu.
        /// </summary>
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// Wersja komunikatu.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Data i godzina udostępnienia komunikatu w serwisach Latarni.
        /// </summary>
        public DateTimeOffset Published { get; set; }
    }
}
