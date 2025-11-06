using System.Collections.Generic;
using UnityEngine;

namespace Assets.Prototypes.Skills.Nodes
{
    /// <summary>
    /// Runtime context passed through skill node execution.
    /// Contains all the dynamic data that nodes need to evaluate at runtime.
    /// </summary>
    public class SkillExecutionContext
    {
        /// <summary>
        /// The character/unit that is using this skill
        /// </summary>
        public GameObject Caster { get; set; }

        /// <summary>
        /// The primary target of this skill (if any)
        /// </summary>
        public GameObject Target { get; set; }

        /// <summary>
        /// Additional targets for multi-target skills
        /// </summary>
        public List<GameObject> AdditionalTargets { get; set; }

        /// <summary>
        /// Accumulated damage value (nodes can add to this)
        /// </summary>
        public float DamageValue { get; set; }

        /// <summary>
        /// Accumulated healing value (nodes can add to this)
        /// </summary>
        public float HealingValue { get; set; }

        /// <summary>
        /// The skill graph being executed
        /// </summary>
        public SkillGraph SkillGraph { get; set; }

        /// <summary>
        /// The skill asset that owns this graph
        /// </summary>
        public Skill Skill { get; set; }

        /// <summary>
        /// Generic data bag for custom values between nodes
        /// </summary>
        public Dictionary<string, object> CustomData { get; private set; }

        /// <summary>
        /// Whether execution should be interrupted
        /// </summary>
        public bool IsInterrupted { get; set; }

        public SkillExecutionContext()
        {
            CustomData = new Dictionary<string, object>();
            AdditionalTargets = new List<GameObject>();
        }

        /// <summary>
        /// Get a custom data value, or default if not found
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (CustomData.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Set a custom data value
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            CustomData[key] = value;
        }
    }
}
