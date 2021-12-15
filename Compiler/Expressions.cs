using System;
using System.Collections.Generic;

namespace Compiler {
    // todo file Expressions with all expressions and etc
    public abstract class Expression : AstNode {
        protected Expression(int row, int col) : base(row, col) { }

        public virtual void PrintOperation(int depth) {
            for (var i = 0; i < depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(GetType().ToString());
        }
    }

    public class CallExpression : Expression {
        public readonly string Name;

        public readonly List<Expression> Args;

        public CallExpression(int row, int col, string name, List<Expression> args) : base(row, col) {
            Name = name;
            Args = args;
        }

        public override void PrintOperation(int depth) {
            base.PrintOperation(depth);
            for (var i = 0; i <= depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(Name);
        }
    }

    public class ConditionalExpression : Expression {
        public readonly Expression Body;

        public readonly Expression Condition;

        public readonly Expression? ElseBody;

        public ConditionalExpression(int row, int col,
            Expression body, Expression condition, Expression? elseBody) : base(row, col) {
            Body = body;
            Condition = condition;
            if (elseBody != null) {
                ElseBody = elseBody;
            }
        }
    }

    public class VarExpression : Expression {
        public readonly string VarName;

        public VarExpression(int row, int col, string data) : base(row, col) {
            VarName = data;
        }

        public override void PrintOperation(int depth) {
            base.PrintOperation(depth);
            for (var i = 0; i <= depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(VarName);
        }
    }

    public class ConstExpression : Expression {
        public readonly dynamic Data; // todo

        public ConstExpression(int row, int col, dynamic data) : base(row, col) {
            Data = data;
        }

        public override void PrintOperation(int depth) {
            base.PrintOperation(depth);
            for (var i = 0; i <= depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(Data);
        }
    }

    public class BinaryOperationExpression : Expression {
        public readonly TokenType Operation;

        public readonly Expression Left;

        public readonly Expression Right;

        public BinaryOperationExpression(int row, int col, TokenType operation, Expression left, Expression right) : base(row, col) {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public override void PrintOperation(int depth) {
            base.PrintOperation(depth);
            Left.PrintOperation(depth + 1);
            Console.WriteLine("\t" + Operation);
            Right.PrintOperation(depth + 1);
        }
    }
}