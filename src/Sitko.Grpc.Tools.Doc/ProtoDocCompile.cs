using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sitko.Grpc.Tools.Doc
{
    public class ProtoDocCompile : ToolTask
    {
        /// <summary>
        /// Protobuf files to compile.
        /// </summary>
        [Required]
        public ITaskItem[] Protobuf { get; set; }

        /// <summary>
        /// Generated code directory. The generator property determines the language.
        /// Switch: --GEN-out= (for different generators GEN).
        /// </summary>
        [Required]
        public string OutputDir { get; set; }
        
        /// <summary>
        /// Codegen options. See also OptionsFromMetadata.
        /// Switch: --GEN_out= (for different generators GEN).
        /// </summary>
        public string[] DocOutputOptions { get; set; }
        
        [Required]
        public string PluginExe { get; set; }

        /// <summary>
        /// The directories to search for imports. Directories will be searched
        /// in order. If not given, the current working directory is used.
        /// Switch: --proto_path.
        /// </summary>
        public string[] ProtoPath { get; set; }
        
        static readonly TimeSpan s_regexTimeout = TimeSpan.FromMilliseconds(100);
        
        static readonly List<ErrorListFilter> s_errorListFilters = new List<ErrorListFilter>()
        {
            // Example warning with location
            //../Protos/greet.proto(19) : warning in column=5 : warning : When enum name is stripped and label is PascalCased (Zero),
            // this value label conflicts with Zero. This will make the proto fail to compile for some languages, such as C#.
            new ErrorListFilter
            {
                Pattern = new Regex(
                    pattern: "^(?'FILENAME'.+?)\\((?'LINE'\\d+)\\) ?: ?warning in column=(?'COLUMN'\\d+) ?: ?(?'TEXT'.*)",
                    options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                    matchTimeout: s_regexTimeout),
                LogAction = (log, match) =>
                {
                    int.TryParse(match.Groups["LINE"].Value, out var line);
                    int.TryParse(match.Groups["COLUMN"].Value, out var column);

                    log.LogWarning(
                        subcategory: null,
                        warningCode: null,
                        helpKeyword: null,
                        file: match.Groups["FILENAME"].Value,
                        lineNumber: line,
                        columnNumber: column,
                        endLineNumber: 0,
                        endColumnNumber: 0,
                        message: match.Groups["TEXT"].Value);
                }
            },

            // Example error with location
            //../Protos/greet.proto(14) : error in column=10: "name" is already defined in "Greet.HelloRequest".
            new ErrorListFilter
            {
                Pattern = new Regex(
                    pattern: "^(?'FILENAME'.+?)\\((?'LINE'\\d+)\\) ?: ?error in column=(?'COLUMN'\\d+) ?: ?(?'TEXT'.*)",
                    options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                    matchTimeout: s_regexTimeout),
                LogAction = (log, match) =>
                {
                    int.TryParse(match.Groups["LINE"].Value, out var line);
                    int.TryParse(match.Groups["COLUMN"].Value, out var column);

                    log.LogError(
                        subcategory: null,
                        errorCode: null,
                        helpKeyword: null,
                        file: match.Groups["FILENAME"].Value,
                        lineNumber: line,
                        columnNumber: column,
                        endLineNumber: 0,
                        endColumnNumber: 0,
                        message: match.Groups["TEXT"].Value);
                }
            },

            // Example warning without location
            //../Protos/greet.proto: warning: Import google/protobuf/empty.proto but not used.
            new ErrorListFilter
            {
                Pattern = new Regex(
                    pattern: "^(?'FILENAME'.+?): ?warning: ?(?'TEXT'.*)",
                    options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                    matchTimeout: s_regexTimeout),
                LogAction = (log, match) =>
                {
                    log.LogWarning(
                        subcategory: null,
                        warningCode: null,
                        helpKeyword: null,
                        file: match.Groups["FILENAME"].Value,
                        lineNumber: 0,
                        columnNumber: 0,
                        endLineNumber: 0,
                        endColumnNumber: 0,
                        message: match.Groups["TEXT"].Value);
                }
            },

            // Example error without location
            //../Protos/greet.proto: Import "google/protobuf/empty.proto" was listed twice.
            new ErrorListFilter
            {
                Pattern = new Regex(
                    pattern: "^(?'FILENAME'.+?): ?(?'TEXT'.*)",
                    options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                    matchTimeout: s_regexTimeout),
                LogAction = (log, match) =>
                {
                    log.LogError(
                        subcategory: null,
                        errorCode: null,
                        helpKeyword: null,
                        file: match.Groups["FILENAME"].Value,
                        lineNumber: 0,
                        columnNumber: 0,
                        endLineNumber: 0,
                        endColumnNumber: 0,
                        message: match.Groups["TEXT"].Value);
                }
            }
        };
        
        private new bool UseCommandProcessor { get; set; }


        protected override string GenerateFullPathToTool() => ToolExe;

        protected override string ToolName => Platform.IsWindows ? "protoc.exe" : "protoc";
        
        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        
     

        // Protoc chokes on BOM, naturally. I would!
        static readonly Encoding s_utf8WithoutBom = new UTF8Encoding(false);
        protected override Encoding ResponseFileEncoding => s_utf8WithoutBom;

        protected override string GenerateResponseFileCommands()
        {
            var cmd = new ProtocResponseFileBuilder();
            cmd.AddSwitchMaybe("plugin=protoc-gen-doc", PluginExe);
            cmd.AddSwitchMaybe("doc_out", TrimEndSlash(OutputDir));
            cmd.AddSwitchMaybe("doc_opt", DocOutputOptions);
            if (ProtoPath != null)
            {
                foreach (string path in ProtoPath)
                {
                    cmd.AddSwitchMaybe("proto_path", TrimEndSlash(path));
                }
            }
            cmd.AddSwitchMaybe("error_format", "msvs");
            foreach (var proto in Protobuf)
            {
                cmd.AddArg(proto.ItemSpec);
            }
            Console.WriteLine(cmd.ToString());
            return cmd.ToString();
        }
        
        // Main task entry point.
        public override bool Execute()
        {
            UseCommandProcessor = false;

            bool ok = base.Execute();
            if (!ok)
            {
                return false;
            }

            return true;
        }
        
        static string TrimEndSlash(string dir)
        {
            if (dir == null || dir.Length <= 1)
            {
                return dir;
            }
            string trim = dir.TrimEnd('/', '\\');
            // Do not trim the root slash, drive letter possible.
            if (trim.Length == 0)
            {
                // Slashes all the way down.
                return dir.Substring(0, 1);
            }
            if (trim.Length == 2 && dir.Length > 2 && trim[1] == ':')
            {
                // We have a drive letter and root, e. g. 'C:\'
                return dir.Substring(0, 3);
            }
            return trim;
        }
        
        class ProtocResponseFileBuilder
        {
            StringBuilder _data = new StringBuilder(1000);
            public override string ToString() => _data.ToString();

            // If 'value' is not empty, append '--name=value\n'.
            public void AddSwitchMaybe(string name, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _data.Append("--").Append(name).Append("=")
                        .Append(value).Append('\n');
                }
            }

            // Add switch with the 'values' separated by commas, for options.
            public void AddSwitchMaybe(string name, string[] values)
            {
                if (values?.Length > 0)
                {
                    _data.Append("--").Append(name).Append("=")
                        .Append(string.Join(",", values)).Append('\n');
                }
            }

            // Add a positional argument to the file data.
            public void AddArg(string arg)
            {
                _data.Append(arg).Append('\n');
            }
        };
        
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            foreach (ErrorListFilter filter in s_errorListFilters)
            {
                Match match = filter.Pattern.Match(singleLine);

                if (match.Success)
                {
                    filter.LogAction(Log, match);
                    return;
                }
            }

            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }

        
        // Called by the base class to log tool's command line.
        //
        // Protoc command file is peculiar, with one argument per line, separated
        // by newlines. Unwrap it for log readability into a single line, and also
        // quote arguments, lest it look weird and so it may be copied and pasted
        // into shell. Since this is for logging only, correct enough is correct.
        protected override void LogToolCommand(string cmd)
        {
            var printer = new StringBuilder(1024);

            // Print 'str' slice into 'printer', wrapping in quotes if contains some
            // interesting characters in file names, or if empty string. The list of
            // characters requiring quoting is not by any means exhaustive; we are
            // just striving to be nice, not guaranteeing to be nice.
            var quotable = new[] { ' ', '!', '$', '&', '\'', '^' };
            void PrintQuoting(string str, int start, int count)
            {
                bool wrap = count == 0 || str.IndexOfAny(quotable, start, count) >= 0;
                if (wrap) printer.Append('"');
                printer.Append(str, start, count);
                if (wrap) printer.Append('"');
            }

            for (int ib = 0, ie; (ie = cmd.IndexOf('\n', ib)) >= 0; ib = ie + 1)
            {
                // First line only contains both the program name and the first switch.
                // We can rely on at least the '--out_dir' switch being always present.
                if (ib == 0)
                {
                    int iep = cmd.IndexOf(" --");
                    if (iep > 0)
                    {
                        PrintQuoting(cmd, 0, iep);
                        ib = iep + 1;
                    }
                }
                printer.Append(' ');
                if (cmd[ib] == '-')
                {
                    // Print switch unquoted, including '=' if any.
                    int iarg = cmd.IndexOf('=', ib, ie - ib);
                    if (iarg < 0)
                    {
                        // Bare switch without a '='.
                        printer.Append(cmd, ib, ie - ib);
                        continue;
                    }
                    printer.Append(cmd, ib, iarg + 1 - ib);
                    ib = iarg + 1;
                }
                // A positional argument or switch value.
                PrintQuoting(cmd, ib, ie - ib);
            }

            base.LogToolCommand(printer.ToString());
        }
    }
    
    class ErrorListFilter
    {
        public Regex Pattern { get; set; }
        public Action<TaskLoggingHelper, Match> LogAction { get; set; }
    }
}
