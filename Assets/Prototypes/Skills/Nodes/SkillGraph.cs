using UnityEngine;
using XNode;

namespace Assets.Prototypes.Skills.Nodes
{
    /// <summary>
    /// A visual node graph that defines skill behavior as a sequence of connected nodes.
    /// Use SkillGraphExecutor to run this graph at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillGraph", menuName = "Turnroot/Skills/Skill Graph")]
    public class SkillGraph : NodeGraph
    {
        /// <summary>
        /// Execute this graph with the given context.
        /// Creates an executor and runs all connected nodes starting from entry points.
        /// </summary>
        public void Execute(SkillExecutionContext context)
        {
            var executor = new SkillGraphExecutor(this);
            executor.Execute(context);
        }

        /// <summary>
        /// Validate this graph (can be called from editor).
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            errorMessage = null;

            // Check if graph has any nodes
            if (nodes.Count == 0)
            {
                errorMessage = "Graph has no nodes.";
                return false;
            }

            // Check if there's at least one SkillNode
            bool hasSkillNode = false;
            foreach (var node in nodes)
            {
                if (node is SkillNode)
                {
                    hasSkillNode = true;
                    break;
                }
            }

            if (!hasSkillNode)
            {
                errorMessage = "Graph has no SkillNode instances.";
                return false;
            }

            return true;
        }
    }
}
