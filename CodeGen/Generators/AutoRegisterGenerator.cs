using CodeGen.Extensions;
using CodeGen.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CodeGen.Generators
{
    [Generator]
    public class AutoRegisterGenerator : ISourceGenerator
    {
        private const string AttributeText = @"using System;

namespace CodeGen.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class AutoRegisterAttribute : Attribute
    {
        public AutoRegisterAttribute()
        {
        }
    }
}";

        private const string ExtensionHeader = @"using Microsoft.Extensions.DependencyInjection;

namespace CodeGeneratorDemo.SourceGeneratorDemo.Extensions
{
    public static class AutoRegisterExtensions
    {
        public static void AutoRegister(this IServiceCollection services)
        {
";

        private const string ExtensionFooter = @"        }
    }
}";
        private const string spaces = "            ";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(c => c.AddSource("Attributes.cs", SourceText.From(AttributeText, Encoding.UTF8)));
            context.RegisterForSyntaxNotifications(() => new AutoRegisterSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AutoRegisterSyntaxReceiver receiver))
            {
                return;
            }

            var registrations = new StringBuilder(ExtensionHeader);

            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            SyntaxTree attributeSyntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(AttributeText, Encoding.UTF8), options);
            Compilation compilation = context.Compilation.AddSyntaxTrees(attributeSyntaxTree);

            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("CodeGen.Attributes.AutoRegisterAttribute");

            foreach (var candidateClass in receiver.CandidateClasses)
            {
                SemanticModel model = compilation.GetSemanticModel(candidateClass.SyntaxTree);
                AttributeData attributeData;
                if (model.GetDeclaredSymbol(candidateClass) is ITypeSymbol typeSymbol && typeSymbol.TryAttributeData(attributeSymbol, out attributeData))
                {
                    registrations.Append(spaces);
                    registrations.AppendLine($"services.AddScoped<{typeSymbol.GetFullName()}>();");

                    foreach (var interf in typeSymbol.AllInterfaces)
                    {
                        registrations.Append(spaces);
                        registrations.AppendLine($"services.AddScoped<{interf.GetFullName()}, {typeSymbol.GetFullName()}>();");
                    }
                }
            }

            registrations.Append(ExtensionFooter);

            var source = registrations.ToString();

            context.AddSource("AutoRegisterExtensions.cs", SourceText.From(source, Encoding.UTF8));
        }
    }
}
