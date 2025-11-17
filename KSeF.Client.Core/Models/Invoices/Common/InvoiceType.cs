namespace KSeF.Client.Core.Models.Invoices.Common
{
    /// <summary>
    /// Typ faktury (metadane).
    /// </summary>
    public enum InvoiceType
    {
        Vat,        // (FA) Podstawowa
        Zal,        // (FA) Zaliczkowa
        Kor,        // (FA) Korygująca
        Roz,        // (FA) Rozliczeniowa
        Upr,        // (FA) Uproszczona
        KorZal,     // (FA) Korygująca fakturę zaliczkową
        KorRoz,     // (FA) Korygująca fakturę rozliczeniową
        VatPef,     // (PEF) Podstawowa
        VatPefSp,   // (PEF) Specjalizowana
        KorPef,     // (PEF) Korygująca
        VatRr,      // (RR) Podstawowa
        KorVatRr    // (RR) Korygująca
    }
}
