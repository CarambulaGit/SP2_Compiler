using System.Collections.Generic;

namespace Compiler {
    public static class Constants {
        public const string FOUR_SPACES = "    ";
        public const string INPUT_FILE_NAME = @"\KP-8-C#-IO-91-Didenko.py";
        public const string OUTPUT_FILE_NAME = @"\KP-8-C#-IO-91-Didenko.asm";

        public static readonly List<char> SYMBOLS = new List<char>() {
            '(', ')', '*', ',', '-', '/', ':', '=', '>', '!',
        };

        public const string PROTO_ASM = "{0} PROTO\n";
        public const string ASSIGN_STATEMENT_ASM =
            "{0}\n\tpop eax\n\tmov dword ptr[ebp{1}], eax\n";
        public const string IF_STATEMENT_ASM = "{0}" +
                                               "pop eax\n" +
                                               "cmp eax, 0\n" +
                                               "je {1}else\n" +
                                               "{2}" +
                                               "{1}else:\n";
        public const string ELSE_STATEMENT_ASM = "{0}" +
                                                 "pop eax\n" +
                                                 "cmp eax, 0\n" +
                                                 "je {1}else\n" +
                                                 "{2}" +
                                                 "jmp {1}final\n" +
                                                 "{1}else:\n" +
                                                 "{3}" +
                                                 "{1}final:\n";

        public const string PRINT_STATEMENT_ASM =
            "{0}fn MessageBoxA,0, str$(eax), \"Didenko Vladyslav IO-91\", MB_OK\n";
        public const string WHILE_STATEMENT_ASM = "Loop{0}start:\n" +
                                                  "{1}" +
                                                  "pop eax\n" +
                                                  "cmp eax, 0\n" +
                                                  "je Loop{0}end\n" +
                                                  "{2}" +
                                                  "jmp Loop{0}start\n" +
                                                  "Loop{0}end:\n";
        public const string EXPRESSION_STATEMENT_ASM = "{0}\n";
        public const string PROCEDURE_ASM = "{0} PROC\n" +
                                            "{1}\n" +
                                            "{0} ENDP\n";
        public const string MASM_CODE_TEMPLATE = ".386\n" + ".model flat,stdcall\n" + "option casemap:none\n\n" +
                                                 @"include \masm32\include\masm32rt.inc" + "\n_main PROTO\n\n" +
                                                 "{0}\n" +
                                                 ".data\n" + ".code\n" + "_start:\n" + "push ebp\n" + "mov ebp, esp\n" +
                                                 "sub esp, {3}\n" + "invoke  _main\n" + "add esp, {3}\n" +
                                                 "mov esp, ebp\n" +
                                                 "pop ebp\n" + "ret\n" + "_main PROC\n\n" + "{1}\n" + 
                                                 "printf(\"\\n\")\n" + "inkey\n" + "ret\n\n" + "_main ENDP\n\n" +
                                                 "{2}" +
                                                 "END _start\n\n";
        public const string FUNCTION_BODY_ARGUMENTS_ASM = "ret {0}\n";
        public const string VAR_EXPRESSION_ASM = "mov eax, dword ptr[ebp{0}] ; {1}\npush eax\n";
        public const string DIVIDE_ASM = "{0}\n{1}\npop eax\npop ebx\nxor edx, edx\ndiv ebx\npush eax\n";
        public const string MULTIPLY_ASM = "{0}\n{1}\npop eax\npop ecx\nimul ecx\npush eax\n";
        public const string CALL_EXPRESSION_ASM = "invoke {0}\n push eax\n";
        public const string NOT_EQUAL_ASM =
            "{0}\n{1}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nsetne al\npush eax\n";
        public const string GREATER_ASM = "{0}\n{1}\npop eax\npop ecx\ncmp ecx, eax\nmov eax, 0\nsetl al\npush eax\n";
        public const string CONDITION_BODY_ASM = "{0}\njmp {1}final\n";
        public const string SUBSTRACT_ASM = "{0}\n{1}\npop eax\npop ecx\nsub eax, ecx\npush eax\n";
        public const string CONDITION_ELSE_ASM = "{0}else:\n{1}\n";
        public const string VARIABLE_ASM = "push ebp\nmov ebp, esp\nsub esp, {0}\n";
        public const string FUNCTION_BODY_PARAMS_ASM = "add esp, {0}\nmov esp, ebp\npop ebp\n";
        public const string CONST_ASM = "\tpush {0}\n";
        public const string RETURN_ASM = "{0}\npop eax\nadd esp, {1}\nmov esp, ebp\npop ebp\nret {2}\n";
        public const string CONDITION_IF_ASM = "{0}\npop eax\ncmp eax, 0\nje {1}final\n";
        public const string CONDITION_IF_WITH_ELSE_ASM = "{0}\npop eax\ncmp eax, 0\nje {1}else\n";
        public const string ID_ASM = @"{0}final:\n";

        public const string PYTHON_PRINT = "print";
        public const string PYTHON_WHILE = "while";
        public const string PYTHON_IF = "if";
        public const string PYTHON_ELSE = "else";
        public const string PYTHON_RETURN = "return";
        public const string PYTHON_FUNCTION_DEFENITION = "def";
    }
}