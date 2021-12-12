using System;
using System.Runtime.Serialization;
using System.Text;

namespace Compiler {
    public class CompilerException : Exception {
        public CompilerException() { }
        protected CompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public CompilerException(string? message) : base(message) { }
        public CompilerException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}