using Turnroot.Skills.Nodes;
using TurnrootFramework.Conversations;
using UnityEngine;
using XNode;

[CreateNodeMenu("Conversation/Split By 2 Choices")]
public class SplitByChoices2Node : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public ExecutionFlow previous;

    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public ExecutionFlow ChoiceA;

    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public ExecutionFlow ChoiceB;
}
