using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Plugins.Unity.Rider.CSharp.Feature.RunMarkers;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Unity.Tests.Rider
{
    [SolutionComponent]
    public class ShowMarkerOnStaticMethodSettingsStub : ShowMarkerOnStaticMethodSettings
    {
        public ShowMarkerOnStaticMethodSettingsStub(ISettingsSchema settingsSchema, ILogger logger)
            : base(settingsSchema, logger, true)
        {
        }
    }
}