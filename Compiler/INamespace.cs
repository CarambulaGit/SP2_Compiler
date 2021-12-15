using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    public interface INamespace {
        Dictionary<string, int> Variables { get; set; }

        List<FuncStatement> FuncList { get; set; }
        
        public int VarCounter { get; set; }

        public void AddVariable(string varName) {
            if (Variables.ContainsKey(varName)) return;
            VarCounter++;
            var indexes = Variables.Keys.ToList();
            foreach (var index in indexes.Where(index => Variables[index] > 0)) {
                Variables[index] += 4;
            }

            Variables[varName] = 4;
        }
        
        public FuncStatement GetFuncByName(string f) =>
            FuncList.Find(func => func.Name == f) ?? throw new SyntaxException($"Function {f} not found");

        public bool ThereIsFuncWithName(string f) => FuncList.Any(func => func.Name == f);
    }
}