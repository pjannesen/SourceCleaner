using System;
using System.Collections.Generic;
using System.IO;
using Minimatch;

namespace Jannesen.Tools.SourceCleaner
{
    internal sealed class Globbing
    {
        private readonly    string                              _cwd;
        private readonly    SortedDictionary<string, string>    _files;

        public              IReadOnlyCollection<string>         Files
        {
            get {
                return _files.Values;
            }
        }

        public                                                  Globbing(string cwd)
        {
            cwd = cwd.Replace('\\', '/');

            if (cwd.Length < 3 || cwd[1] != ':' || cwd[2] != '/')
                throw new FormatException("Invalid root.");

            if (!cwd.EndsWith("/", StringComparison.Ordinal))
                cwd += "/";

            if (!Directory.Exists(cwd))
                throw new DirectoryNotFoundException("Unknown directory '" + cwd + "'.");

            _cwd   = cwd;
            _files = new SortedDictionary<string, string>();
        }
        public              void                                Pattern(string pattern)
        {
            bool    not = pattern.StartsWith("!", StringComparison.Ordinal);

            if (not)
                pattern = pattern.Substring(1);

            pattern = _absolutePattern(_cwd, pattern);
            var f = pattern.IndexOfAny(new char[] { '*', '?', '[', '(', '{', '+' } );
            if (f < 0) {
                if (!not) {
                    if (File.Exists(pattern.Replace('/', '\\'))) {
                        var key = pattern.ToLowerInvariant();
                        if (!_files.ContainsKey(key))
                            _files.Add(key, pattern);
                    }
                }
                else {
                    var key = pattern.ToLowerInvariant();
                    if (_files.ContainsKey(key))
                        _files.Remove(key);
                }
            }
            else {
                f = pattern.LastIndexOf('/', f);
                if (f < 0)
                    throw new FormatException("Can't deterim root.");

                var root    = pattern.Substring(0, f);
                var matcher = new Minimatch.Minimatcher(pattern.Substring(f + 1));

                if (!not) {
                    foreach (var filename in Directory.EnumerateFiles(root.Replace('/', '\\'), "*", SearchOption.AllDirectories)) {
                        var key = filename.Replace('\\', '/').ToLowerInvariant();
                        if (!_files.ContainsKey(key) && matcher.IsMatch(key.Substring(f + 1)))
                            _files.Add(key, filename);
                    }
                }
                else {
                    root = (root + '/').ToLowerInvariant();
                    var toremove = new List<string>();
                    foreach(var key in _files.Keys) {
                        if (key.StartsWith(root, StringComparison.Ordinal)) {
                            if (matcher.IsMatch(key.Substring(f + 1)))
                                toremove.Add(key);
                        }
                    }

                    foreach(var key in toremove)
                        _files.Remove(key);
                }
            }

//            var glob = Glob.Parse();
        }

        private static      string                              _absolutePattern(string cwd, string pattern)
        {
            pattern = pattern.Replace('\\', '/');

            if (pattern.StartsWith("//", StringComparison.Ordinal))
                return pattern; // UNC Path

            if (pattern.Length > 2 && pattern[1] == ':') {
                if (pattern.Length < 3 || pattern[2] != '/')
                    throw new FormatException("Invalid pattern.");

                return pattern;  // Absolute path <drive>:/
            }

            if (pattern.StartsWith("/", StringComparison.Ordinal))
                return cwd.Substring(0,2) + pattern;

            for (;;) {
                if (pattern.StartsWith("../", StringComparison.Ordinal)) {
                    int i = cwd.LastIndexOf('/', cwd.Length - 1);
                    if (i < 0)
                        throw new FormatException("can't go below root.");

                    cwd = cwd.Substring(0, i+1);
                    pattern = pattern.Substring(3);
                    continue;
                }

                if (pattern.StartsWith("./", StringComparison.Ordinal)) {
                    pattern = pattern.Substring(2);
                    continue;
                }
                break;
            }

            return cwd + pattern;
        }
    }
}
