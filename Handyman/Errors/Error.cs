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
        public Error(string code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public string Code { get; private set; }

        public string Message { get; private set; }

        public override string ToString()
        {
            return $"{this.Code}: {this.Message}";
        }
    }
}
