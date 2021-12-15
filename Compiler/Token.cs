﻿using System.Collections.Generic;

namespace Compiler {
    // todo mb struct
    public class Token {
        public TokenType Type { get; set; }

        public string Data { get; set; }

        // todo class Position
        public int Row { get; set; }

        public int Column { get; set; }

        public Token(TokenType type, string data, int row, int column) {
            Type = type;
            Data = data;
            Row = row;
            Column = column;
        }
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
    NotImplemented, // todo remove
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
}