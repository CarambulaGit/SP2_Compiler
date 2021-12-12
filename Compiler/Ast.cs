using System;
using System.Collections.Generic;
using System.Linq;
using static Compiler.Statement;

namespace Compiler {
    public class Ast : IVariableTableContainer {
        public readonly RootNode root;
        public Dictionary<string, int> varTable { get; set; } = new Dictionary<string, int>();

        public List<DefStatement> FuncList { get; set; }

        public Ast(RootNode root) {
            this.root = root;
            FuncList = new List<DefStatement>();
        }

        public Ast() {
            root = new RootNode();
            FuncList = new List<DefStatement>();
        }

        public RootNode GetRoot() {
            return root;
        }

        public bool HaveVariable(string v) {
            if (varTable.ContainsKey(v)) {
                return true;
            }

            return false;
        }

        public bool HaveFunction(string f) {
            return FuncList.Find(func => func.Name == f) != null;
        }

        public void AddFunction(DefStatement f) {
            FuncList.Add(f);
        }

        public DefStatement GetFunctionWithName(string f) {
            return FuncList.Find(func => func.Name == f) ?? throw new NullReferenceException();
        }

        public Dictionary<string, int> GetTable() {
            return varTable;
        }

        public int GetVarIndex(string s) {
            return varTable[s];
        }

        public int GetVarLen() {
            return varTable.Count;
        }

        public void AddVar(string varName) {
            if (!varTable.Keys.Contains(varName)) {
                varTable[varName] = (varTable.Count + 1) * 4;
            }
        }
    }

    public class RootNode {
        private List<AstNode> _childrenNodes;

        public RootNode() {
            _childrenNodes = new List<AstNode>();
        }

        public void AddChild(AstNode child) {
            _childrenNodes.Add(child);
        }

        public List<AstNode> GetChildren() {
            return _childrenNodes;
        }
    }
}