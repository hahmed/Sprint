using System;

namespace Sprint.Models
{
    public class SprintIssue
    {
        public int Id { get; set; }

        /// <summary>
        /// Foriegn key to the related Sprint
        /// </summary>
        public int SprintId { get; set; }

        /// <summary>
        /// GitHub's issue id
        /// </summary>
        public int IssueId { get; set; }

        /// <summary>
        /// When this issues was added to the sprint
        /// </summary>
        public DateTimeOffset When { get; set; }
    }
}