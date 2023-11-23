using System.Diagnostics;
using Compilador.Calculator;
using Compilador.Graph;
using Compilador.IO;
using Compilador.Processors.Lexer;
using Compilador.Processors.Parser;
using Compilador.Quackier;

namespace Compilador
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string logo = @"
:'#######::'##::::'##::::'###:::::'######::'##:::'##:'####:'########:'########::
:##.... ##: ##:::: ##:::'## ##:::'##... ##: ##::'##::. ##:: ##.....:: ##.... ##:
:##:::: ##: ##:::: ##::'##:. ##:: ##:::..:: ##:'##:::: ##:: ##::::::: ##:::: ##:
:##:::: ##: ##:::: ##:'##:::. ##: ##::::::: #####::::: ##:: ######::: ########::
:##:'## ##: ##:::: ##: #########: ##::::::: ##. ##:::: ##:: ##...:::: ##.. ##:::
:##:.. ##:: ##:::: ##: ##.... ##: ##::: ##: ##:. ##::: ##:: ##::::::: ##::. ##::
:'##### ##:. #######:: ##:::: ##:. ######:: ##::. ##:'####: ########: ##:::. ##:
:.....:..:::.......:::..:::::..:::......:::..::::..::....::........::..:::::..::
          
                                            : +                   
                                      ::: :-++                   
                                    :: :::::--=+                  
                                  ::     :::-==++-                
                                  :     :#<@==+*+=)               
                                :::: :::#%@[++>**:               
                                ::::::::--=++::----:->*          
                                  ::::::---=+=:   -+<*+           
                                  ::::----===*+++*<]])            
                        ::         :----===+**=                  
                        ::---:        :-==+*><>=                  
                        :::::::       :  :-=*><>**=:              
                      :  :::::::::::::::::::::-==+*++-            
                      :  :: :::::::::::  :::::--==+++==           
                      :  ::   ::    ::-  :::--===++++=-          
                      : ::: :      ::--+-::---==++++===          
                      ::::::::::::::--=+*---===+++=====          
                        ::::::::::::--===+===========++=          
                        :::::::::-----====-=-=====++++           
                          :::::------------=====++*>+            
                            -+]*+=======++++**>><)#[              
                                  :=+>)][}}}[:       
                  
                   ";

      Console.WriteLine(logo);

      if (args.Length != 0 && args[0] == "-what?")
      {
        Console.WriteLine("Usage: dotnet run Compilador.dll ([option] [argument])*");
        Console.WriteLine("Options:");
        Console.WriteLine("\t-what?\t\tShow this help.");
        Console.Write("\t-i\t\tInput text for the compiler to tokenize and parse.");
        Console.WriteLine(" For this to work, the option (-dl and -dp) or (-sl -sp) must be specified.");
        Console.Write("\t-sl\t\tReads the lexer from the specified serialized datafile path.");
        Console.WriteLine(" For this to work, the option -i must be specified.");
        Console.Write("\t-dl\t\tReads the lexer from the specified lexer datafile path.");
        Console.WriteLine(" For this to work, the option -so must be specified.");
        Console.Write("\t-slo\t\tSpecify the file in which the lexer will be serialized.");
        Console.WriteLine(" For this to work, the option -dl must be specified.");
        Console.Write("\t-sp\t\tReads the parser from the specified serialized datafile path.");
        Console.WriteLine(" For this to work, the option -i must be specified.");
        Console.Write("\t-dp\t\tReads the parser from the specified parser datafile path.");
        Console.WriteLine(" For this to work, the option -spo must be specified.");
        Console.Write("\t-spo\t\tSpecify the file in which the parser will be serialized.");
        Console.WriteLine(" For this to work, the option -dp must be specified.");
        Console.WriteLine("\n\nRecommended usage for first use:\ndotnet Compilador.dll -dl [lexer datafile path] -slo [serialized lexer datafile path] -dp [parser datafile path] -spo [serialized parser datafile path]");
        Console.WriteLine("\nRecommended usage after the first processing:\ndotnet Compilador.dll -sl [serialized lexer datafile path] -sp [serialized parser datafile path] -i [input code file path]");

        Console.WriteLine("\nThe paths can be either absolute or relative to the current directory.");
        return;
      }

      var slIndex = Array.IndexOf(args, "-sl");
      var sloIndex = Array.IndexOf(args, "-slo");
      var iIndex = Array.IndexOf(args, "-i");
      var dlIndex = Array.IndexOf(args, "-dl");
      var spIndex = Array.IndexOf(args, "-sp");
      var spoIndex = Array.IndexOf(args, "-spo");
      var dpIndex = Array.IndexOf(args, "-dp");
      var dIndex = Array.IndexOf(args, "-debug");
      var dsIndex = Array.IndexOf(args, "-debugSerial");
      var demoIndex = Array.IndexOf(args, "-demo");

      try
      {
        if (slIndex != -1 && spIndex != -1 && iIndex != -1)
        {
          LexerIO lexerIO = new LexerIO(GetPath(args[slIndex + 1]));
          TokenStream? tokenStream = lexerIO.GetOutput(lexerIO.ReadFileContent(GetPath(args[iIndex + 1]))) as TokenStream;
          if (tokenStream == null)
            throw new Exception("Error in lexer, is not a TokenStream");
          ParserIO parserIO = new ParserIO(GetPath(args[spIndex + 1]));
          parserIO.WriteFileContent(tokenStream, GetPath(args[iIndex + 1]));
        }
        else if (dlIndex != -1 && sloIndex != -1 && dpIndex != -1 && spoIndex != -1 && iIndex == -1)
        {
          LexerIO lexerIO = new LexerIO(GetPath(args[dlIndex + 1]), GetPath(args[sloIndex + 1]));
          ParserIO parserIO = new ParserIO(GetPath(args[dpIndex + 1]), GetPath(args[spoIndex + 1]));
        }
        else if (dlIndex != -1 && sloIndex != -1 && dpIndex != -1 && spoIndex != -1 && iIndex != -1)
        {
          LexerIO lexerIO = new LexerIO(GetPath(args[dlIndex + 1]), GetPath(args[sloIndex + 1]));
          TokenStream? tokenStream = lexerIO.GetOutput(lexerIO.ReadFileContent(GetPath(args[iIndex + 1]))) as TokenStream;
          if (tokenStream == null)
            throw new Exception("Error in lexer, is not a TokenStream");
          ParserIO parserIO = new ParserIO(GetPath(args[dpIndex + 1]), GetPath(args[spoIndex + 1]));
          parserIO.WriteFileContent(tokenStream, GetPath(args[iIndex + 1]));
        }
        else if (args.Length == 0 || dIndex != -1)
        {
          string basePath = AppDomain.CurrentDomain.BaseDirectory;
          basePath = basePath.Replace("/bin/Debug/net7.0", "");
          LexerIO lexerIO = new LexerIO(basePath + "test/simple_calculator/LexerDataOut.xml");
          ParserIO parserIO = new ParserIO(basePath + "test/simple_calculator/ParserDataOut.xml");

          while (true)
          {
            Console.Write("Enter an expression: ");
            string? expression = Console.ReadLine();
            if (expression == null || expression == "")
              break;

            Compiler.Compile(expression, (Lexer)lexerIO.Processor, (Parser)parserIO.Processor, basePath + "test/simple_calculator/operation");
            Execute($"nasm -f elf32 test/simple_calculator/operation.asm -o test/simple_calculator/operation.o", basePath);
            Execute($"gcc -m32 test/simple_calculator/operation.o -o test/simple_calculator/operation -no-pie", basePath);
            Console.WriteLine(Execute("./operation", $"{basePath}test/simple_calculator/"));
          }
        }
        else if (dsIndex != -1)
        {
          string basePath = AppDomain.CurrentDomain.BaseDirectory;
          basePath = basePath.Replace("/bin/Debug/net7.0", "");
          LexerIO lexerIO = new LexerIO(basePath + "test/syntax_test/LexerDataOut.xml");
          lexerIO.WriteFileContent(lexerIO.ReadFileContent(basePath + "test/syntax_test/Code.clsr"), basePath + "test/syntax_test/Code.clsr");
          TokenStream? tokenStream = lexerIO.GetOutput(lexerIO.ReadFileContent(basePath + "test/syntax_test/Code.clsr")) as TokenStream;
          if (tokenStream == null)
            throw new Exception("Error in lexer, is not a TokenStream");
          ParserIO parserIO = new ParserIO(basePath + "test/syntax_test/ParserDataOut.xml");
          parserIO.WriteFileContent(tokenStream, basePath + "test/syntax_test/Code.clsr");
          Console.WriteLine("Correct syntax!");
          (Tree, ParserSetup)? parseData = parserIO.GetOutput(parserIO.ReadFileContent(basePath + "test/syntax_test/Code.clsr")) as (Tree, ParserSetup)?;
          if (parseData == null)
            throw new Exception("Error in parser, is not a (Tree, ParserSetup)");
          TypeChecker typeChecker = new TypeChecker(parseData.Value.Item1, parseData.Value.Item2);
          typeChecker.CheckTypes();
        }
        else if (demoIndex != -1)
        {
          string basePath = AppDomain.CurrentDomain.BaseDirectory;
          basePath = basePath.Replace("/bin/Debug/net7.0", "");
          LexerIO lexerIO = new LexerIO(basePath + "test/syntax_test/LexerDataOut.xml");
          // Wait for the user to press a key
          Console.WriteLine("\nLexer ready: Press any key to continue...");
          Console.ReadKey();
          ParserIO parserIO = new ParserIO(basePath + "test/syntax_test/ParserDataOut.xml");
          // Wait for the user to press a key
          Console.WriteLine("\nParser ready: Press any key to continue...");
          Console.ReadKey();
          // For each file in the demo folder
          foreach (string filePath in Directory.GetFiles(basePath + "test/demo_code/"))
          {
            if (filePath.EndsWith(".qckr"))
            {
              try
              {
                // If the file is a .clsr file

                // Read the file
                string fileContent = lexerIO.ReadFileContent(filePath);
                // Print the file content
                Console.WriteLine("File: " + Path.GetFileName(filePath));
                // Wait for the user to press a key
                Console.WriteLine("Press any key to continue...\n");
                Console.ReadKey();
                Console.WriteLine(fileContent);
                // Wait for the user to press a key
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                // Get the token stream
                TokenStream? tokenStream = lexerIO.GetOutput(fileContent) as TokenStream;
                if (tokenStream == null)
                  throw new Exception("Error in lexer, is not a TokenStream");
                // Write the file
                parserIO.WriteFileContent(tokenStream, filePath);

              }
              catch (Exception e)
              {
                Console.WriteLine(e.Message);
              }
              finally
              {
                // Wait for the user to press a key
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
              }
            }
          }
        }
        else
        {
          Console.WriteLine("Invalid arguments. Use -what? for help.");
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    private static string GetPath(string path)
    {
      if (Path.IsPathRooted(path))
        return path;
      else
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    private static string Execute(string command, string workingDirectory)
    {
      Process process = new Process();
      process.StartInfo.FileName = "/bin/bash";
      process.StartInfo.Arguments = $"-c \"{command}\"";
      process.StartInfo.WorkingDirectory = workingDirectory;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;

      process.Start();
      string output = process.StandardOutput.ReadToEnd();
      string error = process.StandardError.ReadToEnd();
      process.WaitForExit();

      if (error != "")
        throw new Exception(error);

      return output;
    }
  }
}