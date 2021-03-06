﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using System.Data.Entity;

using Octokit;
using Octokit.Internal;
using Sprint.Models;
using System.Collections.Generic;

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
        public async Task<ActionResult> PRReviews(string ownerName, string repoName)
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

            var request = new PullRequestRequest();
            var results = new Dictionary<string, List<PullRequest>>();
            var pullRequests = await _githubClient.PullRequest.GetAllForRepository(owner.Login, repository.Name, request);

            // for each pull request, get the files and associate with the pr number
            foreach (var pr in pullRequests)
            {
                var files = await _githubClient.PullRequest.Files(owner.Login, repository.Name, pr.Number);
                foreach (var file in files)
                {
                    if (results.ContainsKey(file.FileName))
                    {
                        results[file.FileName].Add(pr);
                    }
                    else
                    {
                        var list = new List<PullRequest> { pr };
                        results.Add(file.FileName, list);
                    }
                }
            }

            var sortbyPopularity = results.OrderByDescending(kvp => kvp.Value.Count);

            var vm = new PullRequestReviewViewModel
            {
                Owner = owner,
                Repository = repository,
                PullRequests = sortbyPopularity
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Stats()
        {
            var vm = new StatsViewModel();

            var issueSearch = new SearchIssuesRequest
            {
                Comments = Range.GreaterThan(25),
                Created = DateRange.GreaterThan(new DateTime(DateTime.Now.Year, 01, 01)),
                SortField = IssueSearchSort.Comments,
                Language = Language.Ruby,
                Order = SortDirection.Descending
            };
            var mostCommented = await _githubClient.Search.SearchIssues(issueSearch);
            vm.MostCommentedIssue = mostCommented.Items.OrderByDescending(x=> x.Comments).Take(10).ToList();

            var repoSearch = new SearchRepositoriesRequest
            {
                Created = DateRange.GreaterThan(new DateTime(DateTime.Now.Year, 01, 01)),
                Language = Language.CSharp,
                SortField = RepoSearchSort.Stars,
                Order = SortDirection.Descending
            };
            var mostStarred = await _githubClient.Search.SearchRepo(repoSearch);
            vm.MostStarred = mostStarred.Items.Take(10).ToList();

            var repo = new RepositoryCollection();
            repo.Add("rails", "rails");
            issueSearch = new SearchIssuesRequest
            {
                 Repos = repo,
                 Created = DateRange.GreaterThan(new DateTime(DateTime.Now.Year, 01, 01)),
            };
            var railsIssues = await _githubClient.Search.SearchIssues(issueSearch);
            var dic = new Dictionary<DayOfWeek, List<Issue>>();
            foreach (var issue in railsIssues.Items)
            {
                if(dic.ContainsKey(issue.CreatedAt.DayOfWeek))
                {
                    dic[issue.CreatedAt.DayOfWeek].Add(issue);
                }
                else
                {
                    dic.Add(issue.CreatedAt.DayOfWeek, new List<Issue> { issue });
                }
            }

            vm.RailsIssues = dic;

            return View(vm);
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

            if (!allRepos.Any(x => x.FullName == repository.FullName))
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