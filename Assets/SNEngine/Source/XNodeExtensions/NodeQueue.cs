using Cysharp.Threading.Tasks;
using SiphoinUnityHelpers.XNodeExtensions.AsyncNodes;
using SiphoinUnityHelpers.XNodeExtensions.Debugging;
using SiphoinUnityHelpers.XNodeExtensions.Exceptions;
using SiphoinUnityHelpers.XNodeExtensions.Extensions;
using SNEngine.AsyncNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using XNode;
using SiphoinUnityHelpers.XNodeExtensions.NodesControlExecutes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SiphoinUnityHelpers.XNodeExtensions
{
    public class NodeQueue
    {
        private int _index;
        private StartNode _startNode;
        private CancellationTokenSource _cancellationTokenSource;

        public event Action OnEnd;

        private List<BaseNodeInteraction> _nodes = new List<BaseNodeInteraction>();
        private List<AsyncNode> _asyncNodes = new List<AsyncNode>();
        private List<ExitNode> _exitNodes = new List<ExitNode>();
        private BaseGraph _graph;

        public int Count => _nodes.Count;
        public BaseNode Current => _index < _nodes.Count ? _nodes[_index] : null;
        public bool IsEnding => _index >= Count;

        public IEnumerable<AsyncNode> AsyncNodes => _asyncNodes;
        public IEnumerable<ExitNode> ExitNodes => _exitNodes;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitEditorTracker()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                NodeHighlighter.ClearAllHighlights();
            }
        }
#endif

        public NodeQueue(BaseGraph parentGraph, IEnumerable<BaseNodeInteraction> nodes)
        {
            if (parentGraph == null) throw new ArgumentNullException("parent graph is null");
            if (nodes == null) throw new ArgumentNullException("nodes is null");

            _graph = parentGraph;
            _cancellationTokenSource = new CancellationTokenSource();

            Build(nodes);
        }

        private void Build(IEnumerable<BaseNodeInteraction> nodes)
        {
            var allNodes = nodes.ToList();
            var internalNodes = new HashSet<BaseNodeInteraction>();

            foreach (var node in allNodes)
            {
                if (node is NodeControlExecute)
                {
                    foreach (var port in node.Outputs)
                    {
                        if (port.fieldName == "_exit" || port.fieldName == "Exit") continue;

                        var connections = port.GetConnections();
                        foreach (var conn in connections)
                        {
                            var connectedNode = conn.node as BaseNodeInteraction;
                            if (connectedNode != null)
                            {
                                CollectInternalChain(connectedNode, internalNodes);
                            }
                        }
                    }
                }
            }

            foreach (var node in allNodes)
            {
                if (internalNodes.Contains(node)) continue;

                _nodes.Add(node);

                if (node is AsyncNode asyncNode)
                    _asyncNodes.Add(asyncNode);

                if (node is ExitNode exitNode)
                {
                    _exitNodes.Add(exitNode);
                    exitNode.OnExit += OnExit;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"New Node Queue from node graph {_graph.name}:\n");
            foreach (var node in _nodes)
                stringBuilder.AppendLine(node.name);

            XNodeExtensionsDebug.Log(stringBuilder.ToString());
        }

        private void CollectInternalChain(BaseNodeInteraction node, HashSet<BaseNodeInteraction> visited)
        {
            if (node == null || visited.Contains(node)) return;

            visited.Add(node);

            var exitPort = node.GetExitPort();
            if (exitPort != null && exitPort.IsConnected)
            {
                var nextNode = exitPort.Connection.node as BaseNodeInteraction;
                CollectInternalChain(nextNode, visited);
            }
        }

        public async UniTask<BaseNode> Next()
        {
            if (_index >= Count)
                return null;

            var node = _nodes[_index];

            if (node.Enabled)
            {
                node.Execute();

                if (node is IIncludeWaitingNode asyncNode)
                {
                    XNodeExtensionsDebug.Log($"Wait node <b>{node.name}</b> GUID: <b>{node.GUID}</b>");
                    NodeHighlighter.HighlightNode(node, Color.green);
                    await XNodeExtensionsUniTask.WaitAsyncNode(asyncNode, _cancellationTokenSource);
                    NodeHighlighter.RemoveHighlight(node);
                }
            }

            _index = Mathf.Clamp(_index + 1, 0, _nodes.Count);
            return node;
        }

        private void OnExit(object sender, EventArgs e)
        {
            var node = sender as ExitNode;
            if (node != null) node.OnExit -= OnExit;
            Exit();
        }

        public void Exit()
        {
            NodeHighlighter.ClearAllHighlights();
            _index = 0;
            StopAsyncNodes();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;

            XNodeExtensionsDebug.Log($"node queue from graph {_graph.name} finished");

            OnEnd?.Invoke();
        }

        private void StopAsyncNodes()
        {
            foreach (var item in _asyncNodes)
                item.StopTask();
        }
    }
}