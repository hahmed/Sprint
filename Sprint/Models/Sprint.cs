using System;
using System.Collections.Generic;

namespace Sprint.Models
{
    public class Sprint
    {
        public int Id { get; set; }

        /// <summary>
        /// GitHub repo Id reference
        /// </summary>
        public int RepoId { get; set; }

        public IList<SprintIssue> Issues { get; set; }
    }
}