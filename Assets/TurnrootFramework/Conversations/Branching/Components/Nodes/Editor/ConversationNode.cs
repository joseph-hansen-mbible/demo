using Turnroot.Skills.Nodes;
using TurnrootFramework.Conversations;
using UnityEngine;
using XNode;

[CreateNodeMenu("Conversation/Conversation")]
public class ConversationNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public ExecutionFlow previous;

    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public ExecutionFlow next;

    public ConversationLayer conversationLayer;
}
