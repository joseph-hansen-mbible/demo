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
        /// Override this in child classes if you need to perform logic on execution.
        /// Most execution flow nodes can leave this empty and rely on OnNodeExecute UnityEvent.
        /// After execution, the graph will wait for Proceed() to be called (e.g., from a UnityEvent).
        /// </summary>
        /// <param name="context">Runtime execution context containing all needed data</param>
        public virtual void Execute(SkillExecutionContext context)
        {
            // Default: no-op. Override if you need custom logic.
        }

        /// <summary>
        /// Override to provide port values. By default returns null.
        /// </summary>
        public override object GetValue(NodePort port)
        {
            return null;
        }

        /// <summary>
        /// Validate connections to ensure type safety.
        /// Only allow connections between ports of the same type.
        /// </summary>
        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            // Enforce strict type matching
            if (from.ValueType != to.ValueType)
            {
                Debug.LogWarning(
                    $"Cannot connect {from.ValueType.Name} ({from.direction}) to {to.ValueType.Name} ({to.direction}). Types must match."
                );
                return; // Don't call base, prevents connection
            }

            // Check if the target port (input) already has a connection
            // Input ports should only have one connection
            if (to.direction == NodePort.IO.Input && to.ConnectionCount > 0)
            {
                // Check if it's already connected to this exact port
                if (!to.IsConnectedTo(from))
                {
                    // Clear existing connection before making new one
                    to.ClearConnections();
                    Debug.Log($"Replacing existing connection to input port {to.fieldName}");
                }
            }

            base.OnCreateConnection(from, to);
        }
    }

    [System.Serializable]
    public struct ExecutionFlow { }

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
