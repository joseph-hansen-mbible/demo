using Assets.Prototypes.Skills.Nodes;
using UnityEngine;
using XNode;

[CreateNodeMenu("Triggers/Unit Attacks")]
public class UnitAttacks : SkillNode
{
    [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
    public ExecutionFlow execOut;

    public override void Execute(SkillExecutionContext context)
    {
        SignalComplete(context);
    }

    public override object GetValue(NodePort port)
    {
        return null;
    }
}
