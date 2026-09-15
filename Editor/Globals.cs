using Editor.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Editor
{
    public static class Globals
    {
        public static Program program = new Program();

        public static ProjectContext CurrentProjectCtx = new ProjectContext
        {
            ProjectFriendlyName = "Test",
            ProjectNamespace = "TestProject",
            ProjectAuthor = "Xtimhedi",
            ProjectVcMethod = VerContMethod.SC_NONE
        };
    }
}
