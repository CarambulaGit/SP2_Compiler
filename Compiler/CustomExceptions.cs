using System;
using System.Runtime.Serialization;
using System.Text;

namespace Compiler {
    public class SyntaxException : Exception {
        public SyntaxException(string message, int row, int column) : base(
            $"{message} at row = {row}, column = {column}") { }

        public SyntaxException() : base() { }

        public SyntaxException(string message) : base(message) { }
    }

    public class CompilerException : Exception {
        public CompilerException() { }
        protected CompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public CompilerException(string? message) : base(message) { }
        public CompilerException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}