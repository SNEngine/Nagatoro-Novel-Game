using Cysharp.Threading.Tasks;
using SiphoinUnityHelpers.XNodeExtensions.AsyncNodes;
using SiphoinUnityHelpers.XNodeExtensions.Debugging;
using SiphoinUnityHelpers.XNodeExtensions.Extensions;
using SNEngine.AsyncNodes;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using XNode;

namespace SiphoinUnityHelpers.XNodeExtensions
{
    [NodeTint("#593d6b")]
    public abstract class NodeControlExecute : BaseNodeInteraction, IIncludeWaitingNode
    {
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsWorking => _cancellationTokenSource != null;

        public void SkipWait()
        {
        }

        protected async UniTask ExecuteNodesFromPort(NodePort port)
        {
            var connections = port.GetConnections();

            if (connections != null)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                foreach (var connect in connections)
                {
                    if (connect.Connection != null)
                    {
                        var node = connect.node as BaseNodeInteraction;
                        if (node == null) continue;

                        await ExecuteAndHighlightBranch(node);
                    }
                }
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private async UniTask ExecuteAndHighlightBranch(BaseNodeInteraction node)
        {
            NodeHighlighter.HighlightNode(node, Color.cyan);

            node.Execute();

            if (node is IIncludeWaitingNode waitingNode)
            {
                await XNodeExtensionsUniTask.WaitAsyncNode(waitingNode, _cancellationTokenSource);
            }

            var exitPort = node.GetExitPort();
            if (exitPort != null && exitPort.IsConnected)
            {
                var nextNode = exitPort.Connection.node as BaseNodeInteraction;
                if (nextNode != null)
                {
                    await ExecuteAndHighlightBranch(nextNode);
                }
            }

            NodeHighlighter.RemoveHighlight(node);
        }
    }
}