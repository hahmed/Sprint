using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Sprint.Models
{
    public class PullRequestReviewViewModel
    {
        /// <summary>
        /// owner of repo
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// Current repo
        /// </summary>
        public Repository Repository { get; set; }

        public IOrderedEnumerable<KeyValuePair<string, List<PullRequest>>> PullRequests { get; set; }
    }
}