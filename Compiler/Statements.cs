using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public abstract class Statement : AstNode {
        public string Name { get; set; }

        protected Statement(int row, int col) : base(row, col) { }
    }

    public class ExprStatement : Statement {
        public Expression Expression { get; set; }

        public ExprStatement(int row, int col, Expression e) : base(row, col) {
            Expression = e;
        }
    }

    public class ConditionalStatement : Statement {
        public Expression Condition { get; set; }

        public ConditionalStatement(int row, int col,
            Expression condition) : base(row, col) {
            Condition = condition;
        }
    }

    public class ConditionalElseStatement : ConditionalStatement {
        public ConditionalElseStatement(int row, int col, Expression condition) : base(row, col, condition) { }
    }

    class BlockStatement : Statement {
        public BlockStatement(int row, int col) : base(row, col) { }
    }

    public class AssignStatement : Statement {
        public string VarName { get; set; }

        public Expression Expression { get; set; }

        public AssignStatement(int row, int col, string name, Expression e) : base(row, col) {
            VarName = name;
            Expression = e;
        }
    }
    
    public class WhileLoopStatement : Statement {
        public Expression Condition { get; set; }

        public WhileLoopStatement(int row, int col, Expression condition) : base(row, col) {
            Condition = condition;
        }
    }

    public class DefStatement : Statement, INamespace {
        public List<string> Args;

        public Dictionary<string, int> varTable { get; set; }

        public List<DefStatement> FuncList { get; set; }

        public int VarCounter { get; set; }

#nullable enable
        public Expression? Return;
#nullable disable


        public DefStatement(int row, int col, Dictionary<string, int> varTable) : base(row, col) {
            Args = new List<string>();
            this.varTable = varTable;
            FuncList = new List<DefStatement>();
        }

        public void AddArg(string argName) {
            varTable[argName] = -(varTable.Count(vt => vt.Value < 0) + 2) * 4;
        }

        public void AddVar(string varName) {
            if (varTable.ContainsKey(varName)) return;
            VarCounter++;
            var indexes = varTable.Keys.ToList();
            foreach (var index in indexes.Where(index => varTable[index] > 0)) {
                varTable[index] += 4;
            }

            varTable[varName] = 4;
        }
    }

    public class CallStatement : Statement {
        public List<Expression> Args;

        public CallStatement(int row, int col) : base(row, col) {
            Args = new List<Expression>();
        }
    }

    public class ReturnStatement : Statement {
        public Expression Expr { get; set; }
        public ReturnStatement(int row, int col) : base(row, col) { }

        public ReturnStatement(int row, int col, Expression ret) : base(row, col) {
            Expr = ret;
        }
    }

    public class PrintStatement : Statement {
        public Expression expr { get; set; }
        public PrintStatement(int row, int col) : base(row, col) { }
    }
}