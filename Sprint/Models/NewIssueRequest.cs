using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sprint.Models
{
    public class NewIssueRequest
    {
        /// <summary>
        /// The title of this issue
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Is this issue added to the sprint board be default?
        /// </summary>
        public bool AddToSprint { get; set; }
    }
}