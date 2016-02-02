using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Sprint.Models
{
    public class StatsViewModel
    {
        /// <summary>
        /// What is the most starred repo this year in the csharp language
        /// </summary>
        public IList<Repository> MostStarred { get; set; }

        /// <summary>
        /// What are the most commented issues this year, must have minimum of 25 comments
        /// </summary>
        public IList<Issue> MostCommentedIssue { get; set; }

        /// <summary>
        /// What are the rails users fav day of reporting issues
        /// </summary>
        public Dictionary<DayOfWeek, List<Issue>> RailsIssues { get; set; }
    }
}