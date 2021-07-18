using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace CodeGenTests
{
    public abstract class GeneratorTests<T> where T : ISourceGenerator, new()
    {
        protected readonly ITestOutputHelper _output;

        protected GeneratorTests(ITestOutputHelper output) => _output = output ?? throw new ArgumentNullException(nameof(output));

        public (string, string) GetGeneratedOutput(string source, bool executable = false)
        {
            var outputCompilation = CreateCompilation(source, executable);
            var trees = outputCompilation.SyntaxTrees.Reverse().Take(2).Reverse().ToList();
            foreach (var tree in trees)
            {
                _output.WriteLine(Path.GetFileName(tree.FilePath) + ":");
                _output.WriteLine(tree.ToString());
            }
            return (trees.First().ToString(), trees[1].ToString());
        }

        private static Compilation CreateCompilation(string source, bool executable)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));

            var compilation = CSharpCompilation.Create("Foo",
                                                       new SyntaxTree[] { syntaxTree },
                                                       references,
                                                       new CSharpCompilationOptions(executable ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary));

            var generator = new T();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

            var compileDiagnostics = outputCompilation.GetDiagnostics();
            compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error).Should().BeFalse("Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

            generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error).Should().BeFalse("Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());
            return outputCompilation;
        }
    }
}
