using System;
using System.Collections.Generic;
using System.Text;

namespace Editor.ProjectSystem
{
    public class ProjectContext
    {
        public string ProjectFriendlyName { get; set; }
        public string ProjectNamespace { get; set; }
        public string ProjectAuthor { get; set; }
        public VerContMethod ProjectVcMethod { get; set; }
    }
}
