using Newtonsoft.Json;
using UnityEngine;
using Turnroot.Characters;
using Turnroot.Gameplay.Brain.Components;
using Turnroot.Utilities;
using Turnroot.Gameplay.Brain;
using Turnroot.Characters.Stats;
using Turnroot.Characters.CharacterClass;

namespace Turnroot.Demos
{
    public partial class GameStartManager
    {
        private void CreateAndSaveAvatarInstance(StarGift starGift)
        {
            _ = OperationResultGuards.All(
                OperationResultGuards.RequireNotNull(AvatarData, nameof(AvatarData)),
                OperationResultGuards.RequireNotNull(saveFileBrain, nameof(saveFileBrain))
            );

            var ltm = saveFileBrain.Brain.GetComponent<LongTermMemory>();
            _ = OperationResultGuards.RequireNotNull(ltm, "LongTermMemory component");

            var factory = new CharacterFactory(ltm);
            var persistence = new CharacterPersistence(saveFileBrain.Brain);

            var avatarInstance = factory.CreateOrRecall(AvatarData);
            _ = OperationResultGuards.RequireNotNull(avatarInstance, "avatar instance");

            ApplyStarGiftStatsToInstance(avatarInstance, starGift);
            ApplyStarGiftGrowthRates(starGift);

            // Store birthday using stargift index as month and selected day as date
            int birthdayMonth = Mathf.Clamp(starGift.index, 1, 12);
            int birthdayDay = Mathf.Clamp(selectedBirthdayDay, 1, 31);
            string birthdayKey = $"Avatar/Birthday";
            string birthdayValue = $"{birthdayMonth}/{birthdayDay}";
            ltm.Remember(birthdayKey, birthdayValue);
            $"Set avatar birthday to month {birthdayMonth}, day {birthdayDay}".LogInfo("GameStartManager");

            // Persist name and pronouns so they can be restored to the ScriptableObject
            // on subsequent sessions (SO mutations are in-memory only).
            ltm.Remember("Avatar/DisplayName", AvatarData.DisplayName);
            ltm.Remember("Avatar/FullName", AvatarData.FullName);
            ltm.Remember("Avatar/Pronouns", AvatarData.CharacterPronouns.Singular);

            // Persist growth rates (also on the SO — resets to asset defaults without explicit save).
            ltm.Remember("Avatar/GrowthRates", JsonConvert.SerializeObject(AvatarData.PersonalGrowthRates));

            persistence.SaveCharacter(avatarInstance, updateIndex: true);

            // Instantiate and persist the player roster so it is ready when the hub loads.
            var gamewideContext = saveFileBrain.Brain.gamewideContextBrain;
            if (gamewideContext != null)
            {
                gamewideContext.CreateOrRecallGamewidePersistentPlayerRoster();
                var rosterInstance = gamewideContext.GetPersistentPlayerTeamRosterInstance();
                if (rosterInstance != null)
                {
                    gamewideContext.SavePlayerRoster(lastSavedBattleTurn: 0);
                }
                else
                {
                    "GameStartManager: Could not instantiate player roster — roster will be created on first hub load.".LogWarning("GameStartManager");
                }

                // Persist difficulty and permadeath — GameplayPlayerSettings is a SO and resets
                // to asset defaults without an explicit save through PlayerSettingsPersistence.
                gamewideContext.SavePlayerSettings();
            }
        }

        private void ApplyStarGiftStatsToInstance(CharacterInstance instance, StarGift gift)
        {
            var statMappings = new (UnboundedStatType type, int value)[]
            {
                (UnboundedStatType.Strength, gift.strength),
                (UnboundedStatType.Skill, gift.skill),
                (UnboundedStatType.Defense, gift.defense),
                (UnboundedStatType.Magic, gift.magic),
                (UnboundedStatType.Resistance, gift.resistance),
                (UnboundedStatType.Speed, gift.speed),
                (UnboundedStatType.Luck, gift.luck),
                (UnboundedStatType.Dexterity, gift.dexterity)
            };

            foreach (var (type, value) in statMappings)
            {
                instance.GetUnboundedStat(type)?.SetCurrent(value);
            }
        }

        private void ApplyStarGiftGrowthRates(StarGift gift)
        {
            AvatarData.PersonalGrowthRates.Clear();
            var growthMappings = new (UnboundedStatType type, int growth)[]
            {
                (UnboundedStatType.Strength, gift.strengthGrowth),
                (UnboundedStatType.Skill, gift.skillGrowth),
                (UnboundedStatType.Defense, gift.defenseGrowth),
                (UnboundedStatType.Magic, gift.magicGrowth),
                (UnboundedStatType.Resistance, gift.resistanceGrowth),
                (UnboundedStatType.Speed, gift.speedGrowth),
                (UnboundedStatType.Luck, gift.luckGrowth),
                (UnboundedStatType.Dexterity, gift.dexterityGrowth)
            };

            foreach (var (type, growth) in growthMappings)
            {
                AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(type, growth));
            }

            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(BoundedStatType.Health, 85f));
        }
    }
}