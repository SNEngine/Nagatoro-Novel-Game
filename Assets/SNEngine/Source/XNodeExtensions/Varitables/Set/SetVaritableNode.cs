using UnityEngine;
using System.Linq;
using SNEngine.Graphs;
using System.Collections.Generic;

namespace SiphoinUnityHelpers.XNodeExtensions.Varitables.Set
{
    public abstract class SetVaritableNode<T> : BaseNodeInteraction
    {
        [Input(ShowBackingValue.Never, ConnectionType.Override), SerializeField] private T _varitable;

        [Input, SerializeField] private T _value;

        [HideInInspector, SerializeField] private string _targetGuid;

        private T _editorOldValue;

        public string TargetGuid { get => _targetGuid; set => _targetGuid = value; }

        public override void Execute()
        {
            var outputVaritable = GetInputPort(nameof(_varitable));
            var inputValue = GetInputPort(nameof(_value));
            var connectedVaritables = outputVaritable.GetConnections();

            object finalValue = _value;
            var connectedValue = inputValue.Connection;

            if (connectedValue != null)
            {
                finalValue = connectedValue.GetOutputValue();
            }

            if (connectedVaritables.Count == 0 && !string.IsNullOrEmpty(_targetGuid))
            {
                VaritableNode targetNode = null;

                if (graph is BaseGraph baseGraph)
                {
                    targetNode = baseGraph.GetNodeByGuid(_targetGuid) as VaritableNode;
                }

                if (targetNode == null)
                {
                    targetNode = FindGlobalNode(_targetGuid);
                }

                if (targetNode is VaritableNode<T> typedNode)
                {
                    SetTypedValue(typedNode, finalValue);
                }
            }
            else
            {
                foreach (var port in connectedVaritables)
                {
                    var connectedVaritable = port.node;

                    if (connectedVaritable is VaritableNode<T> varitableNode)
                    {
                        SetTypedValue(varitableNode, finalValue);
                    }

                    if (connectedVaritable is VaritableCollectionNode<T> collectionNode)
                    {
                        int index = RegexCollectionNode.GetIndex(port);
                        if (finalValue is T castValue)
                        {
                            collectionNode.SetValue(index, castValue);
                        }
                    }
                }
            }
        }

        protected virtual void OnSetTargetValueChanged(VaritableNode<T> targetNode, T newValue) { }

#if UNITY_EDITOR
        protected override void OnEnable()
        {
            base.OnEnable();
            _editorOldValue = _value;
        }

        protected virtual void OnValidate()
        {
            if (!EqualityComparer<T>.Default.Equals(_editorOldValue, _value))
            {
                _editorOldValue = _value;
                OnSetTargetValueChanged(null, _value);
            }
        }
#endif

        private VaritableNode FindGlobalNode(string guid)
        {
            var containers = Resources.LoadAll<VaritableContainerGraph>("");
            foreach (var container in containers)
            {
                var node = container.nodes.OfType<VaritableNode>().FirstOrDefault(n => n.GUID == guid);
                if (node != null) return node;
            }
            return null;
        }

        private void SetTypedValue(VaritableNode<T> node, object value)
        {
            T castValue = default;
            bool success = false;

            if (value is T directValue)
            {
                castValue = directValue;
                success = true;
            }
            else
            {
                try
                {
                    castValue = (T)System.Convert.ChangeType(value, typeof(T));
                    success = true;
                }
                catch { }
            }

            if (success)
            {
                node.SetValue(castValue);
                OnSetTargetValueChanged(node, castValue);
            }
        }
    }
}