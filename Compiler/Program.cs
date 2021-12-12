using System;

namespace Compiler {
    class Program {
        // todo fix path to code file
        public static readonly string Code = System.IO.File.ReadAllText(@"C:\Study\SP_course_work\toParse.py");
        public const string FOUR_SPACES = "    ";

        private static void Main(string[] args) {
            var codeWithSpaces = Code.Replace("\t", FOUR_SPACES);
            var l = new Lexer(codeWithSpaces);
            l.GetTokens();
            l.PrintTokens();
        }
    }
}