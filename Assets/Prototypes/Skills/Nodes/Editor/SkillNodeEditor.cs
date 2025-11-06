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
            // Fall back to default tint
            return base.GetTint();
        }
    }
}
