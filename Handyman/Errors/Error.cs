using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handyman.Errors
{
    /// <summary>
    /// Describes an error.
    /// </summary>
    public class Error
    {
        public Error(ErrorCode code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public ErrorCode Code { get; private set; }

        public string Message { get; private set; }

        public override string ToString() => $"{this.Code}: {this.Message}";

        public enum ErrorCode
        {
            UnexpectedExecuteMethodImplementation,
            NotAType,
            NotARequestType,
            CannotResolveCommerceRuntimeReferenceDueToCompilationError,
            CannotResolveCommerceRuntimeReference,
            CannotCreateAnalysisContext,
        }
    }
}
