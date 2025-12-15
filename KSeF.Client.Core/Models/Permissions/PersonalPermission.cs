using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;
using System;

namespace KSeF.Client.Core.Models.Permissions
{
	public class PersonalPermission
	{
		public string Id { get; set; }
		public PersonalPermissionContextIdentifier ContextIdentifier { get; set; }
		public PersonalPermissionAuthorizedIdentifier AuthorizedIdentifier { get; set; }
		public PersonalPermissionTargetIdentifier TargetIdentifier { get; set; }
		public PersonalPermissionScopeType PermissionScope { get; set; }
		public PersonPermissionSubjectPersonDetails SubjectPersonDetails { get; set; }
		public EntityPermissionSubjectEntityDetails SubjectEntityDetails { get; set; }
		public string Description { get; set; }
		public PersonalPermissionState PermissionState { get; set; }
		public DateTimeOffset StartDate { get; set; }
		public bool CanDelegate { get; set; }

		public enum PersonalPermissionScopeType
		{
			CredentialsManage,
			CredentialsRead,
			InvoiceWrite,
			InvoiceRead,
			Introspection,
			SubunitManage,
			EnforcementOperations,
			VatUeManage
		}

		public enum PersonalPermissionState
		{
			Active,
			Inactive
		}
	}
}
