﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Compiler {
    public class Parser {
        private readonly List<Token> _tokens;
        private readonly MyEnumerator<Token> _tokensEnumerator;
        public AstTree AstTree { get; private set; }

        private INamespace _currentNameSpace;

        public Parser(List<Token> tokens) {
            _tokens = tokens;
            AstTree = new AstTree();
            _tokensEnumerator = new MyEnumerator<Token>(_tokens.GetEnumerator());
            _currentNameSpace = AstTree;
        }

        private Statement ParseWhileLoop() {
            var token = _tokensEnumerator.Current;
            _tokensEnumerator.MoveNext();
            var ret = new WhileLoopStatement(token.Row,
                token.Column, ParseExpr());
            SameCurrent(TokenType.Colon);
            if (!_tokensEnumerator.MoveNext()) {
                _tokensEnumerator.MovePrevious();
                if (_tokensEnumerator.Current != null)
                    throw new CompilerException(
                        $"Expected token {_tokensEnumerator.Current} at {_tokensEnumerator.Current.Row}:{_tokensEnumerator.Current.Column}");
            }

            if (_tokensEnumerator.Current == null) return ret;
            var body = new BlockStatement(_tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
            if (SameToCurrentBool(TokenType.Newline)) {
                Same(TokenType.Indent);
                _tokensEnumerator.MovePrevious();
                ParseUntil(body, TokenType.Dedent);
            }
            else {
                ParseUntil(body, TokenType.Newline);
            }

            ret.AddChild(body);

            return ret;
        }

        private Statement ParseConditional() {
            var rowCol = new {
                row = _tokensEnumerator.Current.Row, column = _tokensEnumerator.Current.Column
            };
            if (!_tokensEnumerator.MoveNext()) {
                throw new SyntaxException("Token expected",
                    rowCol.row, rowCol.column);
            }

            var condition = ParseExpr();
            SameCurrent(TokenType.Colon);

            var body = new BlockStatement(_tokensEnumerator.Current.Row,
                _tokensEnumerator.Current.Column);

            if (!_tokensEnumerator.MoveNext())
                if (_tokensEnumerator.Current != null)
                    throw new SyntaxException("Token expected",
                        _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);

            Same(TokenType.Indent);
            _tokensEnumerator.MovePrevious();
            ParseUntil(body,
                _tokensEnumerator.Current is {Type: TokenType.Newline} ? TokenType.Dedent : TokenType.Newline);

            if (SameBool(TokenType.ElseCondition)) {
                var conditionalElseStatement = new ElseStatement(rowCol.row,
                    rowCol.column,
                    condition
                );
                if (_tokensEnumerator.Current == null) return conditionalElseStatement;
                var elseBody = new BlockStatement(_tokensEnumerator.Current.Row,
                    _tokensEnumerator.Current.Column);
                if (!_tokensEnumerator.MoveNext())
                    if (_tokensEnumerator.Current != null)
                        throw new SyntaxException("Token expected",
                            _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
                _tokensEnumerator.MoveNext();
                Same(TokenType.Indent);
                _tokensEnumerator.MovePrevious();
                ParseUntil(elseBody,
                    _tokensEnumerator.Current is {Type: TokenType.Newline} ? TokenType.Dedent : TokenType.Newline);
                conditionalElseStatement.AddChild(body);
                conditionalElseStatement.AddChild(elseBody);

                return conditionalElseStatement;
            }

            var conditionalStatement = new IfStatement(rowCol.row,
                rowCol.column,
                condition
            );
            conditionalStatement.AddChild(body);

            return conditionalStatement;
        }

        private List<Expression> CheckArguments() {
            var res = new List<Expression>();
            Same(TokenType.OpenBracket);
            while (_tokensEnumerator.MoveNext()) {
                if (_tokensEnumerator.Current != null)
                    switch (_tokensEnumerator.Current.Type) {
                        case TokenType.Identifier:
                        case TokenType.IntegerNumber:
                            res.Add(ParseExpr());
                            //_enumerator.MoveNext();
                            switch (_tokensEnumerator.Current.Type) {
                                case TokenType.Comma:
                                    break;
                                case TokenType.CloseBracket:
                                    return res;
                                default:
                                    throw new SyntaxException(
                                        $"Unexpected token {_tokensEnumerator.Current.Type.ToString()} at {_tokensEnumerator.Current.Row + 1}:{_tokensEnumerator.Current.Column}",
                                        _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column
                                    );
                            }

                            break;
                        case TokenType.CloseBracket:
                            return res;
                        default:
                            throw new SyntaxException(
                                $"Unexpected token {_tokensEnumerator.Current.Type.ToString()} at {_tokensEnumerator.Current.Row + 1}:{_tokensEnumerator.Current.Column}",
                                _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column
                            );
                    }
            }

            return res;
        }

        private FuncStatement ParseFunc(Dictionary<string, int> varTable) {
            var def = new FuncStatement(_tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column, varTable) {
                Name = Same(TokenType.Identifier).Data,
                Args = MatchDefArgs()
            };
            def.FuncList = new List<FuncStatement>(_currentNameSpace.FuncList);
            _currentNameSpace.FuncList.Add(def);
            foreach (var arg in def.Args) {
                if (def.Variables.Keys.Contains(arg)) {
                    def.Variables.Remove(arg);
                }

                def.AddArg(arg);
            }

            Same(TokenType.Colon);

            if (SameBool(TokenType.Newline)) {
                var prevNameSpace = _currentNameSpace;
                _currentNameSpace = def;
                ParseUntil(def, TokenType.Dedent);
                _currentNameSpace = prevNameSpace;
            }
            else {
                SameCurrent(TokenType.Return);
                def.Return = ParseExpr();
                SameCurrent(TokenType.Newline);
            }

            return def;
        }

        private Expression ParseExpr() {
            var first = ParseTerm();
            while (SameToCurrentBool(TokenType.Subtract)) {
                if (_tokensEnumerator.Current == null) continue;
                var operatorType = _tokensEnumerator.Current.Type;
                if (!_tokensEnumerator.MoveNext()) continue;
                var second = ParseTerm();
                first = new BinaryOperationExpression(first.Row,
                    first.Column,
                    operatorType,
                    first,
                    second
                );
            }

            if (SameToCurrentBool(TokenType.IfCondition) && _tokensEnumerator.MoveNext()) {
                var condition = ParseExpr();

                SameCurrent(TokenType.ElseCondition);
                _tokensEnumerator.MoveNext();
                var elseExpression = ParseExpr();
                return new ConditionalExpression(first.Row,
                    first.Column,
                    first,
                    condition,
                    elseExpression);
            }

            if (!SameToCurrentBool(TokenType.Greater)) return first;
            if (_tokensEnumerator.Current == null) return first;
            var op = _tokensEnumerator.Current.Type;
            if (!_tokensEnumerator.MoveNext()) return first;
            var third = ParseExpr();
            first = new BinaryOperationExpression(first.Row,
                first.Column,
                op,
                first,
                third
            );

            return first;
        }

        private Expression ParseTerm() {
            var first = ParseFactor();
            while (_tokensEnumerator.MoveNext() &&
                   (SameToCurrentBool(TokenType.Multiply) ||
                    SameToCurrentBool(TokenType.Divide) ||
                    SameToCurrentBool(TokenType.NotEqual) ||
                    SameToCurrentBool(TokenType.Subtract))) {
                if (_tokensEnumerator.Current == null) continue;
                var termOperator = _tokensEnumerator.Current.Type;
                var errorRow = _tokensEnumerator.Current.Row;
                var errorCol = _tokensEnumerator.Current.Column;
                if (_tokensEnumerator.MoveNext()) {
                    var second = ParseFactor();
                    first = new BinaryOperationExpression(first.Row, first.Column, termOperator, first, second);
                }
                else {
                    throw new SyntaxException($"Expected token", errorRow, errorCol);
                }
            }


            return first;
        }

        private Expression ParseFactor() {
            if (SameToCurrentBool(TokenType.OpenBracket)) {
                if (_tokensEnumerator.MoveNext()) {
                    var expr = ParseExpr();
                    SameCurrent(TokenType.CloseBracket);
                    return expr;
                }
            }

            if (SameToCurrentBool(TokenType.IntegerNumber)) {
                if (_tokensEnumerator.Current != null)
                    return new ConstExpression(_tokensEnumerator.Current.Row,
                        _tokensEnumerator.Current.Column,
                        _tokensEnumerator.Current.Data);
            }

            if (!SameToCurrentBool(TokenType.Identifier))
                if (_tokensEnumerator.Current != null)
                    throw new SyntaxException(
                        $"Unexpected token {_tokensEnumerator.Current.Type.ToString()} at {_tokensEnumerator.Current.Row}:{_tokensEnumerator.Current.Column}",
                        _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
            if (_tokensEnumerator.Current == null)
                if (_tokensEnumerator.Current != null)
                    throw new SyntaxException($"Variable used before assignment " +
                                              $"\"{_tokensEnumerator.Current.Data.ToString()}\" " +
                                              $"at {_tokensEnumerator.Current.Row}:{_tokensEnumerator.Current.Column}",
                        _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
            var name = _tokensEnumerator.Current.Data;
            if (_tokensEnumerator.Current != null && _tokensEnumerator.MoveNext() &&
                _tokensEnumerator.Current is {Type: TokenType.OpenBracket}) {
                _tokensEnumerator.MovePrevious();
                if (_tokensEnumerator.Current != null) {
                    var ret = new CallExpression(_tokensEnumerator.Current.Row,
                        _tokensEnumerator.Current.Column,
                        name, CheckArguments());
                    if (!_currentNameSpace.ThereIsFuncWithName(ret.Name)) {
                        throw new SyntaxException($"Name {ret.Name} is not defined ", ret.Row, ret.Column);
                    }

                    if (_currentNameSpace.GetFuncByName(ret.Name).Args.Count != ret.Args.Count) {
                        throw new SyntaxException($"Function {ret.Name} called with {ret.Args.Count} args, " +
                                                  $"but it have {_currentNameSpace.GetFuncByName(ret.Name).Args.Count} args " +
                                                  $"at {ret.Row + 1} : {ret.Column + 1}",
                            ret.Row, ret.Column);
                    }

                    return ret;
                }
            }

            _tokensEnumerator.MovePrevious();
            if (_tokensEnumerator.Current != null &&
                _currentNameSpace.Variables.ContainsKey(_tokensEnumerator.Current.Data)) {
                return new VarExpression(_tokensEnumerator.Current.Row,
                    _tokensEnumerator.Current.Column,
                    name);
            }

            throw new SyntaxException($"Variable used before assignment " +
                                      $"\"{_tokensEnumerator.Current.Data.ToString()}\" " +
                                      $"at {_tokensEnumerator.Current.Row}:{_tokensEnumerator.Current.Column}",
                _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
        }

        private Token Same(TokenType tokenType) {
            if (!_tokensEnumerator.MoveNext()) throw new SyntaxException();
            if (_tokensEnumerator.Current != null && tokenType != _tokensEnumerator.Current.Type) {
                throw new SyntaxException("Got " + _tokensEnumerator.Current.Type +
                                          $", {tokenType.ToString()} expected" +
                                          $" at {_tokensEnumerator.Current.Row + 1}:{_tokensEnumerator.Current.Column + 1}",
                    _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
            }

            return _tokensEnumerator.Current;
        }

        private void SameCurrent(TokenType tokenType) {
            if (_tokensEnumerator.Current != null && tokenType != _tokensEnumerator.Current.Type) {
                throw new SyntaxException("Got " + _tokensEnumerator.Current.Type +
                                          $", {tokenType.ToString()} expected" +
                                          $" at {_tokensEnumerator.Current.Row + 1}:{_tokensEnumerator.Current.Column}",
                    _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column);
            }
        }

        private bool SameBool(TokenType tokenType) =>
            _tokensEnumerator.Current != null && _tokensEnumerator.MoveNext() && tokenType == _tokensEnumerator.Current.Type;

        private bool SameToCurrentBool(TokenType tokenType) {
            return _tokensEnumerator.Current != null && tokenType == _tokensEnumerator.Current.Type;
        }

        private void MatchIndentation() {
            if (!_tokensEnumerator.MoveNext()) return;
            if (SameToCurrentBool(TokenType.Newline) || SameToCurrentBool(TokenType.Dedent)) return;
            if (_tokensEnumerator.Current != null) Console.WriteLine(_tokensEnumerator.Current.ToString());
            throw new SyntaxException("Expected new line or semicolon");
        }

        private void MatchIndentationCurrent() {
            if (SameToCurrentBool(TokenType.Newline) || SameToCurrentBool(TokenType.Dedent)) return;
            if (_tokensEnumerator.Current != null) Console.WriteLine(_tokensEnumerator.Current.ToString());
            throw new SyntaxException("Expected new line or semicolon");
        }

        private Expression MatchReturn() {
            Same(TokenType.Return);
            if (_tokensEnumerator.Current != null) {
                var errorRow = _tokensEnumerator.Current.Row;
                var errorCol = _tokensEnumerator.Current.Column;
                if (!_tokensEnumerator.MoveNext())
                    throw new SyntaxException($"Expected token",
                        errorRow, errorCol);
            }

            var returnExpr = ParseExpr();
            return returnExpr;
        }

        private List<string> MatchDefArgs() {
            var res = new List<string>();
            Same(TokenType.OpenBracket);
            while (_tokensEnumerator.MoveNext()) {
                if (_tokensEnumerator.Current != null)
                    switch (_tokensEnumerator.Current.Type) {
                        case TokenType.Identifier:
                            res.Add(_tokensEnumerator.Current.Data);
                            //Console.WriteLine(res[^1].ToString());
                            _tokensEnumerator.MoveNext();
                            if (_tokensEnumerator.Current != null)
                                switch (_tokensEnumerator.Current.Type) {
                                    case TokenType.Comma:
                                        break;
                                    case TokenType.CloseBracket:
                                        return res;
                                    default:
                                        throw new SyntaxException(
                                            $"Unexpected token at {_tokensEnumerator.Current.Row + 1}:{_tokensEnumerator.Current.Column}",
                                            _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column
                                        );
                                }

                            break;
                        case TokenType.CloseBracket:
                            return res;
                        default:
                            throw new SyntaxException(
                                $"Unexpected token at {_tokensEnumerator.Current.Row + 1}:{_tokensEnumerator.Current.Column}",
                                _tokensEnumerator.Current.Row, _tokensEnumerator.Current.Column
                            );
                    }
            }

            return res;
        }

        public void Parse() {
            ParseUntil(AstTree.Root);
        }

        private void ParseUntil(RootNode baseNode, TokenType? stopToken = null) {
            while (_tokensEnumerator.MoveNext()) {
                var token = _tokensEnumerator.Current;
                if (token.Type == stopToken) {
                    break;
                }

                switch (token.Type) {
                    case TokenType.FuncDefinition: {
                        var temp = baseNode switch {
                            INamespace tableContainer =>
                                ParseFunc(new Dictionary<string, int>(tableContainer.Variables)),
                            _ => ParseFunc(new Dictionary<string, int>(AstTree.Variables))
                        };

                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenType.Identifier: {
                        if (_tokensEnumerator.MoveNext()) {
                            if (_tokensEnumerator.Current != null &&
                                _tokensEnumerator.Current.Type == TokenType.Assignment) {
                                _tokensEnumerator.MovePrevious();
                                if (_tokensEnumerator.Current != null) {
                                    var name = _tokensEnumerator.Current.Data;
                                    _tokensEnumerator.MoveNext();
                                    _tokensEnumerator.MoveNext();
                                    var expr = ParseExpr();
                                    baseNode.AddChild(new AssignStatement(
                                        _tokensEnumerator.Current.Row,
                                        _tokensEnumerator.Current.Column,
                                        name, expr));
                                    switch (baseNode) {
                                        case INamespace tableContainer: {
                                            tableContainer.AddVariable(name);
                                            break;
                                        }
                                        default: {
                                            _currentNameSpace.AddVariable(name);
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (_tokensEnumerator.Current.Type == TokenType.OpenBracket) {
                                _tokensEnumerator.MovePrevious();
                                var tempEx = ParseExpr();
                                var temp = new ExpressionStatement(tempEx.Row, tempEx.Column, tempEx);
                                baseNode.AddChild(temp);
                                MatchIndentation();
                                break;
                            }
                            else {
                                _tokensEnumerator.MovePrevious();
                                baseNode.AddChild(new ExpressionStatement(
                                    _tokensEnumerator.Current.Row,
                                    _tokensEnumerator.Current.Column,
                                    ParseExpr()));
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
                        var temp = new ExpressionStatement(_tokensEnumerator.Current.Row,
                            _tokensEnumerator.Current.Column, ParseExpr());
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
                        var row = _tokensEnumerator.Current.Row;
                        var column = _tokensEnumerator.Current.Column;
                        Same(TokenType.OpenBracket);
                        var temp = new PrintStatement(row, column, ParseExpr());
                        _tokensEnumerator.MovePrevious();
                        SameCurrent(TokenType.CloseBracket);
                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenType.Return: {
                        // todo fix exceptions messages
                        if (_currentNameSpace.GetType() != typeof(FuncStatement)) {
                            throw new SyntaxException(
                                $"Return outside of function at {_tokensEnumerator.Current.Row}:" +
                                $"{_tokensEnumerator.Current.Column}",
                                _tokensEnumerator.Current.Row,
                                _tokensEnumerator.Current.Column);
                        }

                        var currentToken = _tokensEnumerator.Current;
                        _tokensEnumerator.MovePrevious();
                        var currentNameSpace = ((FuncStatement) _currentNameSpace);
                        currentNameSpace.Return = MatchReturn();
                        baseNode.AddChild(new ReturnStatement(currentToken.Row, currentToken.Column,
                            currentNameSpace.Return));
                        break;
                    }
                    default: {
                        break;
                    }
                }
            }
        }
    }
}