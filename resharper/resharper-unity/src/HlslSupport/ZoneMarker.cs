﻿using JetBrains.Application.BuildScript.Application.Zones;
 using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.Plugins.Unity.HlslSupport
{
  [ZoneMarker]
  public class ZoneMarker :
    IRequire<ILanguageCppZone>
  {
  }
}