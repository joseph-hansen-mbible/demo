using System;
using UnityEngine;

namespace Assets.Prototypes.Skills.Nodes
{
    /// <summary>
    /// Attribute to categorize skill nodes by their purpose.
    /// Automatically applies tint color based on category.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeCategoryAttribute : Attribute
    {
        public NodeCategory Category { get; }

        public NodeCategoryAttribute(NodeCategory category)
        {
            Category = category;
        }

        public Color GetTintColor()
        {
            return GetCategoryColor(Category);
        }

        /// <summary>
        /// Get the tint color for a specific category without requiring an attribute instance.
        /// </summary>
        public static Color GetCategoryColor(NodeCategory category)
        {
            return category switch
            {
                // Tailwind indigo-800 (#3730a3)
                NodeCategory.Triggers => new Color(55f / 255f, 48f / 255f, 163f / 255f),

                // Tailwind blue-800 (#1e40af)
                NodeCategory.Math => new Color(30f / 255f, 64f / 255f, 175f / 255f),

                // Tailwind purple-800 (#6b21a8)
                NodeCategory.Events => new Color(107f / 255f, 33f / 255f, 168f / 255f),

                // Tailwind violet-800 (#5b21b6)
                NodeCategory.Conditions => new Color(91f / 255f, 33f / 255f, 182f / 255f),

                _ => Color.gray,
            };
        }
    }

    public enum NodeCategory
    {
        Triggers,
        Math,
        Events,
        Conditions,
    }
}
