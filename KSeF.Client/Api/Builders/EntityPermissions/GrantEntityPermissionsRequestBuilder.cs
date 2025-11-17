using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.Client.Api.Builders.EntityPermissions;

public static class GrantEntityPermissionsRequestBuilder
{
    public static ISubjectStep Create() => GrantPermissionsRequestBuilderImpl.Create();

    public interface ISubjectStep
    {
        IPermissionsStep WithSubject(GrantPermissionsEntitySubjectIdentifier subject);
    }

    public interface IPermissionsStep
    {
        IOptionalStep WithPermissions(params EntityPermission[] permissions);
    }

    public interface IOptionalStep
    {
        IOptionalStep WithDescription(string description);
        GrantPermissionsEntityRequest Build();
    }

    private sealed class GrantPermissionsRequestBuilderImpl :
        ISubjectStep,
        IPermissionsStep,
        IOptionalStep
    {
        private GrantPermissionsEntitySubjectIdentifier _subject;
        private ICollection<EntityPermission> _permissions;
        private string _description;

        private GrantPermissionsRequestBuilderImpl() { }

        internal static ISubjectStep Create() => new GrantPermissionsRequestBuilderImpl();

        public IPermissionsStep WithSubject(GrantPermissionsEntitySubjectIdentifier subject)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
            return this;
        }

        public IOptionalStep WithPermissions(params EntityPermission[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                throw new ArgumentException("Wymagane jest co najmniej jedno uprawnienie.", nameof(permissions));

            _permissions = permissions;
            return this;
        }

        public IOptionalStep WithDescription(string description)
        {
            _description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        public GrantPermissionsEntityRequest Build()
        {
            if (_subject is null)
                throw new InvalidOperationException("Najpierw należy wywołać WithSubject(...).");
            if (_permissions is null)
                throw new InvalidOperationException("Po wskazaniu podmiotu należy wywołać WithPermissions(...).");

            return new GrantPermissionsEntityRequest
            {
                SubjectIdentifier = _subject,
                Permissions = _permissions,
                Description = _description,
            };
        }
    }
}
