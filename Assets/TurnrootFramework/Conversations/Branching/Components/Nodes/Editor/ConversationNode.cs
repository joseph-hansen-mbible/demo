using Turnroot.Skills.Nodes;
using TurnrootFramework.Conversations;
using UnityEngine;
using XNode;

[CreateNodeMenu("Conversation/Conversation")]
public class ConversationNode : Node
{
    [Input]
    public ConversationFlow previous;

    [Output(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public ConversationFlow next;
    public ConversationLayer conversationLayer;
}

[System.Serializable]
public struct ConversationFlow { }
