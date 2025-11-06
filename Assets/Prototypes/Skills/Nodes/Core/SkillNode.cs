using UnityEngine;
using UnityEngine.Events;
using XNode;

namespace Assets.Prototypes.Skills.Nodes
{
    /// <summary>
    /// Base class for all skill nodes. Provides execution flow and data evaluation.
    /// </summary>
    public abstract class SkillNode : Node
    {
        public UnityEvent OnNodeExecute;

        protected override void Init()
        {
            base.Init();
            if (OnNodeExecute == null)
            {
                OnNodeExecute = new UnityEvent();
            }
        }

        /// <summary>
        /// Execute this node's logic with the given context.
        /// Override this in child classes to implement node behavior.
        /// </summary>
        /// <param name="context">Runtime execution context containing all needed data</param>
        public abstract void Execute(SkillExecutionContext context);

        /// <summary>
        /// Called by external systems (like animation events) to signal that this node can proceed.
        /// Default implementation immediately continues to next nodes.
        /// Override if you need to wait for something (animation, timing, etc).
        /// </summary>
        public virtual void SignalComplete(SkillExecutionContext context)
        {
            // Default: immediately execute next nodes
            var executor = context.GetCustomData<SkillGraphExecutor>("_executor");
            if (executor != null)
            {
                executor.ContinueFromNode(this);
            }
        }
    }

    [System.Serializable]
    public struct ExecutionFlow
    {
    }

    [System.Serializable]
    public struct BoolValue
    {
        public bool value;
    }

    [System.Serializable]
    public struct FloatValue
    {
        public float value;
    }

    [System.Serializable]
    public struct StringValue
    {
        public string value;
    }
}
