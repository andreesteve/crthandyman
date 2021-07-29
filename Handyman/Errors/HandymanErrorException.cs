using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handyman.Errors
{
    /// <summary>
    /// Represents an error when performing an operation.
    /// </summary>
    public class HandymanErrorException : Exception
    {
        public HandymanErrorException(Error error, Exception innerException = null)
            : base(error.Message, innerException)
        {
            this.Error = error;
        }

        public Error Error { get; private set; }

        public override string ToString()
        {
            return this.Error.ToString();
        }
    }
}
