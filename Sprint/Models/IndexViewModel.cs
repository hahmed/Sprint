using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Sprint.Models
{
    public class IndexViewModel
    {
        /// <summary>
        /// owner of repo
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// Current repo
        /// </summary>
        public Repository Repository { get; set; }

        /// <summary>
        /// List of open issues
        /// </summary>
        public IReadOnlyList<Issue> OpenIssues { get; set; }

        /// <summary>
        /// List of Open PR's
        /// </summary>
        public IReadOnlyList<Issue> OpenPRs { get; set; }

        /// <summary>
        /// List of recently closed issues
        /// </summary>
        public IReadOnlyList<Issue> RecentlyClosed { get; set; }

        /// <summary>
        /// The current sprint
        /// </summary>
        public Sprint Sprint { get; set; }

        public IList<Issue> CurrentSprint
        {
            get
            {
                return OpenIssues.Where(x => Sprint.Issues.Any(y => y.IssueId == x.Number)).ToList();
            }
        }

        public IList<Issue> OpenSprintIssues
        {
            get
            {
                return (Sprint != null && Sprint.Issues != null && Sprint.Issues.Count > 0)
                        ? OpenIssues.Where(x => !Sprint.Issues.Any(y => y.IssueId == x.Number)).ToList()
                        : OpenIssues.ToList();
            }
        }

        /// <summary>
        /// Create a new issue in GitHub
        /// </summary>
        public NewIssue NewIssue { get; set; }

        public IReadOnlyList<RepositoryContributor> Contributors { get; set; }
    }
}