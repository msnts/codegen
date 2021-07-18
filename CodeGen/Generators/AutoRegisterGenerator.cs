using CodeGen.Extensions;
using CodeGen.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Collections.Generic;
using System.Linq;
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

            var services = new List<string>();
            var namespaces = new List<string>();
            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            SyntaxTree attributeSyntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(AttributeText, Encoding.UTF8), options);
            Compilation compilation = context.Compilation.AddSyntaxTrees(attributeSyntaxTree);

            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("CodeGen.Attributes.AutoRegisterAttribute");

            foreach (var candidateClass in receiver.CandidateClasses)
            {
                SemanticModel model = compilation.GetSemanticModel(candidateClass.SyntaxTree);
                if (model.GetDeclaredSymbol(candidateClass) is ITypeSymbol typeSymbol &&
                    typeSymbol.GetAttributes().Any(x =>
                        x.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    var ns = typeSymbol.GetFullNamespace();
                    if (!string.IsNullOrEmpty(ns) && !namespaces.Contains(ns))
                    {
                        namespaces.Add(ns);
                    }
                    services.Add($"{candidateClass.Identifier.Text}");

                    foreach (var interf in typeSymbol.AllInterfaces)
                    {
                    }
                }
            }

            var template = GetAutoRegisterExtensionsTemplate();
            var text = template.Render(new { Namespaces = namespaces, Services = services });

            context.AddSource("AutoRegisterExtensions.cs", SourceText.From(text, Encoding.UTF8));
        }

        private static Template GetAutoRegisterExtensionsTemplate()
        {
            return Template.Parse(@"using Microsoft.Extensions.DependencyInjection;
{{ for namespace in namespaces }}using {{ namespace }};{{ end }}

namespace CodeGeneratorDemo.SourceGeneratorDemo.Extensions
{
    public static class AutoRegisterExtensions
    {
        public static void AutoRegister(this IServiceCollection services)
        {
            {{ for service in services }}services.AddScoped<{{ service }}>();{{ end }}
        }
    }
}");
        }
    }
}
