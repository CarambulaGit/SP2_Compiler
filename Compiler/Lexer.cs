using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler {
    // todo lexer + parser + generator = static????
    public class Lexer {
        private readonly string _code;

        // todo to Consts?
        public static readonly List<char> Symbols = new List<char>() {
            '(', ')', '*', ',', '-', '/', ':', '=', '>', '!',
        };

        private List<Token> _tokens = new List<Token>();
        
        // todo rename levelVlojenosti?
        private int _currentLevel;

        public Lexer(string code) {
            _code = code;
            _currentLevel = 0;
        }

        public void Tokenize() {
            var strings = _code.Split(Environment.NewLine);
            for (var i = 0; i < strings.Length; i++) {
                if (ParseLine(strings[i], i)) {
                    _tokens.Add(new Token() {
                        Type = TokenType.Newline,
                        data = @"\n",
                        row = i,
                        column = strings[i].Length
                    });
                }
            }
        }

        private static int CountTabs(string str) {
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
            //todo rename
            var newCurrentLevel = CountTabs(line);
            if (newCurrentLevel - _currentLevel > 1) {
                throw new CompilerException($"Not expected indent at {row + 1}");
            }

            if (newCurrentLevel - _currentLevel == 1) {
                _currentLevel = newCurrentLevel;
                _tokens.Add(new Token() {
                    Type = TokenType.Indent,
                    data = "\t",
                    row = row,
                    column = 0
                });
            }
            else if (_currentLevel - newCurrentLevel > 0) {
                var difference = _currentLevel - newCurrentLevel;
                while (difference > 0) {
                    _tokens.Add(new Token() {
                        Type = TokenType.Dedent,
                        data = null,
                        row = row,
                        column = difference
                    });
                    difference--;
                }

                _currentLevel = newCurrentLevel;
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
                else if (Symbols.Contains(line[pos])) {
                    pos += StartsWithSym(line, row, pos);
                }
            }

            return true;
        }

        private int StartsWithDigit(string str, int row, int col) {
            var pos = col;
            var type = TokenType.IntegerNumber;
            var st = new StringBuilder(str.Length - col);
            while (pos < str.Length) {
                if (char.IsDigit(str[pos])) {
                    st.Append(str[pos]);
                }
                // else if (str[pos].Equals('.')) {
                // type = TokenKind.FLOAT;
                // st.Append(str[pos]);
                // }
                else {
                    break;
                }

                pos++;
            }

            
            // todo just write string?
            if (type == TokenType.IntegerNumber) {
                var res = 0;
                //Console.WriteLine(st.ToString());
                if (int.TryParse(st.ToString(), out res)) {
                    _tokens.Add(new Token() {
                        Type = TokenType.IntegerNumber,
                        data = res,
                        row = row,
                        column = col
                    });
                    return st.Length;
                }
            }
            // else if (type == TokenKind.FLOAT) {
            //     float res = 0;
            //     if (float.TryParse(st.ToString(), out res)) {
            //         // _tokens.Add(new Token()
            //         // {
            //         //     Kind = TokenKind.FLOAT,
            //         //     data = res,
            //         //     row = row,
            //         //     column = col
            //         // });
            //         _tokens.Add(new Token() {
            //             Kind = TokenKind.INT,
            //             data = Convert.ToInt32(res),
            //             row = row,
            //             column = col
            //         });
            //         return st.Length;
            //     }
            // }

            throw new CompilerException($"invalid syntax at {str} {row + 1}:{col}");
        }

        private int StartsWithLetter(string str, int row, int col) {
            var st = new StringBuilder(str.Length - col);
            var pos = col;
            while (pos < str.Length &&
                   (char.IsDigit(str[pos]) ||
                    char.IsLetter(str[pos]))) {
                st.Append(str[pos]);
                pos++;
            }

            switch (st.ToString()) {
                // todo make names consts 
                case "def":
                    _tokens.Add(new Token() {
                        Type = TokenType.FuncDefinition,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    break;
                case "while":
                    _tokens.Add(new Token() {
                        Type = TokenType.WhileLoop,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    break;
                case "print":
                    _tokens.Add(new Token() {
                        Type = TokenType.PrintOperator,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    break;
                case "return":
                    _tokens.Add(new Token() {
                        Type = TokenType.Return,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    break;
                case "if":
                    _tokens.Add(new Token() {
                        Type = TokenType.IfCondition,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    break;
                case "else":
                    _tokens.Add(new Token() {
                        Type = TokenType.ElseCondition,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    break;
                default:
                    _tokens.Add(new Token() {
                        Type = TokenType.Identifier,
                        data = st.ToString(),
                        row = row,
                        column = col
                    });
                    //Console.WriteLine(_tokens[^1].ToString());
                    break;
            }

            return st.ToString().Length;
        }

        private int StartsWithSym(string str, int row, int col) {
            var st = new StringBuilder(str.Length - col);
            var pos = col;
            while (pos < str.Length) {
                if (Symbols.Contains(str[pos])) {
                    st.Append(str[pos]);
                    pos++;
                }
                else {
                    break;
                }
            }

            if (st.Length > 0) {
                if (st.Length >= 2) {
                    if (LexerTwoChars(str[col], str[col + 1]) != TokenType.OP) {
                        _tokens.Add(new Token() {
                            Type = LexerTwoChars(str[col], str[col + 1]),
                            data = LexerTwoChars(str[col], str[col + 1]).ToString(),
                            row = row,
                            column = col
                        });
                        return 2;
                    }
                }

                if (LexerOneChar(str[col]) != TokenType.OP) {
                    _tokens.Add(new Token() {
                        Type = LexerOneChar(str[col]),
                        data = LexerOneChar(str[col]).ToString(),
                        row = row,
                        column = col
                    });
                    return 1;
                }
            }
            else {
                throw new CompilerException($"Unexpected token {str[col]} at {row + 1}:{col}");
            }

            return 0;
        }


        public TokenType LexerOneChar(int c1) {
            return c1 switch {
                '(' => TokenType.OpenBracket,
                ')' => TokenType.CloseBracket,
                '*' => TokenType.Multiply,
                ',' => TokenType.Comma,
                '-' => TokenType.Subtract,
                '/' => TokenType.Divide,
                ':' => TokenType.Colon,
                '=' => TokenType.Assignment,
                '>' => TokenType.Greater,
                _ => TokenType.OP
            };
        }

        public static TokenType LexerTwoChars(int c1, int c2) {
            switch (c1) {
                case '!':
                    switch (c2) {
                        case '=': return TokenType.NotEqual;
                    }

                    break;
            }

            return TokenType.OP;
        }

        public List<Token> GetTokens() {
            return _tokens;
        }

        public void PrintTokens() {
            foreach (var token in _tokens) {
                Console.WriteLine(token.ToString());
            }
        }
    }
}