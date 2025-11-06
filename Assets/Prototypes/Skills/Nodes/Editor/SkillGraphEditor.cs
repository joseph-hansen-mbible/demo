using System;
using Assets.Prototypes.Skills.Nodes;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Assets.Prototypes.Skills.Nodes.Editor
{
    /// <summary>
    /// Custom NodeGraphEditor that overrides port colors for skill node socket types.
    /// Colors from Tailwind CSS 500 shades.
    /// </summary>
    [CustomNodeGraphEditor(typeof(SkillGraph))]
    public class SkillGraphEditor : NodeGraphEditor
    {
        public override Color GetTypeColor(Type type)
        {
            // Execution: orange-500 (#f97316)
            if (type == typeof(ExecutionFlow))
            {
                ColorUtility.TryParseHtmlString("#F97316", out Color color);
                return color;
            }

            // Bool: violet-500 (#8b5cf6)
            if (type == typeof(BoolValue))
            {
                ColorUtility.TryParseHtmlString("#8B5CF6", out Color color);
                return color;
            }

            // Float: sky-500 (#0ea5e9)
            if (type == typeof(FloatValue))
            {
                ColorUtility.TryParseHtmlString("#0EA5E9", out Color color);
                return color;
            }

            // String: teal-500 (#14b8a6)
            if (type == typeof(StringValue))
            {
                ColorUtility.TryParseHtmlString("#14B8A6", out Color color);
                return color;
            }

            // Fall back to default for other types
            return base.GetTypeColor(type);
        }
    }
}
