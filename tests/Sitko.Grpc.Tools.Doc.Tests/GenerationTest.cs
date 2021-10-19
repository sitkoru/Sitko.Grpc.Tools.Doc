using System;
using System.IO;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sitko.Core.Xunit;
using Xunit;
using Xunit.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace Sitko.Grpc.Tools.Doc.Tests
{
    public class GenerationTest : BaseTest<GenerationTestScope>
    {
        public GenerationTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task GenerateDocsAsync()
        {
            var scope = await GetScopeAsync();
            var engine = scope.GetService<FakeBuildEngine>();
            string workingDirectory = Environment.CurrentDirectory;
            string protoDirectory = Path.Combine(workingDirectory, "Proto");
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var osDir = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "windows_x64",
                PlatformID.Unix => "linux_x64",
                PlatformID.MacOSX => "macosx_x64",
                _ => throw new InvalidOperationException($"Unsupported platform: {Environment.OSVersion.Platform}")
            };
            var protocExe = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "protoc.exe",
                PlatformID.Unix => "protoc",
                PlatformID.MacOSX => "protoc",
                _ => throw new InvalidOperationException($"Unsupported platform: {Environment.OSVersion.Platform}")
            };
            var protocGenDocExe = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => "protoc-gen-doc.exe",
                PlatformID.Unix => "protoc-gen-doc",
                PlatformID.MacOSX => "protoc-gen-doc",
                _ => throw new InvalidOperationException($"Unsupported platform: {Environment.OSVersion.Platform}")
            };
            string toolsDirectory =
                Path.Combine(projectDirectory, "..", "..", "src", "Sitko.Grpc.Tools.Doc", "Tools", osDir);
            string protoIncludeDirectory =
                Path.Combine(projectDirectory, "..", "..", "src", "Sitko.Grpc.Tools.Doc", "Proto");
            string outputDir = Path.Combine(workingDirectory, "docs");
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }

            Directory.CreateDirectory(outputDir);
            var task = new ProtoDocCompile
            {
                ToolExe = Path.Combine(toolsDirectory, protocExe),
                PluginExe = Path.Combine(toolsDirectory, protocGenDocExe),
                OutputDir = outputDir,
                Protobuf = new ITaskItem[]
                {
                    new TaskItem(Path.Combine(protoDirectory,
                        "Sitko/Grpc/Tools/Doc/Tests/DocsTestService.proto")),
                },
                BuildEngine = engine,
                ProtoPath = new[] { protoDirectory, protoIncludeDirectory }
            };
            task.Execute();

            var docFile = Path.Combine(outputDir, "index.html");
            File.Exists(docFile).Should().BeTrue();

            var html = await File.ReadAllTextAsync(docFile);
            html.Should().Contain("AccountingService");
        }
    }

    public class GenerationTestScope : BaseTestScope
    {
        protected override IServiceCollection ConfigureServices(IConfiguration configuration,
            IHostEnvironment environment,
            IServiceCollection services, string name)
        {
            base.ConfigureServices(configuration, environment, services, name);
            services.AddSingleton<FakeBuildEngine>();
            return services;
        }
    }

    public class FakeBuildEngine : IBuildEngine
    {
        private readonly ILogger<FakeBuildEngine> _logger;

        public FakeBuildEngine(ILogger<FakeBuildEngine> logger)
        {
            _logger = logger;
        }

        public bool BuildProjectFile(
            string projectFileName, string[] targetNames,
            System.Collections.IDictionary globalProperties,
            System.Collections.IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public int ColumnNumberOfTaskNode
        {
            get { return 0; }
        }

        public bool ContinueOnError
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int LineNumberOfTaskNode
        {
            get { return 0; }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            _logger.LogInformation("Custom: {Message}", e.Message);
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            _logger.LogError("{Message}", e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            _logger.LogInformation("{Message}", e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            _logger.LogWarning("{Message}", e.Message);
        }

        public string ProjectFileOfTaskNode
        {
            get { return "fake ProjectFileOfTaskNode"; }
        }
    }
}
diff --git a/.github/workflows/main.yml b/.github/workflows/main.yml
