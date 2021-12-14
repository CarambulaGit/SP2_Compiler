using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compiler;

public class AsmCodeGenerator {
    private readonly AstTree _base;

    private INamespace _currentNameSpace;

    private List<string> _functionProtoNames;

    private List<string> _functions;

    private List<string> _statements;

    private string _currModule = "MyModule";
    private int _currentFreeId;

    private static readonly Dictionary<Type, string> TemplateDict = new Dictionary<Type, string>() {
        {typeof(CallStatement), "call {0}\n"},
        {typeof(AssignStatement), "{0}\n\tpop eax\n\tmov dword ptr[ebp{1}], eax\n"},
        {typeof(ExprStatement), "{0}\n"}, {
            typeof(ConditionalElseStatement), "{0}" +
                                              "pop eax\n" +
                                              "cmp eax, 0\n" +
                                              "je {1}else\n" +
                                              "{2}" +
                                              "jmp {1}final\n" +
                                              "{1}else:\n" +
                                              "{3}" +
                                              "{1}final:\n"
        }, {
            typeof(ConditionalStatement), "{0}" +
                                          "pop eax\n" +
                                          "cmp eax, 0\n" +
                                          "je {1}else\n" +
                                          "{2}" +
                                          "{1}else:\n"
        }, {
            typeof(WhileLoopStatement), "Loop{0}start:\n" +
                               "{1}" +
                               "pop eax\n" +
                               "cmp eax, 0\n" +
                               "je Loop{0}end\n" +
                               "{2}" +
                               "jmp Loop{0}start\n" +
                               "Loop{0}end:\n"
        },
        // {typeof(Print), "{0}printf(str$(eax))\nprintf(\"\\n\")\n"}
        {typeof(PrintStatement), "{0}fn MessageBoxA,0, str$(eax), \"Didenko Vladyslav IO-91\", MB_OK\n"}
    };

    private const string ProcTemplate = "{0} PROC\n" +
                                        "{1}\n" +
                                        "{0} ENDP\n";

    private const string ProtoTemplate = "{0} PROTO\n";

    private string _templateMasm = ".386\n" +
                                   ".model flat,stdcall\n" +
                                   "option casemap:none\n\n" +
                                   @"include \masm32\include\masm32rt.inc" + "\n" +
                                   "_main        PROTO\n\n" +
                                   "{0}\n" + // insert prototype of functions
                                   ".data\n" +
                                   ".code\n" +
                                   "_start:\n" +
                                   "push ebp\n" +
                                   "mov ebp, esp\n" +
                                   "sub esp, {3}\n" +
                                   "invoke  _main\n" +
                                   "add esp, {3}\n" +
                                   "mov esp, ebp\n" +
                                   "pop ebp\n" +
                                   "ret\n" +
                                   "_main PROC\n\n" +
                                   "\n" +
                                   "{1}" + // insert code
                                   "\n" +
                                   // "fn MessageBoxA,0,str$(eax), \"Didenko Vladyslav IO-91\", MB_OK\n" +
                                   "printf(\"\\n\")\n" +
                                   "inkey\n" +
                                   "\nret\n\n" +
                                   "_main ENDP\n\n" +
                                   "{2}" + // insert functions
                                   "END _start\n";

    public AsmCodeGenerator(AstTree Base) {
        _base = Base;
        _functions = new List<string>();
        _statements = new List<string>();
        _functionProtoNames = new List<string>();
        _currentNameSpace = Base;
    }

    public void GenerateAsm() {
        foreach (var child in _base.Root.GetChildren()) {
            _statements.Add(GenerateCode(child));
        }

        using (var fs = File.Create(
            "output.asm")) {
            var info = new UTF8Encoding(true).GetBytes(
                string.Format(_templateMasm, string.Join("", _functionProtoNames.ToArray()),
                    string.Join("", _statements.ToArray()),
                    string.Join("", _functions.ToArray()),
                    (_currentNameSpace.varTable.Count * 4).ToString()));
            fs.Write(info, 0, info.Length);
        }
    }

