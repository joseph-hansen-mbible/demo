using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Assets.Prototypes.Skills.Nodes.Editor
{
    /// <summary>
    /// Custom NodeEditor that applies category-based tinting to skill nodes.
    /// Automatically determines category from the script's subfolder path.
    /// </summary>
    [CustomNodeEditor(typeof(SkillNode))]
    public class SkillNodeEditor : NodeEditor
    {
        public override Color GetTint()
        {
            // Get the script path for this node type
            var script = MonoScript.FromScriptableObject(target);

            if (script != null)
            {
                string scriptPath = AssetDatabase.GetAssetPath(script);

                // Check if the path contains a category subfolder
                if (scriptPath.Contains("/Triggers/"))
                {
                    return NodeCategoryAttribute.GetCategoryColor(NodeCategory.Triggers);
                }
                else if (scriptPath.Contains("/Math/"))
                {
                    return NodeCategoryAttribute.GetCategoryColor(NodeCategory.Math);
                }
                else if (scriptPath.Contains("/Events/"))
                {
                    return NodeCategoryAttribute.GetCategoryColor(NodeCategory.Events);
                }
                else if (scriptPath.Contains("/Conditions/"))
                {
                    return NodeCategoryAttribute.GetCategoryColor(NodeCategory.Conditions);
                }
            }

            // Fallback: Check if node has NodeCategory attribute (for manual override)
            var categoryAttr = target
                .GetType()
                .GetCustomAttributes(typeof(NodeCategoryAttribute), true);

            if (categoryAttr != null && categoryAttr.Length > 0)
            {
                var attr = (NodeCategoryAttribute)categoryAttr[0];
                return attr.GetTintColor();
            }

            // Fall back to default tint
            return base.GetTint();
        }
    }
}
