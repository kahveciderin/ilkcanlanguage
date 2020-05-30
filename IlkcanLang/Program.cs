using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace IlkcanLang
{
    class MainClass
    {

        static string csprj = String.Empty;
        static List<List<string>> lexiled = new List<List<string>>();
        public static void Main(string[] args)
        {


            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();

            Console.WriteLine("IlkcanLang Compiler v1.0\n\n\n\tIlkcanLang Compiler comes with absolutely NO WARRANTY,\n\tto the extent permitted by applicable law\n" +
            	"\n\nInput file: "+args[0]+"\n" +"Output file: Program.exe\n"+
            	"Including dependencies...");
            csprj += "using System;\nusing System.Collections;\nusing System.Collections.Generic;\nnamespace Program\n{\n\tpublic class MainClass\n\t{\n\t\tpublic static void Main(string[] args)\n\t\t{\n\t\t\t\n";
            Console.WriteLine("Adding lists...");
            lexiled.Add(new List<string>());
            lexiled.Add(new List<string>());
            Console.WriteLine("Reading file...");
            try { 
            Lexile(File.ReadAllText(args[0]));
            Console.WriteLine("Creating code...");
            Interpret();
            Console.WriteLine("Finalizing code...");
            csprj += "\n\t\t}\n\t}\n}";
            Console.WriteLine("Writing code to file...");
            File.WriteAllText("csprj.cs", csprj);

            Console.WriteLine("Initalizing compiler...");
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            Console.WriteLine("Generating parameters...");
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, "Program.exe", true);
            parameters.GenerateExecutable = true;
            Console.WriteLine("Compiling...");
            CompilerResults results = csc.CompileAssemblyFromSource(parameters,csprj);
            Console.WriteLine("Deleting temp file...\n");
            File.Delete("csprj.cs");
            Console.WriteLine("Looking for errors...\n");
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText));
            Console.WriteLine("\n\n");
            if(results.Errors.Cast<CompilerError>().ToList().Count > 0)
            {
                Console.WriteLine("Compiler exitted with {0} error(s).", results.Errors.Cast<CompilerError>().ToList().Count);
                Console.WriteLine("Compiling FAILED! Time elapsed: {0}ms",watch.ElapsedMilliseconds);
            }
            else
            {
                Console.WriteLine("DONE! Time elapsed: {0}ms", watch.ElapsedMilliseconds);
            }
            }
            catch
            {
                Console.WriteLine("ERROR!");
            }
        }



        static void Interpret()
        {
            int k = 0;
            List<List<List<string>>> part = new List<List<List<string>>>();
            part.Add(new List<List<string>>());
            part[0].Add(new List<string>());
            part[0].Add(new List<string>());
            int llength = 0;

            for (int im = 0; im < lexiled[0].Count; im++)
            {

                if (lexiled[0][im] != "EOL")
                {
                    part[k][0].Add(lexiled[0][im]);
                    part[k][1].Add(lexiled[1][im]);
                }
                else
                {
                    k++;
                    part.Add(new List<List<string>>());
                    part[k].Add(new List<string>());
                    part[k].Add(new List<string>());

                }

                llength = k - 1;
            }

            //we have a list of 2 lists, splitted by EOL's.
            int i = 0;
            int a = 0;
            for (i = 0; i < llength; i++)
            {

                csprj += "\t\t\t";
                List<List<string>> line = part[i];

                string csline = String.Empty;
                string cache = String.Empty;

                bool isnumvar = false;
                for (a = 0; a < line[0].Count; a++)
                {
                    string def = line[0][a];
                    string cmd = line[1][a];
                    //Console.WriteLine("{0}\t{1}", def, cmd);
                    switch (def)
                    {

                        case "NUM":
                            csline += "\"" + cmd + "\""; 
                            break;
                        case "OP":
                            if(cmd == ">")
                            {
                                csline = "(" + csline + ")";
                                if(line[0].Count == a + 1)
                                {
                                    csline =  "Console.Write(" + csline +")";
                                }
                            }
                            else if(cmd == "<")
                            {
                                if(line[0].Count == a + 1)
                                {
                                    csline +=  "Console.ReadLine()";
                                }
                            }
                            else if (cmd == ";")
                            {
                                csline += " + ";
                            }
                            break;
                        case "VAR":

                            if (cmd[0] == '#')
                            {

                                if (cmd[cmd.Length - 1] == '.')
                                {
                                    var temp = csline;
                                    csline = "int " + cmd.Remove(cmd.Length - 1).Remove(0,1) + " = Int32.Parse(" + csline;
                                    isnumvar = true;
                                }else if (cmd[cmd.Length - 1] == ',')
                                {

                                    csline = cmd.Remove(cmd.Length - 1).Remove(0, 1) + " = Int32.Parse(" + csline; isnumvar = true;
                                }
                                else
                                {

                                    csline = cmd.Remove(0, 1) + "" + csline;
                                }
                            }
                            else
                            {
                                if (cmd[cmd.Length - 1] == '.')
                                {
                                    var temp = csline;
                                    csline = "string " + cmd.Remove(cmd.Length - 1).Remove(0, 1) + "=" + csline;
                                }
                                else if (cmd[cmd.Length - 1] == ',')
                                {

                                    csline = cmd.Remove(cmd.Length - 1).Remove(0, 1) + " = " + csline;
                                }
                                else
                                {

                                    csline = cmd.Remove(0, 1) + "" + csline;
                                }
                            }
                            break;
                        case "STR":
                            csline += "\"" + cmd.Substring(1, cmd.Length - 2) + "\"";
                            break;
                        case "PAR":
                            if(cmd == "(")
                            {
                                csline += "{";
                            }
                            else if (cmd == ")")
                            {
                                csline += "}";
                            }else if (cmd == "[")
                            {
                                while(line[1][a] != "]")
                                {
                                    a++;
                                    if(line[0][a] == "VAR") csline += line[1][a].Remove(0, 1);
                                    else if(line[0][a] == "STR") csline += "\"" + line[1][a].Substring(1, line[1][a].Length - 2) + "\"";
                                    else                    csline += line[1][a];
                                }
                                csline = csline.Remove(csline.Length - 1); 
                            }
                            else if (cmd == "]")
                            {
                            }
                            else if (cmd == "{")
                            {
                                csline += "(";
                            }
                            else if (cmd == "}")
                            {
                                csline += ")";
                            }
                            break;
                        case "CMD":
                            switch (cmd)
                            {
                                case "w":
                                    csline = "while" + csline;
                                    break;
                                case "y":
                                    csline = "if" + csline;
                                    break;
                                case "n":
                                    csline += "else";
                                    break;
                            }
                            break;
                    }


                }
                if(isnumvar)
                csprj += csline + "+\"\");\n";
                else
                csprj += csline + ";\n";
            }




        }


        static void Lexile(string code)
        {
            Console.WriteLine("Lexiling...");
            List<List<string>> codeparsed = new List<List<string>>();
            foreach (var items in Regex.Split(code, "\n"))
            {
                codeparsed.Add(Regex.Split(items, "(\\/.*\\/)? (\\/.*\\/)?").Where(x => !string.IsNullOrEmpty(x)).ToList());
            }


            foreach (var items in codeparsed)
            {
                foreach (var item in items)
                {
                    if (Regex.IsMatch(item, "^\\d*$")) lexadd("NUM", item);
                    else if (Regex.IsMatch(item, "^[<>,;:]$")) lexadd("OP", item);
                    else if (Regex.IsMatch(item, "^[@#].*$")) lexadd("VAR", item);
                    else if (Regex.IsMatch(item, "^\\/.*\\/$")) lexadd("STR", item);
                    else if (Regex.IsMatch(item, "^\\.$")) lexadd("EOL", item);
                    else if (Regex.IsMatch(item, "^[\\[\\]\\(\\)\\{\\}]$")) lexadd("PAR", item);
                    else if (Regex.IsMatch(item, "^#.*$")) lexadd("CMT", item);
                    else if (Regex.IsMatch(item, "^[+\\-*\\/%&|\\^!]$")) lexadd("MATH", item);
                    else lexadd("CMD", item);

                }
                lexadd("EOL", "\n");
            }

            for (int g = 0; g < lexiled[0].Count; g++)
            {
                //Console.WriteLine("{0} {1}",lexiled[0][g],lexiled[1][g]);

            }


        }
        static void lexadd(string a, string b)
        {
            lexiled[0].Add(a);
            lexiled[1].Add(b);
        }

    }
}
