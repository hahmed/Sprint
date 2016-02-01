using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Sprint.Models
{
    public class TeamViewModel
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
        public IReadOnlyList<RepositoryContributor> Team { get; set; }

        /// <summary>
        /// The current sprint
        /// </summary>
        public Sprint Sprint { get; set; }
    }

}