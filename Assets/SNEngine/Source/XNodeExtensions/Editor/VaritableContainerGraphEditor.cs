using System;
using System.Collections.Generic;

namespace SiphoinUnityHelpers.XNodeExtensions.Editor
{
    [CustomNodeGraphEditor(typeof(SNEngine.Graphs.VaritableContainerGraph))]
    public class VaritableContainerGraphEditor : FilteredNodeGraphEditor
    {
        private static readonly Dictionary<Type, bool> _typeCache = new Dictionary<Type, bool>();

        protected override bool IsNodeTypeAllowed(Type nodeType)
        {
            if (_typeCache.TryGetValue(nodeType, out bool allowed))
                return allowed;

            allowed = CheckIfAllowed(nodeType);
            _typeCache[nodeType] = allowed;
            return allowed;
        }

        private bool CheckIfAllowed(Type nodeType)
        {
            if (typeof(VaritableNode).IsAssignableFrom(nodeType))
                return true;

            Type currentType = nodeType;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType.IsGenericType &&
                    currentType.GetGenericTypeDefinition().Name.Contains("SetVaritableNode"))
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }

            return nodeType.Name.Contains("SetVaritable");
        }
    }
}