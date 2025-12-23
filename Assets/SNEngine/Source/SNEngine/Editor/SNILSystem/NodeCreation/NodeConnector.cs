using System;
using System.Collections.Generic;
using SiphoinUnityHelpers.XNodeExtensions;
using SiphoinUnityHelpers.XNodeExtensions.NodesControlExecutes;
using SNEngine.Graphs;
using UnityEngine;

namespace SNEngine.Editor.SNILSystem.NodeCreation
{
    public class NodeConnector
    {
        public static void ConnectFunctionBodyToGroup(GroupCallsNode groupNode, List<BaseNode> bodyNodes)
        {
            if (bodyNodes.Count == 0) return;

            // Подключаем первую ноду тела к порту _operations GroupCallsNode
            var operationsPort = groupNode.GetOutputPort("_operations");
            var firstNodeEnterPort = bodyNodes[0] is BaseNodeInteraction interaction ? interaction.GetEnterPort() : null;

            if (operationsPort != null && firstNodeEnterPort != null)
            {
                operationsPort.Connect(firstNodeEnterPort);
            }

            // Подключаем остальные ноды тела последовательно
            for (int i = 0; i < bodyNodes.Count - 1; i++)
            {
                if (bodyNodes[i] is BaseNodeInteraction curr && bodyNodes[i + 1] is BaseNodeInteraction next)
                {
                    var outPort = curr.GetExitPort();
                    var inPort = next.GetEnterPort();
                    if (outPort != null && inPort != null) outPort.Connect(inPort);
                }
            }

            // Позиционируем ноды тела функции выше основного потока
            NodePositioner.PositionFunctionBodyNodes(groupNode, bodyNodes);
        }

        public static void ConnectNodesSequentially(List<BaseNode> nodes)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                if (nodes[i] is BaseNodeInteraction curr && nodes[i + 1] is BaseNodeInteraction next)
                {
                    var outPort = curr.GetExitPort();
                    var inPort = next.GetEnterPort();
                    if (outPort != null && inPort != null) outPort.Connect(inPort);
                }
            }
        }
    }
}