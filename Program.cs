using AzureDevopsAPI;
using make_pr_az_repos;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Organization.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Spectre.Console;

AnsiConsole.Clear();

var presetOrgs = new List<string>
{
  "moovapps",
  "custom"
};

// transform to Dictionary
var presetProjectsDico = new Dictionary<string, string>
{
  { "134eced6-808c-4adb-964d-f06d99cf86f1", "Visiativ CPQ" },
  { "12345-67890", "custom" }
};
var presetProjects = new List<string>
{
  "Visiativ CPQ",
  "custom"
};

//  Ask user for preconfigured Azure repository's branches
var presetBranches = new List<string>
{
  "V6.1.3.Recette",
  "V6.1.3.PreProd",
  "V6.1.3",
  "custom"
};
PinMessages.DisplayChoices();
string org = Utils.GetOrganisation(presetOrgs);
// set Organization variable
PinMessages.Organization = org;

var instance = $"https://dev.azure.com/{org}";

ClientAPIManagedIdentityExtended? clientAPI = Utils.GetClientAPI(instance);

if (clientAPI is null)
{
  AnsiConsole.MarkupLine("[red]Failed to initialize Azure DevOps client API.[/]");
  return;
}

// Check if the organization is valid
if (string.IsNullOrWhiteSpace(org))
{
  AnsiConsole.MarkupLine("[red]Organization cannot be empty.[/]");
  return;
}

// Check if org exists using httpClient
if (!await Utils.CheckOrganizationExists(instance))
{
  AnsiConsole.MarkupLine($"[red]Organization '{org}' does not exist or is not accessible.[/]");
  return;
}

PinMessages.DisplayChoices();
// Ask user for Azure Project
var projectName = Utils.GetProject(presetProjects);
PinMessages.Project = projectName;


/*************************************************/
// var projectTeams = Utils.GetTeam(clientAPI, projectName);
/*************************************************/


PinMessages.DisplayChoices();
// Get all repositories in the project
var repos = Utils.GetRepositories(clientAPI, projectName);

// // set Repository variable
AnsiConsole.MarkupLine("[green] Repositories :[/]");
var repoNameToString = string.Join(", ", repos.Select(repo => repo.Name));
PinMessages.Repository = repoNameToString;

// define a dictionary to map repository IDs to names
var presetRepositories = new Dictionary<string, string>();
foreach (var repo in repos)
{
    presetRepositories[repo.Id.ToString()] = repo.Name;
}

var reposIdList = repos.Select(repo => repo.Id).ToList();
var allReposBranches = Utils.GetBranchesFromAllRepositories(clientAPI, reposIdList);

// Ask user for Azure Source Branch
PinMessages.DisplayChoices();
string sourceBranch = Utils.GetBranch(presetBranches, "Select your [green]Azure Source Branch[/]:");
PinMessages.SourceBranch = sourceBranch;

if (string.IsNullOrWhiteSpace(sourceBranch))
{
  AnsiConsole.MarkupLine("[red]Source branch cannot be empty.[/]");
  return;
}

// Ask user for Azure Target Branch
PinMessages.DisplayChoices();
var targetBranch = Utils.GetBranch(presetBranches, "Select your [green]Azure Target Branch[/]:");
PinMessages.TargetBranch = targetBranch;

if (string.IsNullOrWhiteSpace(targetBranch))
{
  AnsiConsole.MarkupLine("[red]Target branch cannot be empty.[/]");
  return;
}

PinMessages.DisplayChoices();
// Verify branches exist in each repository
AnsiConsole.MarkupLine("[yellow]Verifying branches in selected repositories...[/]");

var title = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter the [green]title[/] for the pull request:")
        .DefaultValue($"PR from {sourceBranch} to {targetBranch}"));

var description = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter the [green]description[/] for the pull request:")
        .DefaultValue($"This PR is created from {sourceBranch} to {targetBranch}."));

var projectKey = presetProjectsDico.FirstOrDefault(x => x.Value == projectName).Key;

PinMessages.DisplayChoices();
var reviewerName = Utils.GetReviewers(clientAPI, projectKey);
var reviewerNameToString = string.Join(", ", reviewerName.Select(reviewer => reviewer.DisplayName));

AnsiConsole.MarkupLine($"Reviewers: [green]{reviewerNameToString}[/]\n");

// Ask user for confirmation
var confirm = AnsiConsole.Confirm("Pull request will be created for these repositories. Do you want to proceed?");
if (!confirm)
{
  AnsiConsole.MarkupLine("[red]Operation cancelled by user.[/]");
  return;
}

AnsiConsole.MarkupLine("[yellow]Creating pull requests...[/]");

Dictionary<string, GitRepository> gitrepos = new Dictionary<string, GitRepository>();

var prUrls = Utils.CreatePullRequests(clientAPI, repos, sourceBranch, targetBranch, title, description, reviewerName);
var prUrlsToString = string.Join("\n", prUrls);
AnsiConsole.MarkupLine($"Created PR urls : \n[green]{prUrlsToString}[/]\n");