    private string GenerateFunction(DefStatement defStatement) {
        var oldNameSpace = _currentNameSpace;
        _currentNameSpace = defStatement;
        var bodystatements = new StringBuilder();
        bodystatements.Append($"push ebp\nmov ebp, esp\nsub esp, {defStatement.VarCounter * 4}\n");

        foreach (var statement in defStatement.GetChildren()) {
            //Console.WriteLine(statement.GetType() + statement.Row.ToString() + ':' + statement.Column.ToString());
            bodystatements.Append(GenerateCode(statement));
            bodystatements.Append('\n');
        }

        bodystatements.Append($"add esp, {defStatement.VarCounter * 4}\nmov esp, ebp\npop ebp\n");

        bodystatements.Append($"ret {defStatement.Args.Count * 4}\n");

        _currentNameSpace = oldNameSpace;
        _functionProtoNames.Add(string.Format(ProtoTemplate, defStatement.Name));

        _functions.Add(string.Format(ProcTemplate, defStatement.Name, bodystatements.ToString()));
        return "\n";
    }

    private string GenerateWhileLoop(WhileLoopStatement whileLoopStatement) {
        var id = GenerateId();
        //Console.WriteLine(GenerateExpr(whileLoop.Condition));
        var ret = string.Format(TemplateDict[whileLoopStatement.GetType()], id,
            GenerateExpr(whileLoopStatement.Condition),
            GenerateCode(whileLoopStatement.GetChildren()[0]));
        return ret;
    }

    private string GenerateBinExpr(BinaryOperationExpression e) {
        string code;
        var a = GenerateExpr(e.LeftExpression);
        var b = GenerateExpr(e.RightExpression);
        if (e.Op == TokenType.Subtract) {
            code = $"{b}\n{a}\npop eax\npop ecx\nsub eax, ecx\npush eax\n";
        }
        else if (e.Op == TokenType.Multiply) {
            code = $"{b}\n{a}\npop eax\npop ecx\nimul ecx\npush eax\n";
        }
        else if (e.Op == TokenType.Divide) {
            code = $"{b}\n{a}\npop eax\npop ebx\nxor edx, edx\ndiv ebx\npush eax\n";
        }
        else if (e.Op == TokenType.NotEqual) {
            code = $"{b}\n{a}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nsetne al\npush eax\n";
        }
        else if (e.Op == TokenType.Greater) {
            code = $"{b}\n{a}\npop eax\npop ecx\ncmp ecx, eax\nmov eax, 0\nsetl al\npush eax\n";
        }
        else {
            throw new CompilerException($"Sorry, but {e.Op.ToString()} not implemented yet");
        }

        //Console.WriteLine(code);
        return code;
    }

    // private string GenerateUnExpr(UnOp e) {
    //     string code = "";
    //     var expr = GenerateExpr(e.Expression);
    //     if (e.Op == TokenKind.MINUS) {
    //         code = expr + $"\npop eax\nneg eax\npush eax\n";
    //     }
    //     else {
    //         throw new CompilerException($"Sorry, but {e.Op.ToString()} not implemented yet");
    //     }
    //
    //     return code;
    // }

    private string GenerateConstExpr(ConstExpression e) {
        return $"\tpush {e.Data}\n";
    }

    private string GenerateVarExpr(VarExpression e) {
        return $"mov eax, dword ptr[ebp{GetVarOffset(e.varName)}] ; {e.varName}\n" +
               $"push eax\n";
    }

    private string GenerateReturn(Expression ret) {
        var func = (DefStatement) _currentNameSpace;
        return
            $"{GenerateExpr(ret)}\npop eax\nadd esp, {func.VarCounter * 4}\nmov esp, ebp\npop ebp\nret {func.Args.Count * 4}\n";
    }

    private string GenerateCallExpression(CallExpression e) {
        var st = new StringBuilder();
        e.Args.Reverse();
        if (e.Args.Count > 0) {
            foreach (var arg in e.Args) {
                st.Append(GenerateExpr(arg));
            }
        }

        st.Append($"invoke {e.name}\n push eax\n");
        return st.ToString();
    }

