namespace Compiler {
    class Program {
        // todo fix path to code file
        public static readonly string Code = System.IO.File.ReadAllText(@"C:\Study\SP_course_work\toParse.py");
        public const string FOUR_SPACES = "    ";
        // todo add Extensions.cs
        private static void Main(string[] args) {
            // todo to funcs
            var codeWithSpaces = Code.Replace("\t", FOUR_SPACES);
            var lexer = new Lexer(codeWithSpaces); // todo tokenizer
            lexer.Tokenize();
            lexer.PrintTokens();
            var parser = new Parser(lexer.GetTokens());
            parser.Parse();
            var asmCodeGenerator = new AsmCodeGenerator(parser.GetAstTree());
            asmCodeGenerator.GenerateAsm();
        }
    }
}