using System.Collections.Generic;

namespace Compiler {
    public class Ast {
        public int Row { get; set; }

        public int Column { get; set; }
        private readonly List<Ast> _children = new List<Ast>();

        public void AddChild(Ast child) {
            _children.Add(child);
        }

        public List<Ast> GetChildren() {
            return _children;
        }


        public Ast(int row, int col) {
            _children = new List<Ast>();
            Row = row;
            Column = col;
        }

        public Ast() { }
    }
}