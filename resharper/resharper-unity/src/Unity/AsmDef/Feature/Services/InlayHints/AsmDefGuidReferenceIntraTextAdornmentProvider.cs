using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.TextControl.DocumentMarkup.IntraTextAdornments;

#nullable enable

namespace JetBrains.ReSharper.Plugins.Unity.AsmDef.Feature.Services.InlayHints
{
    [SolutionComponent]
    public class AsmDefGuidReferenceIntraTextAdornmentProvider : IHighlighterIntraTextAdornmentProvider
    {
        private readonly ISolution mySolution;
        private readonly ISettingsStore mySettingsStore;

        public AsmDefGuidReferenceIntraTextAdornmentProvider(ISolution solution, ISettingsStore settingsStore)
        {
            mySolution = solution;
            mySettingsStore = settingsStore;
        }

        public bool IsValid(IHighlighter highlighter)
        {
            return highlighter.UserData is AsmDefGuidReferenceInlayHintHighlighting highlighting &&
                   highlighting.IsValid();
        }

        public IIntraTextAdornmentDataModel? CreateDataModel(IHighlighter highlighter)
        {
            if (highlighter.UserData is AsmDefGuidReferenceInlayHintHighlighting highlighting && highlighting.IsValid())
            {
                return new AsmDefIntraTextAdornmentModel(highlighting, s => s.ShowAsmDefGuidReferenceNames, mySolution,
                    mySettingsStore);
            }

            return null;
        }
    }
}