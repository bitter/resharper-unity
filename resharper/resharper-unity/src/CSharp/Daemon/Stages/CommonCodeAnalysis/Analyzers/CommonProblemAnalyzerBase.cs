using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.Stages.CallGraphStage;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.Stages.ContextSystem;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.Stages.CommonCodeAnalysis.Analyzers
{
    public abstract class CommonProblemAnalyzerBase<T> : CallGraphProblemAnalyzerBase<T> where T : ITreeNode
    {
        public override CallGraphContextElement Context => CallGraphContextElement.NONE;
        public override CallGraphContextElement ProhibitedContext => CallGraphContextElement.NONE;
    }
}