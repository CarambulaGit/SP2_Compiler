using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compiler;

public class AsmCodeGenerator {
    private const string ModuleName = "name";
    private readonly AstTree _root;
    private INamespace _currentNameSpace;
    private int _currentFreeId;
    private readonly List<string> _functions;
    private readonly List<string> _functionProtoNames;
    public string AsmCode { get; private set; }

    private static readonly Dictionary<Type, string> StatementCodeDict = new Dictionary<Type, string>() {
        {typeof(PrintStatement), Constants.PRINT_STATEMENT_ASM},
        {typeof(AssignStatement), Constants.ASSIGN_STATEMENT_ASM},
        {typeof(ExpressionStatement), Constants.EXPRESSION_STATEMENT_ASM},
        {typeof(IfStatement), Constants.IF_STATEMENT_ASM},
        {typeof(ElseStatement), Constants.ELSE_STATEMENT_ASM},
        {typeof(WhileLoopStatement), Constants.WHILE_STATEMENT_ASM},
    };

    private readonly List<string> _statementsList;

    private const string ProcedureTemplate = Constants.PROCEDURE_ASM;

    private const string ProtoTemplate = Constants.PROTO_ASM;

    private const string MasmCodeTemplate = Constants.MASM_CODE_TEMPLATE;

    public AsmCodeGenerator(AstTree root) {
        _functionProtoNames = new List<string>();
        _statementsList = new List<string>();
        _functions = new List<string>();
        _root = root;
        _currentNameSpace = root;
    }

    public void GenerateAsm() {
        foreach (var child in _root.Root.GetChildren()) {
            _statementsList.Add(GenerateCode(child));
        }


        AsmCode = string.Format(MasmCodeTemplate, string.Join("", _functionProtoNames.ToArray()),
            string.Join("", _statementsList.ToArray()),
            string.Join("", _functions.ToArray()),
            (_currentNameSpace.Variables.Count * 4).ToString());
    }

    private string GenerateFunction(FuncStatement funcStatement) {
        var oldNameSpace = _currentNameSpace;
        _currentNameSpace = funcStatement;
        var bodyStatements = new StringBuilder();
        bodyStatements.Append(string.Format(Constants.VARIABLE_ASM, funcStatement.VarCounter * 4));

        foreach (var statement in funcStatement.GetChildren()) {
            bodyStatements.Append(GenerateCode(statement));
            bodyStatements.Append('\n');
        }

        bodyStatements.Append(string.Format(Constants.FUNCTION_BODY_PARAMS_ASM, funcStatement.VarCounter * 4));

        bodyStatements.Append(string.Format(Constants.FUNCTION_BODY_ARGUMENTS_ASM, funcStatement.Args.Count * 4));

        _currentNameSpace = oldNameSpace;
        _functionProtoNames.Add(string.Format(ProtoTemplate, funcStatement.Name));

        _functions.Add(string.Format(ProcedureTemplate, funcStatement.Name, bodyStatements));
        return "\n";
    }

    private string GenerateWhileLoop(WhileLoopStatement whileLoopStatement) {
        var id = GenerateId();
        var ret = string.Format(StatementCodeDict[whileLoopStatement.GetType()], id,
            GenerateExpr(whileLoopStatement.Condition),
            GenerateCode(whileLoopStatement.GetChildren()[0]));
        return ret;
    }

    private string GenerateBinExpr(BinaryOperationExpression expression) {
        var left = GenerateExpr(expression.Left);
        var right = GenerateExpr(expression.Right);
        var operation = expression.Operation switch {
            TokenType.Subtract => string.Format(Constants.SUBSTRACT_ASM, right, left),
            TokenType.Multiply => string.Format(Constants.MULTIPLY_ASM, right, left),
            TokenType.Divide => string.Format(Constants.DIVIDE_ASM, right, left),
            TokenType.NotEqual => string.Format(Constants.NOT_EQUAL_ASM, right, left),
            TokenType.Greater => string.Format(Constants.GREATER_ASM, right, left),
            _ => throw new CompilerException($"{expression.Operation.ToString()} not implemented yet")
        };

        return operation;
    }

    private string GenerateConstExpr(ConstExpression expression) {
        return string.Format(Constants.CONST_ASM, (object?) expression.Data);
    }

    private string GenerateVarExpr(VarExpression expression) {
        return string.Format(Constants.VAR_EXPRESSION_ASM, GetVarOffset(expression.VarName), expression.VarName);
    }

    private string GenerateReturn(Expression returnExpression) {
        return string.Format(Constants.RETURN_ASM, GenerateExpr(returnExpression),
            ((FuncStatement) _currentNameSpace).VarCounter * 4, ((FuncStatement) _currentNameSpace).Args.Count * 4);
    }

