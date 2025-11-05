using System.Collections.Generic;

namespace Assets.Prototypes.Characters.Stats
{
    public enum BoundedStatType
    {
        Health,
    }

    public enum UnboundedStatType
    {
        Strength,
    }

    public static class BoundedStatTypeExtensions
    {
        private static readonly Dictionary<
            BoundedStatType,
            (string DisplayName, string Description)
        > StatInfo = new() { { BoundedStatType.Health, ("Health", "Character's life force") } };

        public static string GetDisplayName(this BoundedStatType type) =>
            StatInfo[type].DisplayName;

        public static string GetDescription(this BoundedStatType type) =>
            StatInfo[type].Description;
    }

    public static class UnboundedStatTypeExtensions
    {
        private static readonly Dictionary<
            UnboundedStatType,
            (string DisplayName, string Description)
        > StatInfo = new()
        {
            { UnboundedStatType.Strength, ("Strength", "Physical power and melee damage") },
        };

        public static string GetDisplayName(this UnboundedStatType type) =>
            StatInfo[type].DisplayName;

        public static string GetDescription(this UnboundedStatType type) =>
            StatInfo[type].Description;
    }
}
