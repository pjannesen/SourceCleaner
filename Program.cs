using System;

namespace Jannesen.Tools.SourceCleaner
{
    class Program
    {
        static      void         Main(string[] args)
        {
            try {
                var globbing = (Globbing)null;
                var cleaner  = new SourceCleaner();

                foreach(var arg in args) {
                    if (arg.StartsWith("--", StringComparison.Ordinal)) {
                        var i = arg.IndexOf('=', 2);
                        var name  = (i > 0) ? arg.Substring(2, i-2) : arg.Substring(2);
                        var value = (i > 0) ? arg.Substring(i+1) : null;

                        switch(name) {
                        case "path":
                            if (String.IsNullOrWhiteSpace(value))
                                throw new FormatException("Invalid path value");

                            if (globbing != null) {
                                cleaner.Run(globbing);
                            }

                            globbing = value != null ? new Globbing(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), value)) : null;
                            break;

                        default:
                            cleaner.SetOption(name, value);
                            break;
                        }
                    }
                    else {
                        if (globbing == null) {
                            globbing = new Globbing(System.IO.Directory.GetCurrentDirectory());
                        }

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
