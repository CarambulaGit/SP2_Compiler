using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compiler {
    class Program {
        private static readonly string PythonCode = File.ReadAllText(string.Format("{0}{1}", Directory.GetParent(
            Directory.GetCurrentDirectory()), Constants.INPUT_FILE_NAME));

        private static void Lexing(string code, out List<Token> tokens) {
            var codeWithSpaces = code.Replace("\t", Constants.FOUR_SPACES);
            var lexer = new Lexer(codeWithSpaces);
            lexer.Tokenize();
            tokens = lexer.Tokens;
        }

        private static void Parsing(List<Token> tokens, out AstTree astTree) {
            var parser = new Parser(tokens);
            parser.Parse();
            astTree = parser.AstTree;
        }

        private static void AsmCodeGenerator(AstTree astTree, out string asmCode) {
            var asmCodeGenerator = new AsmCodeGenerator(astTree);
            asmCodeGenerator.GenerateAsm();
            asmCode = asmCodeGenerator.AsmCode;
        }

        private static void WriteCodeToFile(string asmCode) {
            using var fs = File.Create(Constants.OUTPUT_FILE_NAME);
            var bytes = new UTF8Encoding(true).GetBytes(asmCode);
            fs.Write(bytes, 0, bytes.Length);
        }

        private static void Main(string[] args) {
            Lexing(PythonCode, out var tokens);
            Parsing(tokens, out var astTree);
            AsmCodeGenerator(astTree, out var asmCode);
            WriteCodeToFile(asmCode);
        }
    }
}