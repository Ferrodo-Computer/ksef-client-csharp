using System.Collections.Generic;

namespace KSeF.Client.Core.Models.TestData
{
    /// <summary>Nadanie uprawnień w środowisku testowym.</summary>
    public sealed class TestDataPermissionsGrantRequest
    {
        /// <summary>Kontekst - jeśli dotyczy.</summary>
        public ContextIdentifier ContextIdentifier { get; set; }

        /// <summary>Identyfikator podmiotu upoważniającego.</summary>
        public AuthorizedIdentifier AuthorizedIdentifier { get; set; }               

        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public class ContextIdentifier
    {
        /// <summary>Typ kontekstu — jeśli dotyczy.</summary>
        public ContextIdentifierType Type { get; } = ContextIdentifierType.Nip;
        public string Value { get; set; }
    }

    public class AuthorizedIdentifier
    {
        /// <summary>Typ kontekstu — jeśli dotyczy.</summary>
        public AuthorizedIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public sealed class Permission
    {
        /// <summary>Typ uprawnienia (np. odczyt/wystawianie/zarządzanie).</summary>
        public PermissionType PermissionType { get; set; }
        public string Description { get; set; }
    }

    public enum PermissionType
    {
        InvoiceRead,
        InvoiceWrite,
        Introspection,
        CredentialsRead,
        CredentialsManage,
        EnforcementOperations,
        SubunitManage
    }

    public enum AuthorizedIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }

    public enum ContextIdentifierType
    {
        Nip
    }
}
