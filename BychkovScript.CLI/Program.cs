using BychkovScript.CLI;

string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
string mainScriptPath = Path.Combine(baseDirectory, "main.bs");

var runner = new ScriptRunner();

Console.WriteLine("--- BychkovScript ---");

runner.RunMain(mainScriptPath);