using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public abstract class Statement : Ast {
        protected Statement(int row, int col) : base(row, col) { }
        public string Name { get; set; }
    }

    public abstract class StatementWithExpression : Statement {
        public Expression Expression { get; set; }

        protected StatementWithExpression(int row, int col, Expression expression) : base(row, col) {
            Expression = expression;
        }
    }

    public class ExpressionStatement : StatementWithExpression {
        public ExpressionStatement(int row, int col, Expression expression) : base(row, col, expression) {
        }
    }

    public class IfStatement : StatementWithExpression {
        public Expression Condition => Expression;

        public IfStatement(int row, int col,
            Expression condition) : base(row, col, condition) {
        }
    }

    public class ElseStatement : StatementWithExpression {
        public Expression Condition => Expression;

        public ElseStatement(int row, int col, Expression condition) : base(row, col, condition) {
            
        }
    }

    internal class BlockStatement : Statement {
        public BlockStatement(int row, int col) : base(row, col) { }
    }

    public class AssignStatement : StatementWithExpression {
        public string VarName { get; set; }
        
        public AssignStatement(int row, int col, string name, Expression e) : base(row, col, e) {
            VarName = name;
        }
    }
    
    public class WhileLoopStatement : StatementWithExpression {
        public Expression Condition => Expression;

        public WhileLoopStatement(int row, int col, Expression condition) : base(row, col, condition) { }
    }

    public class FuncStatement : Statement, INamespace {
        public List<string> Args;

        public Dictionary<string, int> Variables { get; set; }

        public List<FuncStatement> FuncList { get; set; }

        public int VarCounter { get; set; }

        public Expression? Return;

        public FuncStatement(int row, int col, Dictionary<string, int> varTable) : base(row, col) {
            Args = new List<string>();
            this.Variables = varTable;
            FuncList = new List<FuncStatement>();
        }

        public void AddArg(string argName) {
            Variables[argName] = -(Variables.Count(vt => vt.Value < 0) + 2) * 4;
        }

        public void AddVariable(string varName) {
            if (Variables.ContainsKey(varName)) return;
            VarCounter++;
            var indexes = Variables.Keys.ToList();
            foreach (var index in indexes.Where(index => Variables[index] > 0)) {
                Variables[index] += 4;
            }

            Variables[varName] = 4;
        }
    }
    
    public class ReturnStatement : StatementWithExpression {
        public Expression Return => Expression;
        public ReturnStatement(int row, int col, Expression returnExpression) : base(row, col, returnExpression) { }
    }

    public class PrintStatement : StatementWithExpression {
        public PrintStatement(int row, int col, Expression expression) : base(row, col, expression) { }
    }
}