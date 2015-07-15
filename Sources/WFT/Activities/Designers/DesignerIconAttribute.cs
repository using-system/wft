using System;

namespace WFT.Activities.Designers
{
    /// <summary>
    /// Display icon to the WFT Activity designers
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class DesignerIconAttribute : Attribute
    {
        public DesignerIconAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }

        /// <summary>
        /// Get the resource name
        /// </summary>
        public string ResourceName { get; private set; }
    }
}
