
using KSeF.Client.Core.Models.Permissions.Identifiers;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Permissions.Entity
{
    public class GrantPermissionsEntityRequest
    {
        public GrantPermissionsEntitySubjectIdentifier SubjectIdentifier { get; set; }
        public ICollection<EntityPermission> Permissions { get; set; }
        public string Description { get; set; }
        public PermissionsEntitySubjectDetails SubjectDetails { get; set; }
    }

    public enum EntityStandardPermissionType
    {
        InvoiceRead,
        InvoiceWrite,
    }

    public class EntityPermission
    {
        public EntityStandardPermissionType Type { get; set; }
        public bool CanDelegate { get; set; }

        public EntityPermission(EntityStandardPermissionType type, bool canDelegate)
        {
            Type = type;
            CanDelegate = canDelegate;
        }

        public static EntityPermission New(EntityStandardPermissionType invoiceRead, bool canDelegate)
        {
            return new EntityPermission(invoiceRead, canDelegate);
        }
    }

    public class PermissionsEntitySubjectDetails
    {
        public string FullName { get; set; }
    }
}
