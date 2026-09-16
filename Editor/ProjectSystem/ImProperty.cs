using System;
using System.Collections.Generic;
using System.Text;

namespace Editor.ProjectSystem
{
    public class ImProperty<Type>
    {
        public Type Value { get; set; }

        public const string String = typeof(int).ToString();

        public void EnumValue()
        {
            switch (typeof(Type).ToString())
            {
                case :
                    break;
            }
        }
    }
}
