using AzureDevopsAPI;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Spectre.Console;

public static class Utils
{
    public static async Task<bool> CheckOrganizationExists(string instance)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{instance}");
        return response.IsSuccessStatusCode;
    }

    public static ClientAPIManagedIdentityExtended? GetClientAPI(string instance)
    {
        // Detect if running as Managed Identity
        try
        {
            return new ClientAPIManagedIdentityExtended(instance, null, null);
        }
        catch (VssException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error using Managed Identity: {ex.Message}[/]");
            return null;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Unexpected error: {ex.Message}[/]");
            return null;
        }
    }

    public static string GetOrganisation(List<string> orgs)
    {
        // Ask user for preconfigured Azure orgs
        var org = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select your [green]Azure Organization[/]:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more repositories)[/]")
                .AddChoices(orgs));

        // Ask user for custom Azure org
        if (org == "custom")
        {
            // If custom, ask user for Azure DevOps Organization
            org = AnsiConsole.Ask<string>("Enter your [green]Azure DevOps Organization[/]:");
        }

        return org;
    }

    public static string GetProject(List<string> presetProjects)
    {
        var projectName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select your [green]Azure Project[/]:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more projects)[/]")
                .AddChoices(presetProjects));

        if (projectName == "custom")
        {
            // If custom, ask user for Azure Project
            projectName = AnsiConsole.Ask<string>("Enter your [green]Azure Project[/]:");
        }

        return projectName;
    }

    //public static string GetProjectFromApi(ClientAPIManagedIdentityExtended client, string org)
    //{
    //    var projects = client.GetProject(org);

    //    var projectName = AnsiConsole.Prompt(
    //        new SelectionPrompt<string>()
    //            .Title("Select your [green]Azure Project[/]:")
    //            .PageSize(10)
    //            .MoreChoicesText("[grey](Move up and down to reveal more projects)[/]")
    //            .AddChoices(projects));

    //    if (projectName == "custom")
    //    {
    //        // If custom, ask user for Azure Project
    //        projectName = AnsiConsole.Ask<string>("Enter your [green]Azure Project[/]:");
    //    }

    //    return projectName;
    //}

    public static List<GitRepository> GetRepositories(AbstractClientAPI clientAPI, string projectName)
    {
        var repos = clientAPI.ListProjectRepositories(projectName);
        if (repos == null || !repos.Any())
        {
            AnsiConsole.MarkupLine("[red]No repositories found in the selected project.[/]");
            return new List<GitRepository>();
        }
        var reposToString = repos.Select(repo => new GitRepositoryToString(repo)).ToList();
        // Ask user for Azure Repos
        var reposName = AnsiConsole.Prompt(
            new MultiSelectionPrompt<GitRepositoryToString>()
                .Title("Select your [green]Azure Repository[/]:")
                .PageSize(30)
                .MoreChoicesText("[grey](Move up and down to reveal more repositories)[/]")
            .InstructionsText(
                "[grey](Press [blue]<space>[/] to toggle a repository, " +
                "[green]<enter>[/] to accept)[/]")
            .AddChoices(reposToString));

        return [.. reposName.Select(repo => repo.GetRepository())];
    }

    public static string SetRepositoryChoices(List<string> reposName)
    {
        // // set Repository variable
        var repoNameToString = string.Empty;
        AnsiConsole.MarkupLine("[green] Repositories :[/]");
        foreach (var repo in reposName)
        {
            repoNameToString += repo + ", ";
        }
        return repoNameToString;
    }

    public static string GetBranch(List<string> presetBranches, string title)
    {
        var selectedBranch = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .AddChoices(presetBranches));

        if (selectedBranch == "custom")
        {
            // If custom, ask user for Azure Source Branch
            selectedBranch = AnsiConsole.Ask<string>("Enter your [green]Azure Source Branch[/]:");
        }

        return selectedBranch;
    }

    // retrieve branches from repository
     public static List<string> GetBranchesFromRepository(ClientAPIManagedIdentityExtended clientAPI, string repoId)
    {
        bool formatRepositoryGuid = Guid.TryParse(repoId, out Guid repositoryGuidFromId);
        List<GitRef> repositoryBranches = clientAPI.ListRefs(repositoryGuidFromId);
        AnsiConsole.MarkupLine("[green] Branches :[/]");
        var repositoryBranchesName = repositoryBranches.Select(repoBranch => repoBranch.Name);
        var repoBranchesToString = string.Join(", ", repositoryBranchesName);

        var selectedBranch = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Select your repository Branches :[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .AddChoices(repositoryBranchesName));

        return repositoryBranchesName.ToList();
    }


    // retrieve branches from repository
    public static List<string> GetBranchesFromAllRepositories(ClientAPIManagedIdentityExtended clientAPI, List<Guid> repoIds)
    {
        if (repoIds == null || !repoIds.Any())
        {
            AnsiConsole.MarkupLine("[red]No repositories found.[/]");
            return new List<string>();
        }
        // If multiple repositories, ask user to select one
        List<GitRef> repositoriesAllBranches = new List<GitRef>();
        foreach (var repoId in repoIds)
        {
            List<GitRef> repositoryBranches = clientAPI.ListRefs(repoId);
            if(repositoryBranches != null && repositoryBranches.Any())
            {
                repositoriesAllBranches.AddRange(repositoryBranches);
                // Add project separator
                repositoriesAllBranches.Add(new GitRef { Name = "--------" }); 
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]No branches found for repository with ID: {repoId}[/]");
            }
        }
        AnsiConsole.MarkupLine("[green] Branches :[/]");
        var repositoryBranchesName = repositoriesAllBranches.Select(repoBranch => repoBranch.Name);

        var selectedBranch = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green] Select your repository Branches :[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more branches)[/]")
                .AddChoices(repositoryBranchesName));

        return repositoryBranchesName.ToList();
    }

     public static string GetTeam(ClientAPIManagedIdentityExtended clientAPI, string projectId)
    {
        // Get all teams in the project
        var teams = clientAPI.GetAllTeams();
        // Ask user for team
        var teamName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select your [green]Azure Team[/]:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more teams)[/]")
                .AddChoices(teams.Select(t => t.Name).ToList()));

        return teamName;
    }

    public static List<IdentityRef> GetReviewers(ClientAPIManagedIdentityExtended clientAPI, string projectId)
    {
        var teamId = "7a8deba3-4a9b-4069-a2b7-92eeb7a3dc03"; // premier choix de reviewer,
                                                             // Get team members
        var teamMembers = clientAPI.GetTeamMembers(projectId, teamId).ToList();
        teamMembers.Insert(0, new TeamMember
        {
            Identity = new IdentityRef { DisplayName = "Dev", Id = teamId }
        });
        // Ask user for reviewer
        var teamMembersToString = teamMembers.Select(m => new IdentityRefToString(m.Identity)).ToList();
        var reviewerName = AnsiConsole.Prompt(
            new MultiSelectionPrompt<IdentityRefToString>()
                .Title("Select your [green]reviewer[/]:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more projects)[/]")
                .InstructionsText(
                 "[grey](Press [blue]<space>[/] to toggle a repository, " +
                 "[green]<enter>[/] to accept)[/]")
                .AddChoices(teamMembersToString));

        return [.. reviewerName.Select(r => r.GetIdentityRef())];
    }

    // Create pull requests for each repository
    public static List<string> CreatePullRequests(AbstractClientAPI clientAPI,
                                          IEnumerable<GitRepository> repos,
                                          string sourceBranch,
                                          string targetBranch,
                                          string title,
                                          string description,
                                          IEnumerable<IdentityRef> reviewer)
    {
        var reviewers = reviewer.Select(r => new IdentityRefWithVote { DisplayName = r.DisplayName, Id = r.Id }).ToArray();
        List<string> prUrls = new List<string>();
        foreach (var repo in repos)
        {
            // Create pull request and return the URL
            string prUrl = CreatePullRequest(clientAPI, sourceBranch, targetBranch, title, description, repo, reviewers);
            prUrls.Add(prUrl);
        }
        return prUrls;
    }

    // Create a pull request and return the URL
    public static string CreatePullRequest(AbstractClientAPI clientAPI,
                                         string sourceBranch,
                                         string targetBranch,
                                         string title,
                                         string description,
                                         GitRepository repository,
                                         IdentityRefWithVote[] reviewers)
    {
        GitPullRequest pr = clientAPI.CreatePullRequest(repository.Id, sourceBranch, targetBranch, title, description, GitPullRequestMergeStrategy.NoFastForward, false, reviewers);
        return pr.Url;
    }
}