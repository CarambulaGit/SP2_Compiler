using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler {
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

            foreach (var chr in str) {
                if (chr == ' ') {
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
                    throw new LexerException($"Not expected indent at {row + 1}");
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
                switch (line[pos]) {
                    case ' ':
                        pos++;
                        break;
                    case '#':
                        return true;
                    default: {
                        if (char.IsDigit(line[pos])) {
                            pos += StartsWithDigit(line, row, pos);
                        }
                        else if (char.IsLetter(line[pos])) {
                            pos += StartsWithLetter(line, row, pos);
                        }
                        else if (Constants.SYMBOLS.Contains(line[pos])) {
                            pos += StartsWithSym(line, row, pos);
                        }

                        break;
                    }
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

            if (!int.TryParse(st.ToString(), out _))
                throw new LexerException($"invalid syntax at {str} {row + 1}:{col}");
            Tokens.Add(new Token(TokenType.IntegerNumber, st.ToString(), row, col));
            return st.Length;
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

        private int StartsWithLetter(string str, int row, int column) {
            var st = new StringBuilder(str.Length - column);
            var pos = column;
            while (pos < str.Length &&
                   (char.IsDigit(str[pos]) ||
                    char.IsLetter(str[pos]))) {
                st.Append(str[pos]);
                pos++;
            }

            if (st.ToString() == Constants.PYTHON_FUNCTION_DEFENITION)
                Tokens.Add(new Token(TokenType.FuncDefinition, st.ToString(), row, column));
            else if (st.ToString() == Constants.PYTHON_WHILE)
                Tokens.Add(new Token(TokenType.WhileLoop, st.ToString(), row, column));
            else if (st.ToString() == Constants.PYTHON_PRINT)
                Tokens.Add(new Token(TokenType.PrintOperator, st.ToString(), row, column));
            else if (st.ToString() == Constants.PYTHON_RETURN)
                Tokens.Add(new Token(TokenType.Return, st.ToString(), row, column));
            else if (st.ToString() == Constants.PYTHON_IF)
                Tokens.Add(new Token(TokenType.IfCondition, st.ToString(), row, column));
            else if (st.ToString() == Constants.PYTHON_ELSE)
                Tokens.Add(new Token(TokenType.ElseCondition, st.ToString(), row, column));
            else
                Tokens.Add(new Token(TokenType.Identifier, st.ToString(), row, column));

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

            throw new LexerException($"Unexpected token {str[col]} at {row + 1}:{col}");
        }

        private TokenType LexerTwoChars(int symb1, int symb2) {
            if (symb1.Equals('!') && symb2.Equals('=')) {
                return TokenType.NotEqual;
            }

            return TokenType.NotImplemented;
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
    }
}