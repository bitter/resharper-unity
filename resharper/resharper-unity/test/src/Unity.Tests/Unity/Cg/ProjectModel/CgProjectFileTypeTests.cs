﻿using JetBrains.ProjectModel;
using JetBrains.ReSharper.Plugins.Unity.Shaders.Cg.ProjectModel;
using JetBrains.ReSharper.Resources.Shell;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.Tests.Unity.Cg.ProjectModel
{
    [RequireHlslSupport]
    [TestFixture]
    public class CgProjectFileTypeTests
    {
        [Test]
        public void ProjectFileTypeIsRegistered()
        {
            Assert.NotNull(CgProjectFileType.Instance);

            var projectFileTypes = Shell.Instance.GetComponent<IProjectFileTypes>();
            Assert.NotNull(projectFileTypes.GetFileType(CgProjectFileType.Name));
        }

        [TestCase(CgProjectFileType.GLSL_EXTENSION)]
        [TestCase(CgProjectFileType.GLSLINC_EXTENSION)]
        public void ProjectFileTypeFromExtensionCginc(string extension)
        {
            var projectFileExtensions = Shell.Instance.GetComponent<IProjectFileExtensions>();
            Assert.AreSame(CgProjectFileType.Instance, projectFileExtensions.GetFileType(extension));
        }
    }
}
