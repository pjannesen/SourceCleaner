using System;

namespace Jannesen.Tools.SourceCleaner
{
    class Program
    {
        static      void         Main(string[] args)
        {
            try {
                var globbing = new Globbing(System.IO.Directory.GetCurrentDirectory());
                var cleaner  = new SourceCleaner();

                foreach(var arg in args) {
                    if (arg.StartsWith("--")) {
                        var i = arg.IndexOf('=', 2);
                        if (i > 0) {
                            cleaner.SetOption(arg.Substring(2, i-2), arg.Substring(i+1));
                        }
                        else {
                            cleaner.SetOption(arg.Substring(2), null);
                        }
                    }
                    else {
                        globbing.Pattern(arg);
                    }
                }

                cleaner.Run(globbing);
            }
            catch(Exception err) {
                while (err != null) {
                    System.Diagnostics.Debug.WriteLine("ERROR: " + err.Message);
                    Console.WriteLine("ERROR: " + err.Message);
                    err = err.InnerException;
                }
            }
        }
    }
}
