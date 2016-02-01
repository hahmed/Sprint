using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Entity;

using Octokit;
using Octokit.Internal;
using Sprint.Models;


namespace Sprint.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGitHubClient _githubClient;
        private readonly string _apiKey;

        public ApplicationDbContext DbContext { get; } = new ApplicationDbContext();

        public HomeController()
        {
            var appSettings = ConfigurationManager.AppSettings;
            _apiKey = appSettings["Sprint.GitHubKey"];
            Ensure.NotNull(_apiKey);
            var creds = new InMemoryCredentialStore(new Credentials(_apiKey));
            _githubClient = new GitHubClient(new ProductHeaderValue("sprint-app"), creds);
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var repos = await _githubClient.Repository.GetAllForCurrent();
            return View(repos);
        }

        [HttpGet]
        public async Task<ActionResult> List(string ownerName, string repoName)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);

            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var allRepos = await _githubClient.Repository.GetAllForCurrent();

            if(!allRepos.Any(x => x.FullName == repository.FullName))
            {
                return View("NotCollaborator", repository);
            }

            var sprint = DbContext.Sprints.Include(x => x.Issues).SingleOrDefault(x => x.RepoId == repository.Id);
            if (sprint == null)
            {
                return View("NoBoard", repository);
            }

            var vm = new IndexViewModel
            {
                Owner = owner,
                Repository = repository,
                Sprint = sprint
            };

            var issues = await _githubClient.Issue.GetAllForRepository(owner.Login, repository.Name,
                new RepositoryIssueRequest { State = ItemState.Open });

            vm.OpenIssues = issues.Where(x => x.PullRequest == null).ToList();
            vm.OpenPRs = issues.Where(x => x.PullRequest != null).ToList();
            vm.RecentlyClosed = await _githubClient.Issue.GetAllForRepository(owner.Login, repository.Name,
                new RepositoryIssueRequest { State = ItemState.Closed, Since = DateTimeOffset.UtcNow.AddDays(-30) });
            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Backlog(string ownerName, string repoName)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);

            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var allRepos = await _githubClient.Repository.GetAllForCurrent();

            if (!allRepos.Any(x => x.FullName == repository.FullName))
            {
                return View("NotCollaborator", repository);
            }

            var sprint = DbContext.Sprints.Include(x => x.Issues).SingleOrDefault(x => x.RepoId == repository.Id);
            if (sprint == null)
            {
                return View("NoBoard", repository);
            }

            var vm = new BacklogViewModel
            {
                Owner = owner,
                Repository = repository,
                Sprint = sprint
            };

            var issues = await _githubClient.Issue.GetAllForRepository(owner.Login, repository.Name,
                new RepositoryIssueRequest { State = ItemState.Open });

            vm.OpenIssues = issues.Where(x => x.PullRequest == null).ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Team(string ownerName, string repoName)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);

            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var sprint = DbContext.Sprints.Include(x => x.Issues).SingleOrDefault(x => x.RepoId == repository.Id);
            if (sprint == null)
            {
                return HttpNotFound("Sprint board not found");
            }

            var vm = new TeamViewModel
            {
                Owner = owner,
                Repository = repository,
                Sprint = sprint
            };

            vm.Team = await _githubClient.Repository.GetAllContributors(owner.Login, repository.Name);

            return View(vm);
        }

        public async Task<ActionResult> SetupSprint(string ownerName, string repoName)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);
            
            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var allRepos = await _githubClient.Repository.GetAllForCurrent();
            var isCollaborator = allRepos.Any(x => x.FullName == repository.FullName);

            if (!isCollaborator)
            {
                return HttpNotFound("You are not a collaborator for this repo");
            }

            var sprint = DbContext.Sprints.SingleOrDefault(x => x.RepoId == repository.Id);

            if (sprint != null)
            {
                return RedirectToAction("List"); // sprint board already setup
            }

            sprint = new Models.Sprint
            {
                RepoId = repository.Id
            };
            DbContext.Sprints.Add(sprint);
            DbContext.SaveChanges();

            return RedirectToAction("List");
        }

        public async Task<ActionResult> AddIssue(string ownerName, string repoName, int issueId)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);

            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var sprint = DbContext.Sprints.SingleOrDefault(x => x.RepoId == repository.Id);

            if (sprint == null)
            {
                return HttpNotFound("sprint not found");
            }

            var issue = await _githubClient.Issue.Get(owner.Login, repoName, issueId);

            if (issue == null)
            {
                return HttpNotFound("issue not found");
            }

            // create the related sprint issue
            var sprintIssue = new SprintIssue
            {
                IssueId = issue.Number,
                When = DateTimeOffset.Now,
                SprintId = sprint.Id
            };
            DbContext.SprintsIssues.Add(sprintIssue);
            DbContext.SaveChanges();

            return RedirectToAction("List");
        }

        public async Task<ActionResult> RemoveIssue(string ownerName, string repoName, int issueId)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);

            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var sprint = DbContext.Sprints.Include(x => x.Issues).SingleOrDefault(x => x.RepoId == repository.Id);

            if (sprint == null)
            {
                return HttpNotFound("sprint not found");
            }

            var issue = await _githubClient.Issue.Get(owner.Login, repoName, issueId);

            if (issue == null)
            {
                return HttpNotFound("issue not found");
            }

            var sprintIssue = sprint.Issues.SingleOrDefault(x => x.IssueId == issue.Number);

            DbContext.SprintsIssues.Remove(sprintIssue);
            DbContext.SaveChanges();

            return RedirectToAction("List");
        }

        public async Task<ActionResult> CompletedIssue(string ownerName, string repoName, int issueId)
        {
            Ensure.NotNull(ownerName);
            Ensure.NotNull(repoName);

            var owner = await _githubClient.User.Get(ownerName);

            if (owner == null)
            {
                return HttpNotFound("owner not found");
            }

            var repository = await _githubClient.Repository.Get(owner.Login, repoName);

            if (repository == null)
            {
                return HttpNotFound("repo not found");
            }

            var sprint = DbContext.Sprints.Include(x => x.Issues).SingleOrDefault(x => x.RepoId == repository.Id);

            if (sprint == null)
            {
                return HttpNotFound("sprint not found");
            }

            var issue = await _githubClient.Issue.Update(owner.Login, repoName, issueId, new IssueUpdate { State = ItemState.Closed });
            if (issue == null)
            {
                return HttpNotFound("issue not found");
            }

            var sprintIssue = sprint.Issues.SingleOrDefault(x => x.IssueId == issue.Number);

            DbContext.SprintsIssues.Remove(sprintIssue);
            DbContext.SaveChanges();

            return RedirectToAction("List");
        }
    }
}