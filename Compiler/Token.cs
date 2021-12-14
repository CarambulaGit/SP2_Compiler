using System.Collections.Generic;

namespace Compiler {
    // todo move to Lexer
    
    // todo mb struct
    public class Token {
        //todo constructor
        // todo remove toStr?
        public override string ToString() {
            if (data != null) {
                return $"Kind is {Type.ToString()}\n" +
                       $"data is {data.ToString()}\n" +
                       $"row is {row.ToString()}\n" +
                       $"column is {column.ToString()}\n";
            }
            else {
                return $"Kind is {Type.ToString()}\n" +
                       $"row is {row.ToString()}\n" +
                       $"column is {column.ToString()}\n";
            }
        }

        public TokenType Type { get; set; }

        public dynamic data { get; set; }

        // todo class Position
        public int row { get; set; }

        public int column { get; set; }
    }
}

// int 		– 	int_keyword
// main 	– 	іdentifier
// (		– 	open parentheses
// ) 		– 	close parentheses
// {		– 	open brace
// return 	– 	return_keyword
// «2» 		– 	int_constant
// ‘ 		– 	open_quote
// ’ 		– 	close_quote
// f 		– 	character (можуть бути лише у форматі ASCII)
// «3.14» 	– 	float_constant
// ;		– 	semicolon
// } 		– 	close brace

public enum TokenType {
    FuncDefinition,
    Identifier,
    OpenBracket,
    CloseBracket,
    Colon,
    Newline,
    Indent,
    Return,
    Multiply,
    Divide,
    Dedent,
    WhileLoop,
    NotEqual,
    IfCondition,
    Greater,
    Assignment,
    Subtract,
    ElseCondition,
    Comma,
    PrintOperator,
    IntegerNumber,
    OP, // todo remove
}