    private string GenerateConditionalExpression(ConditionalExpression e) {
        var currId = GenerateId();
        if (e.elseBody != null) {
            return $"{GenerateExpr(e.condition)}\npop eax\ncmp eax, 0\nje {currId}else\n" +
                   $"{GenerateExpr(e.body)}\njmp {currId}final\n" +
                   $"{currId}else:\n{GenerateExpr(e.elseBody)}\n" +
                   $"{currId}final:\n";
        }

        return $"{GenerateExpr(e.condition)}\npop eax\ncmp eax, 0\nje {currId}final\n" +
               $"{GenerateExpr(e.body)}\n" +
               $"{currId}final:\n";
    }

    private string GenerateExpr(Expression e) {
        return e switch {
            BinaryOperationExpression binop => GenerateBinExpr(binop),
            // UnOp unop => GenerateUnExpr(unop),
            ConstExpression constExpression => GenerateConstExpr(constExpression), // todo remove?
            VarExpression varExpression => GenerateVarExpr(varExpression),
            CallExpression callExpression => GenerateCallExpression(callExpression),
            ConditionalExpression conditionalExpression => GenerateConditionalExpression(conditionalExpression),
            _ => throw new CompilerException($"{e.GetType()} at row = {e.Row} column = {e.Column}")
        };
    }

    private string GenerateId() {
        return $"{_currModule}{_currentFreeId++}";
    }

    private string GetVarOffset(string varName) {
        // todo remove?
        return _currentNameSpace.varTable[varName] < 0 ? $"+{-_currentNameSpace.varTable[varName]}" : $"-{_currentNameSpace.varTable[varName]}";
    }

    private string TrimPush(string s) {
        if (s.EndsWith("push eax\n")) {
            return s.Substring(0, s.IndexOf("push eax\n", StringComparison.Ordinal));
        }

        return s;
    }

    private string GenerateCode(AstNode st) {
        //Console.WriteLine(st.GetType());
        return st switch {
            CallStatement callStatement => string.Format(TemplateDict[st.GetType()],
                callStatement.Name),
            BlockStatement blockStatement =>
                string.Join('\n',
                    blockStatement.GetChildren()
                        .Select(c => GenerateCode(c) + '\n').ToArray()),
            //blockStatement.GetChildren().S,
            AssignStatement assignStatement =>
                string.Format(TemplateDict[assignStatement.GetType()],
                    GenerateExpr(assignStatement.Expression),
                    GetVarOffset(assignStatement.VarName)),
            ExprStatement exprStatement =>
                string.Format(TemplateDict[exprStatement.GetType()],
                    TrimPush(GenerateExpr(exprStatement.Expression))),
            ConditionalElseStatement conditionalElseStatement =>
                string.Format(TemplateDict[conditionalElseStatement.GetType()],
                    GenerateExpr(conditionalElseStatement.Condition),
                    GenerateId(),
                    GenerateCode(conditionalElseStatement.GetChildren()[0]),
                    GenerateCode(conditionalElseStatement.GetChildren()[1])
                ),
            ConditionalStatement conditionalStatement =>
                string.Format(TemplateDict[conditionalStatement.GetType()],
                    GenerateExpr(conditionalStatement.Condition),
                    GenerateId(),
                    GenerateCode(conditionalStatement.GetChildren()[0])),
            WhileLoopStatement whileLoop =>
                GenerateWhileLoop(whileLoop),
            DefStatement defStatement =>
                GenerateFunction(defStatement),
            ReturnStatement returnStatement =>
                GenerateReturn(returnStatement.Expr),
            PrintStatement print =>
                string.Format(TemplateDict[print.GetType()],
                    TrimPush(GenerateExpr(print.expr))),
            _ => throw new CompilerException(
                $"Ooops, unknown type, seems like this feature is in development {st.GetType()}" +
                $" {st.Row + 1}:{st.Column + 1}")
        } ?? throw new Exception();
    }
}