using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler {
    // todo lexer + parser + generator = static????
    public class Lexer {
        private readonly string _code;

        public List<Token> Tokens { get; private set; } = new List<Token>();

        private int _currentLevel;

        public Lexer(string code) {
            _code = code;
            _currentLevel = 0;
        }

        public void Tokenize() {
            var strings = _code.Split(Environment.NewLine);
            for (var i = 0; i < strings.Length; i++) {
                if (ParseLine(strings[i], i)) {
                    Tokens.Add(new Token(TokenType.Newline, @"\n", i, strings[i].Length));
                }
            }
        }

        private static int CountLevel(string str) {
            var spaces = 0;

            for (var i = 0; i < str.Length; i++) {
                if (str[i] == ' ') {
                    spaces++;
                }
                else {
                    break;
                }
            }

            return spaces / 4;
        }

        private bool ParseLine(string line, int row) {
            // todo out result
            // todo method find indent/dedent
            // todo extension string.WithOut, fix comment finding
            if (line.Length != 0 &&
                line[0].Equals('#') ||
                line.All(s => s == '\t')) {
                return false;
            }

            var newCurrentLevel = CountLevel(line);
            switch (newCurrentLevel - _currentLevel) {
                case > 1:
                    throw new CompilerException($"Not expected indent at {row + 1}");
                case 1:
                    _currentLevel = newCurrentLevel;
                    Tokens.Add(new Token(TokenType.Indent, "\t", row, 0));
                    break;
                default: {
                    if (_currentLevel - newCurrentLevel > 0) {
                        var difference = _currentLevel - newCurrentLevel;
                        while (difference > 0) {
                            difference--;
                            Tokens.Add(new Token(TokenType.Dedent, null, row, difference));
                        }

                        _currentLevel = newCurrentLevel;
                    }

                    break;
                }
            }

            var pos = newCurrentLevel;

            while (pos < line.Length) {
                //Console.WriteLine(str[pos]);
                if (line[pos] == ' ') {
                    pos++;
                }
                // todo remove when change comment detection
                else if (line[pos] == '#') {
                    return true;
                }
                else if (char.IsDigit(line[pos])) {
                    pos += StartsWithDigit(line, row, pos);
                }
                else if (char.IsLetter(line[pos])) {
                    pos += StartsWithLetter(line, row, pos);
                }
                else if (Constants.SYMBOLS.Contains(line[pos])) {
                    pos += StartsWithSym(line, row, pos);
                }
            }

            return true;
        }

        private int StartsWithDigit(string str, int row, int col) {
            var pos = col;
            var st = new StringBuilder(str.Length - col);
            while (pos < str.Length) {
                if (char.IsDigit(str[pos])) st.Append(str[pos]);
                else break;
                pos++;
            }


            // todo just write string?
            if (!int.TryParse(st.ToString(), out _))
                throw new CompilerException($"invalid syntax at {str} {row + 1}:{col}");
            Tokens.Add(new Token(TokenType.IntegerNumber, st.ToString(), row, col));
            return st.Length;
        }

        private int StartsWithLetter(string str, int row, int column) {
            var st = new StringBuilder(str.Length - column);
            var pos = column;
            while (pos < str.Length &&
                   (char.IsDigit(str[pos]) ||
                    char.IsLetter(str[pos]))) {
                st.Append(str[pos]);
                pos++;
            }

            switch (st.ToString()) {
                // todo make names consts 
                case "def":
                    Tokens.Add(new Token(TokenType.FuncDefinition, st.ToString(), row, column));
                    break;
                case "while":
                    Tokens.Add(new Token(TokenType.WhileLoop, st.ToString(), row, column));
                    break;
                case "print":
                    Tokens.Add(new Token(TokenType.PrintOperator, st.ToString(), row, column));
                    break;
                case "return":
                    Tokens.Add(new Token(TokenType.Return, st.ToString(), row, column));
                    break;
                case "if":
                    Tokens.Add(new Token(TokenType.IfCondition, st.ToString(), row, column));
                    break;
                case "else":
                    Tokens.Add(new Token(TokenType.ElseCondition, st.ToString(), row, column));
                    break;
                default:
                    Tokens.Add(new Token(TokenType.Identifier, st.ToString(), row, column));
                    break;
            }

            return st.ToString().Length;
        }

        private int StartsWithSym(string str, int row, int col) {
            var st = new StringBuilder(str.Length - col);
            var pos = col;
            while (pos < str.Length) {
                if (Constants.SYMBOLS.Contains(str[pos])) {
                    st.Append(str[pos]);
                    pos++;
                }
                else {
                    break;
                }
            }

            if (st.Length > 0) {
                return LexerChars(str, row, col, st);
            }

            throw new CompilerException($"Unexpected token {str[col]} at {row + 1}:{col}");
        }

        private int LexerChars(string str, int row, int col, StringBuilder st) {
            if (st.Length >= 2) {
                if (LexerTwoChars(str[col], str[col + 1]) != TokenType.NotImplemented) {
                    Tokens.Add(new Token(LexerTwoChars(str[col], str[col + 1]),
                        LexerTwoChars(str[col], str[col + 1]).ToString(), row, col));
                    return 2;
                }
            }

            if (LexerSingleChar(str[col]) == TokenType.NotImplemented) return 0;
            Tokens.Add(new Token(LexerSingleChar(str[col]), LexerSingleChar(str[col]).ToString(), row, col));
            return 1;
        }


        private TokenType LexerSingleChar(int symb) {
            return symb switch {
                '(' => TokenType.OpenBracket,
                ')' => TokenType.CloseBracket,
                '*' => TokenType.Multiply,
                ',' => TokenType.Comma,
                '-' => TokenType.Subtract,
                '/' => TokenType.Divide,
                ':' => TokenType.Colon,
                '=' => TokenType.Assignment,
                '>' => TokenType.Greater,
                _ => TokenType.NotImplemented
            };
        }

        private TokenType LexerTwoChars(int symb1, int symb2) {
            if (symb1.Equals('!') && symb2.Equals('=')) {
                return TokenType.NotEqual;
            }

            return TokenType.NotImplemented;
        }


        public void PrintTokens() {
            foreach (var token in Tokens) {
                Console.WriteLine(token.ToString());
            }
        }
    }
}