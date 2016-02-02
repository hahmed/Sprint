using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sprint.Models
{
    public class NewIssueRequest
    {
        /// <summary>
        /// The title of this issue
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Is this issue added to the sprint board be default?
        /// </summary>
        [Required]
        [DefaultValue(true)]
        public bool AddToSprint { get; set; }
    }
}