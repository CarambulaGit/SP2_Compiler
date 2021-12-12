using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler {
    public class Lexer {
        private readonly string _code;

        public static readonly List<char> Symbols = new List<char>() {
            '(', ')', '*', ',', '-', '/', ':', '=', '>', '!',
        };

        private List<Token> _tokens = new List<Token>();

        private int _currentLevel;

        public Lexer(string code) {
            _code = code;
            _currentLevel = 0;
        }

        public void GetTokens() {
            var strings = _code.Split(Environment.NewLine);
            for (var i = 0; i < strings.Length; i++) {
                if (ParseLine(strings[i], i)) {
                    _tokens.Add(new Token() {
                        Kind = TokenKind.NEWLINE,
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
            if (line.Length != 0 &&
                line[0].Equals('#') ||
                line.All(s => s == '\t')) {
                return false;
            }

            var tabsC = CountTabs(line);
            if (tabsC - _currentLevel > 1) {
                throw new CompilerException($"Not expected indent at {row + 1}");
            }

            if (tabsC - _currentLevel == 1) {
                _currentLevel = tabsC;
                _tokens.Add(new Token() {
                    Kind = TokenKind.INDENT,
                    data = "\t",
                    row = row,
                    column = 0
                });
            }
            else if (_currentLevel - tabsC > 0) {
                for (var i = 0; i < _currentLevel - tabsC; i++) {
                    _tokens.Add(new Token() {
                        Kind = TokenKind.DEDENT,
                        data = null,
                        row = row - 1 // todo ????
                    });
                }

                _currentLevel = tabsC;
            }

            var pos = tabsC;

            while (pos < line.Length) {
                //Console.WriteLine(str[pos]);
                if (line[pos] == ' ') {
                    pos++;
                }
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
            var type = TokenKind.INT;
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

            if (type == TokenKind.INT) {
                int res = 0;
                //Console.WriteLine(st.ToString());
                if (int.TryParse(st.ToString(), out res)) {
                    _tokens.Add(new Token() {
                        Kind = TokenKind.INT,
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

            if (st.ToString().Equals("def")) {
                _tokens.Add(new Token() {
                    Kind = TokenKind.DEF,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }

            else if (st.ToString().Equals("while")) {
                _tokens.Add(new Token() {
                    Kind = TokenKind.WHILE,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }

            else if (st.ToString().Equals("print")) {
                _tokens.Add(new Token() {
                    Kind = TokenKind.PRINT,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("return")) {
                _tokens.Add(new Token() {
                    Kind = TokenKind.RETURN,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("if")) {
                _tokens.Add(new Token() {
                    Kind = TokenKind.IF,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("else")) {
                _tokens.Add(new Token() {
                    Kind = TokenKind.ELSE,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else {
                _tokens.Add(new Token() {
                    Kind = TokenKind.NAME,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
                //Console.WriteLine(_tokens[^1].ToString());
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
                    if (LexerTwoChars(str[col], str[col + 1]) != TokenKind.OP) {
                        _tokens.Add(new Token() {
                            Kind = LexerTwoChars(str[col], str[col + 1]),
                            data = LexerTwoChars(str[col], str[col + 1]).ToString(),
                            row = row,
                            column = col
                        });
                        return 2;
                    }
                }

                if (LexerOneChar(str[col]) != TokenKind.OP) {
                    _tokens.Add(new Token() {
                        Kind = LexerOneChar(str[col]),
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


        public TokenKind LexerOneChar(int c1) {
            return c1 switch {
                '(' => TokenKind.LPAR,
                ')' => TokenKind.RPAR,
                '*' => TokenKind.STAR,
                ',' => TokenKind.COMMA,
                '-' => TokenKind.MINUS,
                '/' => TokenKind.SLASH,
                ':' => TokenKind.COLON,
                '=' => TokenKind.EQUAL,
                '>' => TokenKind.GREATER,
                _ => TokenKind.OP
            };
        }

        public static TokenKind LexerTwoChars(int c1, int c2) {
            switch (c1) {
                case '!':
                    switch (c2) {
                        case '=': return TokenKind.NOTEQUAL;
                    }

                    break;
            }

            return TokenKind.OP;
        }

        public List<Token> GetTokensList() {
            return _tokens;
        }

        public void PrintTokens() {
            foreach (var token in _tokens) {
                Console.WriteLine(token.ToString());
            }
        }
    }
}