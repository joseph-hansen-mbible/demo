using Assets.Prototypes.Skills.Nodes;
using UnityEngine;
using XNode;

[CreateNodeMenu("Triggers/Turn Starts")]
public class TurnStarts : SkillNode
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
