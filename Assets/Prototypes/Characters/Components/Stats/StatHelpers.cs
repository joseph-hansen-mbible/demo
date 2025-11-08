using System.Collections.Generic;

namespace Assets.Prototypes.Characters.Stats
{
    /// <summary>
    /// Helper methods for working with character stats to reduce code duplication.
    /// </summary>
    public static class StatHelpers
    {
        /// <summary>
        /// Finds a bounded stat by type in a list.
        /// </summary>
        public static BoundedCharacterStat GetBoundedStat(
            List<BoundedCharacterStat> stats,
            BoundedStatType type
        )
        {
            return stats?.Find(s => s.StatType == type);
        }

        /// <summary>
        /// Finds an unbounded stat by type in a list.
        /// </summary>
        public static CharacterStat GetUnboundedStat(
            List<CharacterStat> stats,
            UnboundedStatType type
        )
        {
            return stats?.Find(s => s.StatType == type);
        }
    }
}
