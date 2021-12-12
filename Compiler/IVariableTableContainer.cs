using System.Collections.Generic;

namespace Compiler {
    public interface IVariableTableContainer
    {
        Dictionary<string, int> varTable { get; set; }
        
        List<DefStatement> FuncList { get; set; }

        public bool HaveFunction(string f);

        public void AddFunction(DefStatement f);

        public DefStatement GetFunctionWithName(string f);

        public bool HaveVariable(string v);

        public int GetVarIndex(string s);

        public int GetVarLen();

        public void AddVar(string varName);
    }
}