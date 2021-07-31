using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Handyman.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Handyman.Types
{
    /// <summary>
    /// Represents the a request type.
    /// </summary>
    public class RequestType : MemberedBaseType
    {
        public RequestType(ITypeSymbol declaringType, IEnumerable<Member> members, string documentation, string baseClassFQN)
            // FIXME declaring type is mandatory
            : base(declaringType?.Name, baseClassFQN, members, documentation)
        {
            // TODO add null check
            this.DeclaringType = declaringType;
        }

        public ITypeSymbol DeclaringType { get; internal set; }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is RequestType request
                && (
                    obj == this
                    || this.DeclaringType.Equals(request.DeclaringType, SymbolEqualityComparer.Default)
                );
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.DeclaringType.GetHashCode();
        }
    }
}