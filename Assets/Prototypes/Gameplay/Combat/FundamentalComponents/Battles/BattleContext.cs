using System.Collections.Generic;
using Assets.Prototypes.Characters;
using Assets.Prototypes.Gameplay.Combat.FundamentalComponents.Battles.Environment;
using Assets.Prototypes.Skills.Nodes;
using UnityEngine;

namespace Assets.Prototypes.Gameplay.Combat.FundamentalComponents.Battles
{
    /// <summary>
    /// Runtime context for the entire battle.
    /// Contains all the dynamic data that skills and other systems need at runtime.
    /// </summary>
    public class BattleContext
    {
        // Currently executing skill (if any)
        public Skill CurrentSkill { get; set; }

        // All skills and their graphs that can be executed in this battle
        public List<Skill> ActiveSkills { get; set; }
        public List<SkillGraph> ActiveSkillGraphs { get; set; }

        public float SkillUseCount { get; set; }
        public CharacterInstance UnitInstance { get; set; }
        public List<CharacterInstance> Targets { get; set; }
        public List<CharacterInstance> Allies { get; set; }
        public Adjacency AdjacentUnits { get; set; }

        // Currently executing skill graph (if any)
        public SkillGraph CurrentSkillGraph { get; set; }

        public EnvironmentalConditionsInstance EnvironmentalConditions { get; set; }
        public Dictionary<string, object> CustomData { get; private set; }

        public bool IsInterrupted { get; set; }

        public BattleContext()
        {
            CustomData = new Dictionary<string, object>();
            Targets = new List<CharacterInstance>();
            Allies = new List<CharacterInstance>();
            AdjacentUnits = new Adjacency();
            ActiveSkills = new List<Skill>();
            ActiveSkillGraphs = new List<SkillGraph>();
        }

        // Get a custom data value, or default if not found
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (CustomData.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        // Set a custom data value
        public void SetCustomData(string key, object value)
        {
            CustomData[key] = value;
        }
    }

    /// <summary>
    /// Backwards compatibility alias for BattleContext.
    /// Use BattleContext for new code.
    /// </summary>
    public class SkillExecutionContext : BattleContext
    {
        public Skill Skill
        {
            get => CurrentSkill;
            set => CurrentSkill = value;
        }

        public SkillGraph SkillGraph
        {
            get => CurrentSkillGraph;
            set => CurrentSkillGraph = value;
        }
    }
}
