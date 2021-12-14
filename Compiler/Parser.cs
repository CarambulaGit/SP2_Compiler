using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Compiler {
    public class Parser {
        private Dictionary<string, AstNode> _defAst;

        private readonly List<Token> _tokens;

        private readonly TwoWayEnum<Token> _enumerator;

        private readonly AstTree _base;

        private WhileLoopStatement? _currentLoop = null;

        private INamespace _currentNameSpace;

        public Parser(List<Token> tokens) {
            _defAst = new Dictionary<string, AstNode>();

            _tokens = tokens;

            _base = new AstTree();

            _enumerator = new TwoWayEnum<Token>(_tokens.GetEnumerator());

            _currentNameSpace = _base;
        }

        public void Parse() {
            ParseUntil(_base.Root);
        }

        private void ParseUntil(RootNode baseNode, TokenType? stopToken = null) {
            while (_enumerator.MoveNext()) {
                var token = _enumerator.Current;
                if (token.Type == stopToken) {
                    break;
                }

                switch (token.Type) {
                    case TokenType.FuncDefinition: {
                        DefStatement temp;
                        switch (baseNode) {
                            case INamespace tableContainer:
                                temp = ParseDef(new Dictionary<string, int>(tableContainer.varTable));
                                break;
                            default:
                                temp = ParseDef(new Dictionary<string, int>(_base.varTable));
                                break;
                        }

                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenType.Identifier: {
                        if (_enumerator.MoveNext()) {
                            if (_enumerator.Current.Type == TokenType.Assignment) {
                                _enumerator.MovePrevious();
                                var name = _enumerator.Current.data;
                                _enumerator.MoveNext();
                                _enumerator.MoveNext();
                                var expr = ParseExpr();
                                baseNode.AddChild(new AssignStatement(
                                    _enumerator.Current.row,
                                    _enumerator.Current.column,
                                    name, expr));
                                switch (baseNode) {
                                    case INamespace tableContainer: {
                                        tableContainer.AddVar(name);
                                        break;
                                    }
                                    default: {
                                        _currentNameSpace.AddVar(name);
                                        break;
                                    }
                                }
                            }
                            else if (_enumerator.Current.Type == TokenType.OpenBracket) {
                                _enumerator.MovePrevious();
                                var tempEx = ParseExpr();
                                var temp = new ExprStatement(tempEx.Row, tempEx.Column, tempEx);
                                baseNode.AddChild(temp);
                                this.MatchIndentation();
                                break;
                            }
                            else {
                                _enumerator.MovePrevious();
                                baseNode.AddChild(new ExprStatement(
                                    _enumerator.Current.row,
                                    _enumerator.Current.column,
                                    ParseExpr()));
                                ;
                            }
                        }

                        break;
                    }
                    case TokenType.IfCondition: {
                        var temp = ParseConditional();
                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenType.IntegerNumber:
                    case TokenType.Subtract:
                    case TokenType.OpenBracket: {
                        var temp = new ExprStatement(_enumerator.Current.row,
                            _enumerator.Current.column,
                            ParseExpr());
                        //Console.WriteLine(temp.ToString());
                        baseNode.AddChild(temp);
                        MatchIndentationCurrent();
                        break;
                    }
                    case TokenType.WhileLoop: {
                        var temp = ParseWhileLoop();
                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenType.PrintOperator: {
                        var temp = new PrintStatement(_enumerator.Current.row, _enumerator.Current.column);
                        Match(TokenType.OpenBracket);
                        temp.expr = ParseExpr();
                        _enumerator.MovePrevious();
                        MatchCurrent(TokenType.CloseBracket);
                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenType.Return: {
                        if (_currentNameSpace.GetType() != typeof(DefStatement)) {
                            throw new SyntaxException($"Return outside of function at {_enumerator.Current.row}:" +
                                                      $"{_enumerator.Current.column}",
                                _enumerator.Current.row,
                                _enumerator.Current.column);
                        }

                        var t = _enumerator.Current;
                        _enumerator.MovePrevious();
                        var currentNameSpace = ((DefStatement) _currentNameSpace);
                        currentNameSpace.Return = MatchReturn();
                        baseNode.AddChild(new ReturnStatement(t.row, t.column, currentNameSpace.Return));
                        break;
                    }
                    default: {
                        break;
                    }
                }
            }
        }


        private DefStatement ParseDef(Dictionary<string, int> varTable) {
            //Console.WriteLine(string.Join(", ", varTable.Keys));
            var def = new DefStatement(_enumerator.Current.row, _enumerator.Current.column, varTable) {
                Name = this.Match(TokenType.Identifier).data,
                Args = this.MatchDefArgs()
            };
            def.FuncList = new List<DefStatement>(_currentNameSpace.FuncList);
            _currentNameSpace.FuncList.Add(def);
            //def.Args.Reverse();
            foreach (var arg in def.Args) {
                if (def.varTable.Keys.Contains(arg)) {
                    def.varTable.Remove(arg);
                }

                //def.AddVar(arg);
                def.AddArg(arg);
            }

            this.Match(TokenType.Colon);

            if (MatchBool(TokenType.Newline)) {
                var prevNameSpace = _currentNameSpace;
                _currentNameSpace = def;
                ParseUntil(def, TokenType.Dedent);
                _currentNameSpace = prevNameSpace;
            }
            else {
                this.MatchCurrent(TokenType.Return);
                def.Return = ParseExpr();
                MatchCurrent(TokenType.Newline);
            }

            return def;
        }

        private Statement ParseWhileLoop() {
            var token = _enumerator.Current;
            _enumerator.MoveNext();
            var ret = new WhileLoopStatement(token.row,
                token.column, ParseExpr());
            _currentLoop = ret;
            //_enumerator.MovePrevious();
            MatchCurrent(TokenType.Colon);
            if (!_enumerator.MoveNext()) {
                _enumerator.MovePrevious();
                throw new CompilerException(
                    $"Expected token {_enumerator.Current} at {_enumerator.Current.row}:{_enumerator.Current.column}");
            }

            var body = new BlockStatement(_enumerator.Current.row, _enumerator.Current.column);
            if (MatchCurrentBool(TokenType.Newline)) {
                Match(TokenType.Indent);
                _enumerator.MovePrevious();
                ParseUntil(body, TokenType.Dedent);
            }
            else {
                ParseUntil(body, TokenType.Newline);
            }

            ret.AddChild(body);
            _currentLoop = null;
            return ret;
        }

        private Statement ParseConditional() {
            var rowCol = new {
                _enumerator.Current.row,
                _enumerator.Current.column
            };
            if (!_enumerator.MoveNext()) {
                throw new SyntaxException("Token expected",
                    rowCol.row, rowCol.column);
            }

            var condition = ParseExpr();
            MatchCurrent(TokenType.Colon);

            var body = new BlockStatement(_enumerator.Current.row,
                _enumerator.Current.column);

            if (!_enumerator.MoveNext())
                throw new SyntaxException("Token expected",
                    _enumerator.Current.row, _enumerator.Current.column);

            Match(TokenType.Indent);
            _enumerator.MovePrevious();
            ParseUntil(body,
                _enumerator.Current.Type == TokenType.Newline ? TokenType.Dedent : TokenType.Newline);

            if (MatchBool(TokenType.ElseCondition)) {
                var conditionalElseStatement = new ConditionalElseStatement(rowCol.row,
                    rowCol.column,
                    condition
                );
                var elseBody = new BlockStatement(_enumerator.Current.row,
                    _enumerator.Current.column);
                if (!_enumerator.MoveNext())
                    throw new SyntaxException("Token expected",
                        _enumerator.Current.row, _enumerator.Current.column);
                _enumerator.MoveNext();
                Match(TokenType.Indent);
                _enumerator.MovePrevious();
                ParseUntil(elseBody,
                    _enumerator.Current.Type == TokenType.Newline ? TokenType.Dedent : TokenType.Newline);
                conditionalElseStatement.AddChild(body);
                conditionalElseStatement.AddChild(elseBody);
                return conditionalElseStatement;
            }

            var conditionalStatement = new ConditionalStatement(rowCol.row,
                rowCol.column,
                condition
            );
            conditionalStatement.AddChild(body);

            return conditionalStatement;
        }

        private Token Match(TokenType l) {
            if (_enumerator.MoveNext()) {
                //Console.WriteLine(_enumerator.Current.Kind.ToString());
                if (l != _enumerator.Current.Type) {
                    //Console.WriteLine(_enumerator.Current.Kind.ToString());
                    throw new SyntaxException("Got " + _enumerator.Current.Type.ToString() +
                                              $", {l.ToString()} expected" +
                                              $" at {_enumerator.Current.row + 1}:{_enumerator.Current.column + 1}",
                        _enumerator.Current.row, _enumerator.Current.column);
                }

                //Console.WriteLine(_enumerator.Current.ToString());
                return _enumerator.Current;
            }

            throw new SyntaxException();
        }

        private Token MatchCurrent(TokenType l) {
            if (l != _enumerator.Current.Type) {
                throw new SyntaxException("Got " + _enumerator.Current.Type.ToString() + $", {l.ToString()} expected" +
                                          $" at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                    _enumerator.Current.row, _enumerator.Current.column);
            }

            //Console.WriteLine(_enumerator.Current.ToString());
            return _enumerator.Current;
        }

        private bool MatchBool(TokenType l) {
            if (_enumerator.MoveNext()) {
                return l == _enumerator.Current.Type;
            }

            return false;
        }

        private bool MatchCurrentBool(TokenType l) {
            return l == _enumerator.Current.Type;
        }

        private void MatchIndentation() {
            if (_enumerator.MoveNext()) {
                if (!MatchCurrentBool(TokenType.Newline) &&
                    !MatchCurrentBool(TokenType.Dedent)) {
                    Console.WriteLine(_enumerator.Current.ToString());
                    throw new SyntaxException("Expected new line or semicolon");
                }
            }
        }

        private void MatchIndentationCurrent() {
            if (!MatchCurrentBool(TokenType.Newline) &&
                !MatchCurrentBool(TokenType.Dedent)) {
                Console.WriteLine(_enumerator.Current.ToString());
                throw new SyntaxException("Expected new line or semicolon");
            }
        }

        private Token MatchConst() {
            if (_enumerator.MoveNext()) {
                if (_enumerator.Current.Type != TokenType.IntegerNumber) {
                    throw new SyntaxException();
                }

                //Console.WriteLine(_enumerator.Current.ToString());
                return _enumerator.Current;
            }

            throw new SyntaxException();
        }

        private Expression MatchReturn() {
            this.Match(TokenType.Return);
            var ErrRow = _enumerator.Current.row;
            var ErrCol = _enumerator.Current.column;
            if (_enumerator.MoveNext()) {
                var returnExpr = this.ParseExpr();
                return returnExpr;
            }

            throw new SyntaxException($"Expected token",
                ErrRow, ErrCol);
        }

        private List<Expression> MatchArgs() {
            var res = new List<Expression>();
            this.Match(TokenType.OpenBracket);
            while (_enumerator.MoveNext()) {
                switch (_enumerator.Current.Type) {
                    case TokenType.Identifier:
                    case TokenType.IntegerNumber:
                        res.Add(ParseExpr());
                        //_enumerator.MoveNext();
                        switch (_enumerator.Current.Type) {
                            case TokenType.Comma:
                                break;
                            case TokenType.CloseBracket:
                                return res;
                            default:
                                throw new SyntaxException(
                                    $"Unexpected token {_enumerator.Current.Type.ToString()} at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                                    _enumerator.Current.row, _enumerator.Current.column
                                );
                        }

                        break;
                    case TokenType.CloseBracket:
                        return res;
                    default:
                        throw new SyntaxException(
                            $"Unexpected token {_enumerator.Current.Type.ToString()} at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                            _enumerator.Current.row, _enumerator.Current.column
                        );
                }
            }

            return res;
        }

        private List<string> MatchDefArgs() {
            var res = new List<string>();
            this.Match(TokenType.OpenBracket);
            while (_enumerator.MoveNext()) {
                switch (_enumerator.Current.Type) {
                    case TokenType.Identifier:
                        res.Add(_enumerator.Current.data);
                        //Console.WriteLine(res[^1].ToString());
                        _enumerator.MoveNext();
                        switch (_enumerator.Current.Type) {
                            case TokenType.Comma:
                                break;
                            case TokenType.CloseBracket:
                                return res;
                            default:
                                throw new SyntaxException(
                                    $"Unexpected token at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                                    _enumerator.Current.row, _enumerator.Current.column
                                );
                        }

                        break;
                    case TokenType.CloseBracket:
                        return res;
                    default:
                        throw new SyntaxException(
                            $"Unexpected token at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                            _enumerator.Current.row, _enumerator.Current.column
                        );
                }
            }

            return res;
        }

        private Expression ParseExpr() {
            //Console.WriteLine(_enumerator.Current.Kind.ToString() + "Expr");
            var first = ParseTerm();
            while (MatchCurrentBool(TokenType.Subtract)) {
                var op = _enumerator.Current.Type;
                var ErrRow = _enumerator.Current.row;
                var ErrCol = _enumerator.Current.column;
                if (_enumerator.MoveNext()) {
                    var second = ParseTerm();
                    first = new BinaryOperationExpression(first.Row,
                        first.Column,
                        op,
                        first,
                        second
                    );
                }
            }

            if (MatchCurrentBool(TokenType.IfCondition) && _enumerator.MoveNext()) {
                var condition = ParseExpr();

                MatchCurrent(TokenType.ElseCondition);
                _enumerator.MoveNext();
                var elseExpression = ParseExpr();
                return new ConditionalExpression(first.Row,
                    first.Column,
                    first,
                    condition,
                    elseExpression);
            }

            if (MatchCurrentBool(TokenType.Greater)) {
                var op = _enumerator.Current.Type;
                if (_enumerator.MoveNext()) {
                    var third = ParseExpr();
                    first = new BinaryOperationExpression(first.Row,
                        first.Column,
                        op,
                        first,
                        third
                    );
                }
            }

            //first.PrintOp(0);

            return first;
        }

        private Expression ParseTerm() {
            //Console.WriteLine(_enumerator.Current.Kind.ToString() + "Term");
            var first = ParseFactor();
            while (_enumerator.MoveNext() &&
                   (MatchCurrentBool(TokenType.Multiply) ||
                    MatchCurrentBool(TokenType.Divide) ||
                    MatchCurrentBool(TokenType.NotEqual)/* ||
                    MatchCurrentBool(TokenKind.MINUS)*/)) {
                var op = _enumerator.Current.Type;
                var errRow = _enumerator.Current.row;
                var errCol = _enumerator.Current.column;
                if (_enumerator.MoveNext()) {
                    var second = ParseFactor();
                    first = new BinaryOperationExpression(first.Row,
                        first.Column,
                        op,
                        first,
                        second
                    );
                }
                else {
                    throw new SyntaxException($"Expected token", errRow, errCol);
                }
            }

            //Console.WriteLine(first.GetType().ToString());

            return first;
        }

        private Expression ParseFactor() {
            //Console.WriteLine(_enumerator.Current.Kind.ToString() + " Factor");
            if (MatchCurrentBool(TokenType.OpenBracket)) {
                if (_enumerator.MoveNext()) {
                    var expr = ParseExpr();
                    MatchCurrent(TokenType.CloseBracket);
                    return expr;
                }
            }

            // if (MatchCurrentBool(TokenKind.MINUS)) {
            //     var row = _enumerator.Current.row;
            //     var col = _enumerator.Current.column;
            //     var op = _enumerator.Current.Kind;
            //     if (_enumerator.MoveNext()) {
            //         return new UnOp(row,
            //             col,
            //             op,
            //             ParseFactor());
            //     }
            //
            //     throw new SyntaxException($"Expected token", row, col);
            // }

            if (MatchCurrentBool(TokenType.IntegerNumber)) {
                return new ConstExpression(_enumerator.Current.row,
                    _enumerator.Current.column,
                    _enumerator.Current.data);
            }

            if (MatchCurrentBool(TokenType.Identifier)) {
                var name = _enumerator.Current.data;
                if (_enumerator.MoveNext() &&
                    _enumerator.Current.Type == TokenType.OpenBracket) {
                    _enumerator.MovePrevious();
                    var ret = new CallExpression(_enumerator.Current.row,
                        _enumerator.Current.column,
                        name, MatchArgs());
                    if (!_currentNameSpace.ThereIsFuncWithName(ret.name)) {
                        throw new SyntaxException($"Name {ret.name} is not defined ", ret.Row, ret.Column);
                    }
                    
                    if (_currentNameSpace.GetFuncByName(ret.name).Args.Count != ret.Args.Count) {
                        throw new SyntaxException($"Function {ret.name} called with {ret.Args.Count} args, " +
                                                  $"but it have {_currentNameSpace.GetFuncByName(ret.name).Args.Count} args " +
                                                  $"at {ret.Row + 1} : {ret.Column + 1}",
                            ret.Row, ret.Column);
                    }
                    
                    return ret;
                }

                _enumerator.MovePrevious();
                if (_currentNameSpace.varTable.ContainsKey(_enumerator.Current.data)) {
                    return new VarExpression(_enumerator.Current.row,
                        _enumerator.Current.column,
                        name);
                }

                throw new SyntaxException($"Variable used before assignment " +
                                          $"\"{_enumerator.Current.data.ToString()}\" " +
                                          $"at {_enumerator.Current.row}:{_enumerator.Current.column}",
                    _enumerator.Current.row, _enumerator.Current.column);
            }

            throw new SyntaxException(
                $"Unexpected token {_enumerator.Current.Type.ToString()} at {_enumerator.Current.row}:{_enumerator.Current.column}",
                _enumerator.Current.row, _enumerator.Current.column);
        }

        public AstTree GetAstTree() {
            return _base;
        }
    }
}