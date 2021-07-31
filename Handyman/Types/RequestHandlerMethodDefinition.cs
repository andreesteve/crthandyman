using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Handyman.DocumentAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Handyman.Types
{
    /// <summary>
    /// Represents the implementation of method that handles a request/response pair.
    /// </summary>
    public class RequestHandlerMethodDefinition
    {
        private readonly IMethodSymbol requestHandlerMethod;

        public RequestHandlerMethodDefinition(RequestType requestType, ResponseType responseType, IMethodSymbol requestHandlerMethod)
        {
            this.RequestType = requestType;
            this.ResponseType = responseType;
            this.requestHandlerMethod = requestHandlerMethod;
        }

        public RequestType RequestType { get; private set; }

        public ResponseType ResponseType { get; private set; }

        /// <summary>
        /// Given any method, tries to extract method parameters as request definition and return type as response definition.
        /// </summary>
        /// <param name="method">The method symbol to be used.</param>
        /// <param name="reference">The runtime reference.</param>
        /// <returns>A way to represent the method with request/response entires.</returns>
        public static RequestHandlerMethodDefinition TryGenerateHandlerMethodDefinitionFromMethod(IMethodSymbol method, CommerceRuntimeReference reference)
        {
            // method needs to belong to class
            if (method.ContainingSymbol != null)
            {
                string methodDoc = method.GetDocumentationCommentXml();
                var doc = new DocumentationAnalyzer(methodDoc);

                // REQUEST
                var requetMembers = method.Parameters
                    .Where(p => p.RefKind == RefKind.None)
                    .Select(p => CreateMemberFromParameter(p, doc));

                // HACY: method is not the right type
                var requestType = new RequestType(null, requetMembers, doc.Summary, reference.RequestBaseClassFqn);
                requestType.Name = method.Name + "Request";

                // RESPONSE
                var responseMembers = method.Parameters
                    .Where(p => p.RefKind == RefKind.Out)
                    .Select(p => CreateMemberFromParameter(p, doc));

                if (!method.ReturnsVoid)
                {
                    // keep return parameter first, to avoid changing order of parameters when out parameters are added later
                    responseMembers = new[] { CreateMemberFromReturnType(method.ReturnType, doc) }.Concat(responseMembers);
                }

                string responseDocumentation = $"The response for <see cref=\"{{{requestType.Name}}}\" />.";

                ResponseType responseType;
                if (responseMembers.Any())
                {
                    responseType = new ResponseType(null, responseMembers, responseDocumentation, reference.ResponseBaseClassFqn);
                    responseType.Name = method.Name + "Response";
                }
                else
                {
                    responseType = reference.VoidResponse;
                }

                return new RequestHandlerMethodDefinition(requestType, responseType, method);
            }

            return null;
        }

        public void SetContainingNamespace(string containingNamespace)
        {
            if (string.IsNullOrWhiteSpace(containingNamespace))
            {
                throw new ArgumentNullException(nameof(containingNamespace));
            }

            this.RequestType.Namespace = containingNamespace;
            this.ResponseType.Namespace = containingNamespace;
        }

        private static ITypeSymbol UnwrapTaskType(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol)
            {
                if (type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ToString().StartsWith("System.Threading.Tasks.Task"))
                {
                    return namedType.TypeArguments.First();
                }
            }

            return type;
        }

        private static Member CreateMemberFromParameter(IParameterSymbol parameter, DocumentationAnalyzer doc)
        {
            return new Member(parameter.Name, UnwrapTaskType(parameter.Type), doc.GetParameter(parameter.Name));
        }

        private static Member CreateMemberFromReturnType(ITypeSymbol type, DocumentationAnalyzer doc)
        {
            type = UnwrapTaskType(type);
            string name = type.Name;

            if (IsPrimitive(type))
            {
                name = "Result";
            }

            return new Member(name, type, doc.Returns);
        }

        private static bool IsPrimitive(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Char:
                case SpecialType.System_String:
                case SpecialType.System_Object:
                    return true;
            }

            return false;
        }
    }
}
