using Editor.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using Xenon.Context;

namespace Editor
{
    public abstract class EditorWindow
    {
        public string Title { get; set; }
        public abstract void Window(double dt, RenderContext ctx);

        public EditorWindow(string title)
        {
            this.Title = title;
        }
    }
}
