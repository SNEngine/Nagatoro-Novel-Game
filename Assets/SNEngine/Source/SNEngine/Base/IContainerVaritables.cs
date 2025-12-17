using SiphoinUnityHelpers.XNodeExtensions;
using System.Collections.Generic;

namespace SNEngine.Graphs
{
    public interface IContainerVaritables : IResetable
    {
        IDictionary<string, VaritableNode> GlobalVaritables { get; }
    }
}
