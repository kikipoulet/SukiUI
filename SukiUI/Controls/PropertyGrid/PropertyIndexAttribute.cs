using System;

namespace SukiUI.Controls
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyIndexAttribute : Attribute
    {
        public int Index { get; private set; }

        public PropertyIndexAttribute(int index)
        {
            Index = index;
        }
    }
}
