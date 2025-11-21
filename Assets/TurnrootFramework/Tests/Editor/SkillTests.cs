using NUnit.Framework;
using UnityEngine;

public class SkillTests
{
    [Test]
    public void Skill_TriggerAndExecute_NoGraph_NoException()
    {
        var skill = ScriptableObject.CreateInstance<Skill>();
        skill.SkillName = "UnitTestSkill";

        // No BehaviorGraph assigned; ExecuteSkill should handle this gracefully
        var ctx = new Turnroot.Gameplay.Combat.FundamentalComponents.Battles.BattleContext();
        // Should not throw when no behavior graph is assigned
        Assert.DoesNotThrow(() => skill.ExecuteSkill(ctx));
    }

    [Test]
    public void SkillInstance_SetReadyAndEquipped_TriggersEvents()
    {
        var skill = ScriptableObject.CreateInstance<Skill>();
        skill.SkillName = "InstSkill";

        // assign events
        skill.SkillEquipped = new UnityEngine.Events.UnityEvent();
        skill.SkillUnequipped = new UnityEngine.Events.UnityEvent();

        bool equippedTriggered = false;
        bool unequippedTriggered = false;
        skill.SkillEquipped.AddListener(() => equippedTriggered = true);
        skill.SkillUnequipped.AddListener(() => unequippedTriggered = true);

        var instance = new SkillInstance(skill);
        instance.SetReadyToFire(true);
        Assert.IsTrue(instance.ReadyToFire);

        instance.SetEquipped(true);
        Assert.IsTrue(instance.Equipped);
        Assert.IsTrue(equippedTriggered);

        instance.SetEquipped(false);
        Assert.IsFalse(instance.Equipped);
        Assert.IsTrue(unequippedTriggered);
    }
}
