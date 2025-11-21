using NUnit.Framework;
using Turnroot.Skills.Nodes;
using UnityEngine;
using Turnroot.Gameplay.Combat.FundamentalComponents.Battles;

public class SkillGraphTests
{
    // Minimal test node that records execution
    public class TestNode : SkillNode
    {
        [Input] public ExecutionFlow execIn;
        [Output] public ExecutionFlow execOut;

        public bool Executed = false;

        public override void Execute(BattleContext context)
        {
            Executed = true;
        }
    }

    [Test]
    public void SkillGraph_SingleNode_ExecutesEntry()
    {
        var graph = ScriptableObject.CreateInstance<SkillGraph>();
        var node = graph.AddNode<TestNode>();

        var ctx = new BattleContext();

        // Should not throw and node should be executed
        Assert.DoesNotThrow(() => graph.Execute(ctx));
        Assert.IsTrue(((TestNode)node).Executed);
    }

    [Test]
    public void SkillGraph_Proceed_ChainsToNextNode()
    {
        var graph = ScriptableObject.CreateInstance<SkillGraph>();
        var a = graph.AddNode<TestNode>();
        var b = graph.AddNode<TestNode>();

        // Connect a.execOut -> b.execIn
        var outPort = a.GetOutputPort("execOut");
        var inPort = b.GetInputPort("execIn");
        outPort.Connect(inPort);

        var ctx = new BattleContext();

        graph.Execute(ctx);
        Assert.IsTrue(((TestNode)a).Executed);

        // Proceed should cause executor to continue and execute b
        graph.Proceed();
        Assert.IsTrue(((TestNode)b).Executed);
    }
}
