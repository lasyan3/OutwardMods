using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using ODebug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnRentStash
{
    public class CustomDialogue
    {
        string m_text;
        List<MultipleChoiceNodeExt.Choice> m_choices;
        List<ActionTask> m_actions;
        List<ConditionTask> m_checks;
    }

    public class TreeNode<T>
    {
        List<TreeNode<T>> Children = new List<TreeNode<T>>();
        T Item { get; set; }

        public TreeNode(T item)
        {
            Item = item;
        }

        public TreeNode<T> AddChild(T item)
        {
            TreeNode<T> nodeItem = new TreeNode<T>(item);
            Children.Add(nodeItem);
            return nodeItem;
        }

        public static TreeNode<CustomDialogue> Convert(List<Node> p_nodes)
        {
            foreach (Node node in p_nodes)
            {
                OLogger.Log($"{node.ID}: {node.GetType().Name} ({node.inConnections}/{node.outConnections})");
            }
            return null;
        }

        public static void DebugDialogue(Node p_node, int p_level, NodeCanvas.Status p_status = NodeCanvas.Status.Running)
        {
            string strTab = "";
            for (int i = 0; i < p_level; i++)
            {
                strTab += " |-";
            }
            strTab += " ";
            if (p_node.GetType().Name == "StatementNodeExt")
            {
                StatementNodeExt n = p_node as StatementNodeExt;
                string msg = n.statement.text;
                if (msg.Length > 60) msg = msg.Substring(0, 60) + "...";
                OLogger.Log($"{strTab}{n.ID}: SAY \"{msg}\"");
                //OLogger.Log($"{strTab} outConnectionType={n.outConnectionType.GetType().Name}");
                //OLogger.Log($"{strTab} isBreakpoint={n.isBreakpoint}");
                //OLogger.Log($"{strTab} allowAsPrime={n.allowAsPrime}");
            }
            else if (p_node.GetType().Name == "MultipleChoiceNodeExt")
            {
                MultipleChoiceNodeExt n = p_node as MultipleChoiceNodeExt;
                OLogger.Log($"{strTab}{n.ID}: MCH");
                foreach (var choice in n.availableChoices)
                {
                    int targetNodeId = n.availableChoices.IndexOf(choice);
                    if (targetNodeId >= 0 && n.outConnections.Count > targetNodeId)
                    {
                        targetNodeId = n.outConnections[targetNodeId].targetNode.ID;
                    }
                    OLogger.Log($"{strTab}> {choice.condition?.name}\"{choice.condition?.GetType().Name} {choice.statement.text}\" --> {targetNodeId}");
                }
            }
            else if (p_node.GetType().Name == "ConditionNode")
            {
                ConditionNode n = p_node as ConditionNode;
                string msg = $"{strTab}{n.ID}: CND";
                if (n.condition != null)
                {
                    /*switch (n.condition.GetType().Name)
                    {
                        default:
                            break;
                    }*/
                    msg += $" {n.condition.GetType().Name}";
                }
                OLogger.Log(msg);
            }
            // TODO : MultipleConditionNode
            else if (p_node.GetType().Name == "ActionNode")
            {
                ActionNode n = p_node as ActionNode;
                OLogger.Log($"{strTab}{n.ID}: ACT {n.action?.GetType().Name} ");
                if (n.action.GetType().Name == "BranchDialogue")
                {
                    OLogger.Log($"{(n.action as BranchDialogue).dialogueStarter.ToString()}");
                }
            }
            else if (p_node.GetType().Name == "FinishNode")
            {
                FinishNode n = p_node as FinishNode;
                OLogger.Log($"{strTab}{n.ID}: END");
            }
            else
            {
                OLogger.Log($"{strTab}{p_node.ID}: [{p_node.GetType().Name}]");
            }

            foreach (var conn in p_node.outConnections)
            {
                DebugDialogue(conn.targetNode, p_level + 1, conn.status);
            }
        }
    }
}
