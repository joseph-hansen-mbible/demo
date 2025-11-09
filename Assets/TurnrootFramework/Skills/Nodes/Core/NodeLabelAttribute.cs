using System;

namespace Turnroot.Skills.Nodes
{
    /// <summary>
    /// Attribute to display a custom label at the top of a skill node.
    /// The label is set programmatically and cannot be modified in the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeLabelAttribute : Attribute
    {
        public string Label { get; }

        public NodeLabelAttribute(string label)
        {
            Label = label;
        }
    }
}
