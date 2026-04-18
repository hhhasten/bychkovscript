using BychkovScript.CLI;

string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
string stdLibPath = Path.Combine(baseDirectory, "stdlib");
string mainScriptPath = Path.Combine(baseDirectory, "main.bs");

var runner = new ScriptRunner();

Console.WriteLine("--- BychkovScript ---");

runner.RunFromFiles(mainScriptPath, stdLibPath);