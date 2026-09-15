using System;
using System.Collections.Generic;
using System.Text;

namespace Editor.Exceptions
{
    public class WindowNotImplementedException : Exception
    {
        public WindowNotImplementedException(string type) : base($"Window of type '{type}' is not implemented!") { }
    }
}
