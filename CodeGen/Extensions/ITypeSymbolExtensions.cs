using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CodeGen.Extensions
{
    public static class ITypeSymbolExtensions
    {
        public static string GetFullNamespace(this ITypeSymbol symbol)
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
                return $"{string.Join(".", nss)}";
            return string.Empty;
        }
    }
}
