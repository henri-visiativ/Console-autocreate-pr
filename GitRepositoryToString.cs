using Microsoft.TeamFoundation.SourceControl.WebApi;

public class GitRepositoryToString(GitRepository repository)
{
    public readonly GitRepository _repository = repository;
    public GitRepository GetRepository() => _repository;

    public override string ToString()
    {
        if (_repository == null)
        {
            return "No repository selected";
        }

        // Format the repository information as a string
        return _repository.Name;
    }
}