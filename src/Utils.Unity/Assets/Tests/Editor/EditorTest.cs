#nullable enable

using AndanteTribe.Utils.Unity.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AndanteTribe.Utils.Tests.Editor
{
    public class EditorTest
    {
        [Test]
        [TestCase(BuildTarget.StandaloneLinux64)]
        public void CompileCheckTest(BuildTarget platform)
            => Assert.That(TestUtils.SuccessCompile(platform), Is.True);
    }
}