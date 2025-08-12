using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace make_pr_az_repos
{
    enum Choices
    {
        SET_ORGANIZATION, SET_PROJECT, SET_REPOSITORIES, SET_REVIEWERS, SET_SOURCE_BRANCH, SET_TARGET_BRANCH
    };

    public static class PinMessages
    {
        public static string Organization = string.Empty;
        public static string Repository = string.Empty;
        public static string Project = string.Empty;
        public static string SourceBranch = string.Empty;
        public static string TargetBranch = string.Empty;

        public static void DisplayChoices()
        {
            var displayChoices = string.Empty;
            // display organization if set
            if (!string.IsNullOrWhiteSpace(Organization))
            {
                displayChoices += $"[green]Organization:[/] {Organization}";
            }
            // display project if set
            if (!string.IsNullOrWhiteSpace(Project))
            {
                displayChoices += $" / [green]Project:[/] {Project}";
            }
            // display repositories if set
            if (!string.IsNullOrWhiteSpace(Repository))
            {
                displayChoices += $" / [green]Repository:[/] {Repository}";
            }
            // display source branch if set
            if (!string.IsNullOrWhiteSpace(SourceBranch))
            {
                displayChoices += $" / [green]Source Branch:[/] {SourceBranch}";
            }
            // display target branch if set
            if (!string.IsNullOrWhiteSpace(TargetBranch))
            {
                displayChoices += $" / [green]Target Branch:[/] {TargetBranch}";
            }

            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[yellow]Review Choices : [/] {displayChoices}\n");
        }
    }
}
