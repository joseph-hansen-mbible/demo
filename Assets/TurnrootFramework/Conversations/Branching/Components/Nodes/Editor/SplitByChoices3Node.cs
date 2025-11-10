using Turnroot.Skills.Nodes;
using TurnrootFramework.Conversations;
using UnityEngine;
using XNode;

[CreateNodeMenu("Conversation/Split By 3 Choices")]
public class SplitByChoices3Node : Node
{
    [Input]
    public ExecutionFlow previous;

    [Output]
    public ExecutionFlow ChoiceA;

    [Output]
    public ExecutionFlow ChoiceB;

    [Output]
    public ExecutionFlow ChoiceC;
}
