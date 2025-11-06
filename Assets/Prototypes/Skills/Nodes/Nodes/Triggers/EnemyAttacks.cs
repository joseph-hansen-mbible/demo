using Assets.Prototypes.Skills.Nodes;
using UnityEngine;
using XNode;

[CreateNodeMenu("Triggers/Enemy Attacks")]
public class EnemyAttacks : SkillNode
{
    [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
    public ExecutionFlow flow;

    public override void Execute(SkillExecutionContext context)
    {
        SignalComplete(context);
    }

    public override object GetValue(NodePort port)
    {
        return null;
    }
}
