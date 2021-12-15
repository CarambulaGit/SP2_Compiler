using System;
using System.Collections.Generic;
using System.Linq;
using static Compiler.Statement;

namespace Compiler {
    public class AstTree : INamespace {
        public readonly RootNode Root;
        public int VarCounter { get; set; }

        public List<FuncStatement> FuncList { get; set; }
        public Dictionary<string, int> Variables { get; set; } = new Dictionary<string, int>();

        public void AddVariable(string varName) {
            if (!Variables.Keys.Contains(varName)) {
                Variables[varName] = (Variables.Count + 1) * 4;
            }
        }

        public AstTree() {
            Root = new RootNode();
            FuncList = new List<FuncStatement>();
        }
    }

    public class RootNode {
        private List<AstNode> _childrenNodes;

        public void AddChild(AstNode child) {
            _childrenNodes.Add(child);
        }

        public List<AstNode> GetChildren() {
            return _childrenNodes;
        }

        public RootNode() {
            _childrenNodes = new List<AstNode>();
        }
    }
}