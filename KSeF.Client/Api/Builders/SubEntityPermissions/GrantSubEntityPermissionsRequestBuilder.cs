using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.SubUnit;

namespace KSeF.Client.Api.Builders.SubUnitPermissions;

public static class GrantSubunitPermissionsRequestBuilder
{
    public static ISubjectStep Create() => GrantPermissionsRequestBuilderImpl.Create();

    public interface ISubjectStep
    {
        IContextStep WithSubject(SubunitSubjectIdentifier subject);
    }

    public interface IContextStep
    {
        IOptionalStep WithContext(SubunitContextIdentifier context);
    }

    public interface ISubunitNameStep
    {
        IOptionalStep WithSubunitName(string subunitName);
    }

    public interface IOptionalStep
    {
        IOptionalStep WithDescription(string description);
        IOptionalStep WithSubunitName(string subunitName);
        GrantPermissionsSubunitRequest Build();
    }

    private sealed class GrantPermissionsRequestBuilderImpl :
        ISubjectStep,
        IContextStep,
        IOptionalStep
    {
        private SubunitSubjectIdentifier _subject;
        private SubunitContextIdentifier _context;
        private string _description;
        private string _subunitName;

        private GrantPermissionsRequestBuilderImpl() { }

        internal static ISubjectStep Create() => new GrantPermissionsRequestBuilderImpl();

        public IContextStep WithSubject(SubunitSubjectIdentifier subject)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
            return this;
        }

        public IOptionalStep WithContext(SubunitContextIdentifier context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            return this;
        }

        public IOptionalStep WithDescription(string description)
        {
            _description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        public IOptionalStep WithSubunitName(string subunitName)
        {
            _subunitName = subunitName;
            return this;
        }

        public GrantPermissionsSubunitRequest Build()
        {
            if (_subject is null)
                throw new InvalidOperationException("Metoda WithSubject(...) musi zostać wywołana jako pierwsza.");
            if (_context is null)
                throw new InvalidOperationException("Metoda WithContext(...) musi zostać wywołana po ustawieniu podmiotu.");
            if (_context.Type == SubunitContextIdentifierType.InternalId && string.IsNullOrWhiteSpace(_subunitName))
                throw new InvalidOperationException("Dla typu ContextIdentifierType.InternalId, metoda WithSubunitName(...) musi zostać wywołana przed Build().");

            return new GrantPermissionsSubunitRequest
            {
                SubjectIdentifier = _subject,
                ContextIdentifier = _context,
                Description = _description,
                SubunitName = _subunitName
            };
        }
    }
}