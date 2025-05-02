#nullable enable

using AndanteTribe.Utils.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AndanteTribe.Utils.Tests.Editor
{
    public class EditorTest
    {
        [Test]
        [TestCase(BuildTarget.StandaloneWindows)]
        [TestCase(BuildTarget.StandaloneWindows64)]
        [TestCase(BuildTarget.StandaloneOSX)]
        [TestCase(BuildTarget.StandaloneLinux64)]
        [TestCase(BuildTarget.Android)]
        [TestCase(BuildTarget.iOS)]
        [TestCase(BuildTarget.WebGL)]
        public void CompileCheckTest(BuildTarget platform)
            => Assert.That(TestUtils.SuccessCompile(platform), Is.True);
    }
}