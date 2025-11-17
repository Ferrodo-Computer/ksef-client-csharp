using KSeF.Client.Core.Models.Permissions.Identifiers;
using System;

namespace KSeF.Client.Core.Models.Permissions
{
    public class EntityRole
    {
        public PersonPermissionContextIdentifier ParentEntityIdentifier { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
    }
}
