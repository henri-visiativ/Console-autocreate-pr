using AzureDevopsAPI;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

//
public class ClientAPIManagedIdentityExtended : ClientAPIManagedIdentity
{
    public ClientAPIManagedIdentityExtended(string organization, string clientId, string tenantId)
        : base(organization, clientId, tenantId)
    {
    }
    public ClientAPIManagedIdentityExtended(string organization, string clientId, string tenantId, string[] scopes)
    : base(organization, clientId, tenantId, scopes)
    {
    }

    public IEnumerable<TeamMember> GetTeamMembers(string projectId, string teamId)
    {
        // Retrieve the team members for the specified project and team
        var teamMembers = _currentConnection.GetClient<TeamHttpClient>().GetTeamMembersWithExtendedPropertiesAsync(projectId, teamId).GetAwaiter().GetResult();
        return teamMembers;
    }

    public IEnumerable<WebApiTeam> GetAllTeams()
    {
        // Retrieve all teams in the organization
        var teams = _currentConnection.GetClient<TeamHttpClient>().GetAllTeamsAsync().GetAwaiter().GetResult();
        return teams;
    }
}