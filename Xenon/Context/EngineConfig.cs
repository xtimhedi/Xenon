using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Context
{
    public struct EngineConfig
    {
        public double TargetFramerate { get; set; }
        public bool VsyncEnable { get; set; }

    }
}
