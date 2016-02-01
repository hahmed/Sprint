using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Sprint.Models
{
    public class BacklogViewModel
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
        /// The current sprint
        /// </summary>
        public Sprint Sprint { get; set; }


        public IList<Issue> OpenSprintIssues
        {
            get
            {
                return (Sprint.Issues.Any())
                        ? OpenIssues.Where(x => !Sprint.Issues.Any(y => y.IssueId == x.Number)).ToList()
                        : OpenIssues.ToList();
            }
        }
    }

}