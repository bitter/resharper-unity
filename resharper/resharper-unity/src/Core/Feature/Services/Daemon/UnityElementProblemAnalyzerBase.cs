using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Plugins.Unity.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;

#nullable enable

namespace JetBrains.ReSharper.Plugins.Unity.Core.Feature.Services.Daemon
{
    public abstract class UnityElementProblemAnalyzerBase<TElement, TLanguage> : ElementProblemAnalyzer<TElement>,
                                                                                 IConditionalElementProblemAnalyzer
        where TElement : ITreeNode
        where TLanguage : PsiLanguageType
    {
        public virtual bool ShouldRun(IFile file, ElementProblemAnalyzerData data)
        {
            // Run for visible documents and SWEA. Also run for "other", which is used by scoped quick fixes
            if (data.GetDaemonProcessKind() == DaemonProcessKind.GLOBAL_WARNINGS)
                return false;

            if (!file.GetSolution().HasUnityReference())
                return false;

            if (data.SourceFile == null || !file.Language.Is<TLanguage>())
                return false;

            return IsAcceptableFile(data.SourceFile, data.File);
        }

        protected abstract bool IsAcceptableFile(IPsiSourceFile sourceFile, IFile file);
    }
}