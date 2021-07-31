using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Handyman.Types
{
    /// <summary>
    /// Represents the location of a type.
    /// </summary>
    public sealed class TypeLocation<TContainer>
    {
        /// <summary>
        /// Gets the type that logically contains this location.
        /// </summary>
        /// <value></value>
        public TContainer ContainingType { get; set; }

        /// <summary>
        /// The type that is being located.
        /// </summary>
        public ITypeSymbol TypeSymbol { get; set; }

        /// <summary>
        /// The location of the type.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// The syntax node associated with this location.
        /// </summary>
        public SyntaxNode SyntaxNode { get; set; }

        public override string ToString()
        {
            return $"{System.IO.Path.GetFileName(this.Location.SourceTree.FilePath)}:{this.Location.GetMappedLineSpan().StartLinePosition.Line}";
        }
    }
}
