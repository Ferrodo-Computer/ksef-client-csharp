using KSeF.Client.Core.Models.Permissions.Identifiers;
using System;

namespace KSeF.Client.Core.Models.Permissions
{
    public class SubordinateEntityRole
    {
        public SubordinateEntityIdentifier SubordinateEntityIdentifier { get; set; }
        public SubordinateEntityRoleType Role { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
    }

    public enum SubordinateEntityRoleType
    {
        LocalGovernmentSubUnit,
        VatGroupSubUnit
    }
}
