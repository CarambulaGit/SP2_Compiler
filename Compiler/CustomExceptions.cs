using System;

namespace Compiler {
    public class ParserException : Exception {
        public ParserException(string message, int row, int column) : base(
            $"{message} at row = {row}, column = {column}") { }

        public ParserException() : base() { }

        public ParserException(string message) : base(message) { }
    }

    public class LexerException : Exception {
        public LexerException(string? message) : base(message) { }
    }

    public class AsmGeneratorException : Exception {
        public AsmGeneratorException(string? message) : base(message) { }
    }
}