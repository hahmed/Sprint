using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sprint.Models
{
    public static class Ensure
    {
        public static void NotNull(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(argument);
            }
        }
    }
}