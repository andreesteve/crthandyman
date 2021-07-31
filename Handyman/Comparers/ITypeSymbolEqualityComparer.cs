using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Handyman.DocumentAnalyzers;
using Microsoft.CodeAnalysis;

namespace Handyman.Comparers
{
    /// <summary>
    /// <see cref="IEqualityComparer{T}"/> for <see cref="ITypeSymbol"/>.
    /// </summary>
    public sealed class ITypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol>
    {
        public static readonly ITypeSymbolEqualityComparer Default = new ITypeSymbolEqualityComparer();

        public bool Equals(ITypeSymbol x, ITypeSymbol y)
        {
            return x.Equals(y, SymbolEqualityComparer.Default) || x.GetFullyQualifiedName().Equals(y.GetFullyQualifiedName());
        }

        public int GetHashCode(ITypeSymbol obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