    private string GenerateCallExpression(CallExpression callExpression) {
        var stringBuilder = new StringBuilder();
        callExpression.Args.Reverse();
        if (callExpression.Args.Count > 0) {
            for (var i = 0; i < callExpression.Args.Count; i++) {
                var arg = callExpression.Args[i];
                stringBuilder.Append(GenerateExpr(arg));
            }
        }

        stringBuilder.Append(string.Format(Constants.CALL_EXPRESSION_ASM, callExpression.Name));
        return stringBuilder.ToString();
    }

    private string GenerateConditionalExpression(ConditionalExpression conditionalExpression) {
        var currId = GenerateId();
        if (conditionalExpression.ElseBody != null) {
            return string.Format("{0}{1}{2}{3}",
                string.Format(Constants.CONDITION_IF_WITH_ELSE_ASM, GenerateExpr(conditionalExpression.Condition), currId),
                string.Format(Constants.CONDITION_BODY_ASM, GenerateExpr(conditionalExpression.Body), currId),
                string.Format(Constants.CONDITION_ELSE_ASM, currId, GenerateExpr(conditionalExpression.ElseBody)),
                string.Format(Constants.ID_ASM, currId));
        }

        return string.Format(Constants.CONDITION_IF_ASM, GenerateExpr(conditionalExpression.Condition), currId) +
               $"{GenerateExpr(conditionalExpression.Body)}\n" +
               string.Format(Constants.ID_ASM, currId);
    }

    private string GenerateExpr(Expression expression) {
        return expression switch {
            BinaryOperationExpression binaryOperationExpression => GenerateBinExpr(binaryOperationExpression),
            ConstExpression constExpression => GenerateConstExpr(constExpression),
            VarExpression varExpression => GenerateVarExpr(varExpression),
            CallExpression callExpression => GenerateCallExpression(callExpression),
            ConditionalExpression conditionalExpression => GenerateConditionalExpression(conditionalExpression),
            _ => throw new CompilerException(
                $"{expression.GetType()} at row = {expression.Row} column = {expression.Column}")
        };
    }

    private string GenerateId() {
        return $"{ModuleName}{_currentFreeId++}";
    }

    private string GetVarOffset(string varName) {
        return _currentNameSpace.Variables[varName] < 0
            ? $"+{-_currentNameSpace.Variables[varName]}"
            : $"-{_currentNameSpace.Variables[varName]}";
    }

    private string TrimPush(string s) => s.EndsWith("push eax\n") ? s[..s.IndexOf("push eax\n", StringComparison.Ordinal)] : s;

    private string GenerateCode(AstNode st) {
        return st switch {
            AssignStatement assignStatement =>
                GenerateAssigStatement(assignStatement),
            BlockStatement blockStatement =>
                GenerateBlockStatement(blockStatement),
            WhileLoopStatement whileLoop =>
                GenerateWhileLoop(whileLoop),
            ExpressionStatement expressionStatement =>
                GenerateExpressionStatement(expressionStatement),
            ElseStatement elseStatement =>
                GenerateElseStatement(elseStatement),
            IfStatement ifStatement =>
                GenerateIfStatement(ifStatement),
            FuncStatement funcStatement =>
                GenerateFunction(funcStatement),
            ReturnStatement returnStatement =>
                GenerateReturn(returnStatement.Return),
            PrintStatement print =>
                string.Format(StatementCodeDict[print.GetType()],
                    TrimPush(GenerateExpr(print.Expression))),
            _ => throw new CompilerException(
                $"Unknown type: {st.GetType()}" +
                $" {st.Row + 1}:{st.Column + 1}")
        } ?? throw new Exception();
    }

    private string GenerateIfStatement(IfStatement ifStatement) {
        return string.Format(StatementCodeDict[ifStatement.GetType()],
            GenerateExpr(ifStatement.Condition),
            GenerateId(),
            GenerateCode(ifStatement.GetChildren()[0]));
    }

    private string GenerateElseStatement(ElseStatement elseStatement) {
        return string.Format(StatementCodeDict[elseStatement.GetType()],
            GenerateExpr(elseStatement.Condition),
            GenerateId(),
            GenerateCode(elseStatement.GetChildren()[0]),
            GenerateCode(elseStatement.GetChildren()[1])
        );
    }

    private string GenerateExpressionStatement(ExpressionStatement exprStatement) {
        return string.Format(StatementCodeDict[exprStatement.GetType()],
            TrimPush(GenerateExpr(exprStatement.Expression)));
    }

    private string GenerateBlockStatement(BlockStatement blockStatement) {
        return string.Join('\n',
            blockStatement.GetChildren()
                .Select(c => GenerateCode(c) + '\n').ToArray());
    }

    private string GenerateAssigStatement(AssignStatement assignStatement) {
        return string.Format(StatementCodeDict[assignStatement.GetType()],
            GenerateExpr(assignStatement.Expression),
            GetVarOffset(assignStatement.VarName));
    }
}