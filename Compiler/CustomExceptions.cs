using System;
using System.Runtime.Serialization;
using System.Text;

namespace Compiler {
    // todo ParserException
    public class SyntaxException : Exception {
        public SyntaxException(string message, int row, int column) : base(
            $"{message} at row = {row}, column = {column}") { }

        public SyntaxException() : base() { }

        public SyntaxException(string message) : base(message) { }
    }

    // todo LexerException
    public class CompilerException : Exception {
        public CompilerException() { }
        protected CompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public CompilerException(string? message) : base(message) { }
        public CompilerException(string? message, Exception? innerException) : base(message, innerException) { }
    }
    
    // todo AsmGeneratorException
}