using System;
using System.Collections.Generic;

namespace Compiler {
    // todo 
    // typedef enum ExprKind {
    //     EXPR_NONE,
    //     EXPR_INT,
    //     EXPR_FLOAT,
    //     EXPR_UNARY,
    //     EXPR_BINARY,
    // } ExprKind;
    //
    // struct Expr {
    //     ExprKind kind;
    //     struct Type* type;
    //     union {
    //         int64_t int_val;
    //         double float_val;
    //         char char_val;
    //         struct {
    //             TokenKind op;
    //             Expr* expr;
    //         } unary;
    //         struct {
    //             TokenKind op;
    //             Expr* left;
    //             Expr* right;
    //         } binary;
    //     };
    // };
    // todo file Expressions with all expressions and etc
    public abstract class Expression : AstNode {
        protected Expression(int row, int col) : base(row, col) { }

        public override string ToString() {
            return this.GetType().ToString();
        }

        public virtual void PrintOp(int depth) {
            for (var i = 0; i < depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(this.GetType().ToString());
        }
    }

    public class CallExpression : Expression {
        public readonly string name;

        public List<Expression> Args;

        public CallExpression(int row, int col, string name) : base(row, col) {
            this.name = name;
            Args = new List<Expression>();
        }

        public CallExpression(int row, int col, string name, List<Expression> args) : base(row, col) {
            this.name = name;
            Args = args;
        }

        public override void PrintOp(int depth) {
            base.PrintOp(depth);
            for (var i = 0; i <= depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(name);
        }
    }

    public class ConditionalExpression : Expression {
        public readonly Expression body;

        public readonly Expression condition;

        public readonly Expression? elseBody;

        public ConditionalExpression(int row, int col,
            Expression body, Expression condition, Expression? elseBody) : base(row, col) {
            this.body = body;
            this.condition = condition;
            if (elseBody != null) {
                this.elseBody = elseBody;
            }
        }
    }

    public class VarExpression : Expression {
        public readonly string varName;

        public VarExpression(int row, int col, string data) : base(row, col) {
            varName = data;
        }

        public override void PrintOp(int depth) {
            base.PrintOp(depth);
            for (var i = 0; i <= depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(varName);
        }
    }

    public class ConstExpression : Expression {
        public readonly dynamic Data;

        public ConstExpression(int row, int col, dynamic data) : base(row, col) {
            Data = data;
        }

        public override void PrintOp(int depth) {
            base.PrintOp(depth);
            for (var i = 0; i <= depth; i++) {
                Console.Write('\t');
            }

            Console.WriteLine(Data);
        }
    }

    // public class UnOp : Expression {
    //     public readonly TokenKind Op;
    //
    //     public readonly Expression Expression;
    //
    //     public UnOp(int row, int col, TokenKind op, Expression e) : base(row, col) {
    //         Op = op;
    //         Expression = e;
    //     }
    //
    //     public override void PrintOp(int depth) {
    //         base.PrintOp(depth);
    //         for (var i = 0; i <= depth; i++) {
    //             Console.Write('\t');
    //         }
    //
    //         Console.WriteLine(Op.ToString());
    //         Expression.PrintOp(depth + 1);
    //     }
    // }

    public class BinaryOperationExpression : Expression {
        public readonly TokenType Op;

        public readonly Expression LeftExpression;

        public readonly Expression RightExpression;

        public BinaryOperationExpression(int row, int col, TokenType op, Expression le, Expression re) : base(row, col) {
            Op = op;
            LeftExpression = le;
            RightExpression = re;
        }

        public override void PrintOp(int depth) {
            base.PrintOp(depth);
            //Console.WriteLine(this.ToString());
            LeftExpression.PrintOp(depth + 1);
            Console.WriteLine("\t" + Op.ToString());
            RightExpression.PrintOp(depth + 1);
        }
    }
}