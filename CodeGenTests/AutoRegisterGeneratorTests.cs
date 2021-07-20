using CodeGen.Generators;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CodeGenTests
{
    public class AutoRegisterGeneratorTests : GeneratorTests<AutoRegisterGenerator>
    {
        public AutoRegisterGeneratorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [FileData("Sources\\WebApp.txt", "Sources\\ExpectedAttributeCode.txt", "Sources\\ExpectedExtensionCode.txt")]
        [FileData("Sources\\FullWebApp.txt", "Sources\\ExpectedAttributeCode.txt", "Sources\\FullExpectedExtensionCode.txt")]
        public void GeneratedCodeWithoutServicesWork(string source, string expectedAttributeCode, string expectedExtensionCode)
        {
            var (attributeCode, extensionCode) = GetGeneratedOutput(source);

            attributeCode.Should().NotBeNull().And.Be(expectedAttributeCode);
            extensionCode.Should().NotBeNull().And.Be(expectedExtensionCode);
        }
    }
}
