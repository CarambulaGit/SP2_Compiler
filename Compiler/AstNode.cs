namespace Compiler {
    public abstract class AstNode : RootNode {
        public int Row { get; set; }

        public int Column { get; set; }

        protected AstNode(int row, int col) : base() {
            this.Row = row;
            Column = col;
        }
    }
}