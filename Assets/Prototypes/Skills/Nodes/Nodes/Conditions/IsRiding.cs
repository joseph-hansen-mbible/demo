using Assets.Prototypes.Skills.Nodes;
using UnityEngine;
using XNode;

[CreateNodeMenu("Conditions/Is Riding")]
[NodeLabel("Checks if the unit is riding")]
public class IsRiding : SkillNode
{
    [Output]
    BoolValue UnitRiding;

    [Output]
    BoolValue EnemyRiding;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "UnitRiding")
        {
            BoolValue unitRiding = new();
            // TODO: Implement runtime retrieval of unit riding status
            return unitRiding;
        }
        else if (port.fieldName == "EnemyRiding")
        {
            BoolValue enemyRiding = new();
            // TODO: Implement runtime retrieval of enemy riding status
            return enemyRiding;
        }
        return null;
    }
}
