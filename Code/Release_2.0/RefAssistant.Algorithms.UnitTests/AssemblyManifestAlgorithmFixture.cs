using NUnit.Framework;

namespace Lardite.RefAssistant.Algorithms.UnitTests
{
    [TestFixture]
    public class AssemblyManifestAlgorithmFixture
    {
        [Test]
        public void Process_Assembly_ReturnsManifestAssemblies()
        {
            var manifestAssembly1Obj = MockUtils.CreateAssemblyMock("Assembly_I").Object;
            var manifestAssembly2Obj = MockUtils.CreateAssemblyMock("Assembly_II").Object;
            var manifestAssembly3Obj = MockUtils.CreateAssemblyMock("Assembly_III").Object;

            var inputAssemblyObj = MockUtils.CreateAssemblyMock(
                "Assembly_I",
                manifestAssemblies: new[]
                    {
                        manifestAssembly1Obj,
                        manifestAssembly2Obj,
                        manifestAssembly3Obj
                    }).Object;

            var algorithm = new AssemblyManifestAlgorithm();
            var result = algorithm.Process(inputAssemblyObj);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RequiredFor.Count);
            Assert.AreEqual(typeof(AssemblyManifestAlgorithm).FullName, result.AlgorithmAdvice);
        }
    }
}
