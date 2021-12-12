using System.Collections.Generic;

namespace Compiler {
    public class Token {
        public override string ToString() {
            if (data != null) {
                return $"Kind is {Kind.ToString()}\n" +
                       $"data is {data.ToString()}\n" +
                       $"row is {row.ToString()}\n" +
                       $"column is {column.ToString()}\n";
            }
            else {
                return $"Kind is {Kind.ToString()}\n" +
                       $"row is {row.ToString()}\n" +
                       $"column is {column.ToString()}\n";
            }
        }

        public TokenKind Kind { get; set; }

        public dynamic data { get; set; }

        public int row { get; set; }

        public int column { get; set; }
    }
}

public enum TokenKind {
    NEWLINE,
    DEF,
    NAME,
    LPAR,
    RPAR,
    COLON,
    RETURN,
    STAR,
    SLASH,
    WHILE,
    NOTEQUAL,
    IF,
    GREATER,
    EQUAL,
    MINUS,
    ELSE,
    COMMA,
    PRINT,
    INT,

    INDENT,
    DEDENT,
    OP,

    // LESS,
    // PLUS,
}