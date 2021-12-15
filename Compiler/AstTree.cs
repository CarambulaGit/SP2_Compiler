using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public class AstTree : INamespace {
        public readonly Ast Root;
        public int VarCounter { get; set; }

        public List<FuncStatement> FuncList { get; set; }
        public Dictionary<string, int> Variables { get; set; } = new Dictionary<string, int>();

        public void AddVariable(string varName) {
            if (!Variables.Keys.Contains(varName)) {
                Variables[varName] = (Variables.Count + 1) * 4;
            }
        }

        public AstTree() {
            Root = new Ast();
            FuncList = new List<FuncStatement>();
        }
    }
}