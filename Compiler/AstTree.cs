using System;
using System.Collections.Generic;
using System.Linq;
using static Compiler.Statement;

namespace Compiler {
    public class AstTree : INamespace {
        public readonly RootNode Root;
        public Dictionary<string, int> varTable { get; set; } = new Dictionary<string, int>();

        public List<DefStatement> FuncList { get; set; }
        public int VarCounter { get; set; }

        public AstTree() {
            Root = new RootNode();
            FuncList = new List<DefStatement>();
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