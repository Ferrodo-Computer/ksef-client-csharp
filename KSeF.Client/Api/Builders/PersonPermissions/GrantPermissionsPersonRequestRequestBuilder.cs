using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;

namespace KSeF.Client.Api.Builders.PersonPermissions;

public static class GrantPersonPermissionsRequestBuilder
{
    public static ISubjectStep Create() => GrantPermissionsRequestBuilderImpl.Create();

    public interface ISubjectStep
    {
        IPermissionsStep WithSubject(GrantPermissionsPersonSubjectIdentifier subject);
    }

    public interface IPermissionsStep
    {
        IDescriptionStep WithPermissions(params PersonPermissionType[] permissions);
    }

    public interface IDescriptionStep
    {
        IBuildStep WithDescription(string description);
    }

    public interface IBuildStep
    {
        GrantPermissionsPersonRequest Build();
    }


    private sealed class GrantPermissionsRequestBuilderImpl :
        ISubjectStep,
        IPermissionsStep,
        IDescriptionStep,
        IBuildStep
    {
        private GrantPermissionsPersonSubjectIdentifier _subject;
        private ICollection<PersonPermissionType> _permissions;
        private string _description;

        private GrantPermissionsRequestBuilderImpl() { }

        internal static ISubjectStep Create() => new GrantPermissionsRequestBuilderImpl();

        public IPermissionsStep WithSubject(GrantPermissionsPersonSubjectIdentifier subject)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
            return this;
        }

        public IDescriptionStep WithPermissions(params PersonPermissionType[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                throw new ArgumentException("Należy podać co najmniej jedno uprawnienie.", nameof(permissions));

            _permissions = permissions;
            return this;
        }

        public IBuildStep WithDescription(string description)
        {
            _description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        public GrantPermissionsPersonRequest Build()
        {
            if (_subject is null)
                throw new InvalidOperationException("Metoda WithSubject(...) musi zostać wywołana jako pierwsza.");
            if (_permissions is null)
                throw new InvalidOperationException("Metoda WithPermissions(...) musi zostać wywołana po ustawieniu podmiotu.");
            if (_description is null)
                throw new InvalidOperationException("Metoda WithDescription(...) musi zostać wywołana po ustawieniu uprawnień.");

            return new GrantPermissionsPersonRequest
            {
                SubjectIdentifier = _subject,
                Permissions = _permissions,
                Description = _description,
            };
        }
    }
}