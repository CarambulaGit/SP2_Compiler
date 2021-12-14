using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    // todo abstract class namespace?
    public interface INamespace {
        // todo make varTable List<string>
        Dictionary<string, int> varTable { get; set; }

        List<DefStatement> FuncList { get; set; }
        
        public int VarCounter { get; set; }

        public bool ThereIsFuncWithName(string f) => FuncList.Any(func => func.Name == f);

        // todo there is no func with this name
        public DefStatement GetFuncByName(string f) =>
            FuncList.Find(func => func.Name == f) ?? throw new NullReferenceException();

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
}