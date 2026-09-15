using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Editor.ProjectSystem
{
    public class ProjectFileTemplate
    {
        public string? template { get; set; }
        public ProjectContext? ctx;
        
        public ProjectFileTemplate(ProjectContext? pctx, string? tmplt) 
        {

            ctx = pctx;
            template = tmplt;
        }
    }
}
