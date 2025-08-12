using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

public class IdentityRefToString(IdentityRef identityRef)
{
    public readonly IdentityRef _identityRef = identityRef;
    public IdentityRef GetIdentityRef() => _identityRef;

    public override string ToString()
    {
        if (_identityRef == null)
        {
            return "No identity selected";
        }

        // Format the repository information as a string
        return _identityRef.DisplayName;
    }
}