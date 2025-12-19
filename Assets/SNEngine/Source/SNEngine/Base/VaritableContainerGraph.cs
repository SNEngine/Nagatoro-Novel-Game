using SiphoinUnityHelpers.XNodeExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace SNEngine.Graphs
{
    [CreateAssetMenu(menuName = "SNEngine/VaritableContainerGraph")]
    public class VaritableContainerGraph : BaseGraph, IContainerVaritables
    {
        public IDictionary<string, VaritableNode> GlobalVaritables => Varitables;

        public override void Execute()
        {
            BuidVaritableNodes();
        }

        public void ResetState()
        {
            foreach (var varitable in Varitables.Values)
            {
                varitable.ResetValue();
            }
        }

        public override string GetWindowTitle()
        {
            return "Global Varitables";
        }
    }
}
