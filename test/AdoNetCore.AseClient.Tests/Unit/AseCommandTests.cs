using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public class AseCommandTests
    {
        [Test]
        public void RepeatedDisposal_DoesNotThrow()
        {
            var command = new AseCommand();
            command.Dispose();
            command.Dispose();
        }
    }
}
