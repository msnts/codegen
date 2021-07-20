using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CodeGen.Extensions
{
    public static class ITypeSymbolExtensions
    {
        public static string GetFullName(this ITypeSymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            var nss = new List<string>();
            while (ns != null)
            {
                if (string.IsNullOrWhiteSpace(ns.Name))
                    break;
                nss.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }
            nss.Reverse();
            if (nss.Any())
                return $"{string.Join(".", nss)}.{symbol.Name}";
            return string.Empty;
        }

        public static bool TryAttributeData(this ITypeSymbol symbol, ISymbol attributeSymbol, out AttributeData data)
        {
            data = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

            return data != null;
        }
    }
}
