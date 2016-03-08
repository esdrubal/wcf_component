#tool nuget:?package=XamarinComponent

#addin nuget:?package=Cake.Xamarin
#addin nuget:?package=Cake.FileHelpers
#addin nuget:?package=Cake.Json

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// paths
var GitToolPath = EnvironmentVariable("GIT_EXE") ?? (IsRunningOnWindows() ? "C:\\Program Files (x86)\\Git\\bin\\git.exe" : "git");
var PwdPath = MakeAbsolute(File(".")).GetDirectory();
var DefaultMonoPrefix = (DirectoryPath)"/Library/Frameworks/Mono.framework/Versions/4.5.0/";
var MonoPrefix = FileExists(DefaultMonoPrefix.CombineWithFilePath("bin/mono")) ? DefaultMonoPrefix : PwdPath.Combine("mono/install");
var OutDir = PwdPath.Combine("bin/System.ServiceModel");
var TestDir = PwdPath.Combine("bin/Tests");

var BclDir = MonoPrefix.Combine("lib/mono/4.5");
var PclDir = BclDir.Combine("Facades");

if (!DirectoryExists(OutDir)) {
    CreateDirectory(OutDir);
}
if (!DirectoryExists(TestDir)) {
    CreateDirectory(TestDir);
}

// build bits
string[] IgnoreReferences = new [] { 
    "xunit", 
    "System.ServiceModel.Primitives",
    "System.ServiceModel.Http",
    "System.ServiceModel.Duplex",
    "System.ServiceModel.Security", 
    "System.ServiceModel.NetTcp"
};
string[] IgnoreTests = new [] {
    "Bridge",
    "WcfTestBridgeCommon",
    "Bridge.Build.Tasks",
    "CertificateCleanup",
    "WcfService"
};

string XunitArgs = "-lib:lib/xunit -r:xunit.core -r:xunit.assert -r:xunit.abstractions -r:xunit.execution.dotnet";
string TestArgs = "-r:System.Private.ServiceModel -r:UnitTests.Common -r:System.Runtime -r:System.Threading.Tasks -r:System.Runtime.Serialization " + XunitArgs;
string ScenarioArgs = "-r:System.Private.ServiceModel -r:Infrastructure.Common -r:ScenarioTests.Common.dll -r:System.Runtime -r:System.Threading.Tasks -r:System.Runtime.Serialization " + XunitArgs;

var BuildProject = new Action<string, DirectoryPath, DirectoryPath, string[]>((name, projectDir, outDir, extra) =>
{
    Information("Building {0}...", name);
    
    var refs = string.Empty;
    var csFiles = string.Empty;
    var extras = string.Join(" ", extra);
    
    // read from a project.json
    if (projectDir != null) {
        var csFileArray = GetFiles(projectDir.FullPath + "/**/*.cs");
        var projectJson = projectDir.CombineWithFilePath("project.json");
        var data = ParseJson(FileReadText(projectJson));
        
        IEnumerable<JToken> deps = data["dependencies"];
        IEnumerable<JToken> frames = data["frameworks"]["dnxcore50"]["dependencies"];
        
        if (frames != null) {
            deps = deps.Union(frames);
        }
        
        var refsArray = deps
            .Cast<JProperty>()
            .Where(p => p != null)
            .Select(p => p.Name)
            .Where(n => !IgnoreReferences.Contains(n))
            .Select(n => string.Format("-r:{0}", n))
            .ToArray();
        
        refs = string.Join(" ", refsArray);
        csFiles = string.Join(" ", csFileArray.Select(f => f.FullPath));
    }
    
    // build
    var args = string.Format(
        "-t:library /unsafe {0} -lib:{1} -lib:{2} -lib:{3} -out:{4} {5} {6}",
        extras, outDir, PclDir, BclDir, outDir.CombineWithFilePath(name), refs, csFiles);
    var exitCode = StartProcess("mcs", new ProcessSettings {
        Arguments = args,
    });
    
    if (exitCode != 0) {
        throw new Exception("mcs failed: " + args);
    }
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./bin");
});

Task("BuildMono")
    .WithCriteria (!FileExists(MonoPrefix.CombineWithFilePath("bin/mono")))
    .Does(() =>
{
    StartProcess(GitToolPath, "clone git@github.com:mono/mono.git");
    StartProcess("bash", new ProcessSettings {
        Arguments = "autogen.sh --disable-nls --prefix=" + MonoPrefix,
        WorkingDirectory = "mono"
    });
    StartProcess("make", new ProcessSettings {
        Arguments = "-j8",
        WorkingDirectory = "mono"
    });
    StartProcess("make", new ProcessSettings {
        Arguments = "install",
        WorkingDirectory = "mono"
    });
});

Task("Build")
    .IsDependentOn("BuildMono")
    .Does(() =>
{
    BuildProject(
        "System.Reflection.DispatchProxy.dll", 
        "./external/corefx/src/System.Reflection.DispatchProxy/src",
        OutDir,
        new [] { 
            "./src/System.Reflection.DispatchProxy/SR.cs",
        });
        
    var runtimeInfoSrc = "./external/corefx/src/System.Runtime.InteropServices.RuntimeInformation/src";
    BuildProject(
        "System.Runtime.InteropServices.RuntimeInformation.dll", 
        null,
        OutDir,
        new [] { 
            runtimeInfoSrc + "/RuntimeInformation.OSX.cs",
            runtimeInfoSrc + "/RuntimeInformation.cs",
            runtimeInfoSrc + "/Architecture.cs",
            runtimeInfoSrc + "/OSPlatform.cs",
            "./src/System.Runtime.InteropServices.RuntimeInformation/SR.cs",
        });
        
    BuildProject(
        "System.Private.ServiceModel.dll", 
        "./external/wcf/src/System.Private.ServiceModel/src",
        OutDir,
        new [] { 
            "-r:System.Runtime.Serialization.dll",
            "-d:FEATURE_CORECLR", 
            "./src/System.ServiceModel/SR.cs external/wcf/src/Common/src/System/NotImplemented.cs",
            "./external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketTaskExtensions.cs",
            "./external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketReceiveMessageFromResult.cs",
            "./external/corefx/src/System.Net.Sockets/src/System/Net/Sockets/SocketReceiveFromResult.cs",
        });
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
    NuGetPack("./nuget/Mono.System.Private.ServiceModel.nuspec", new NuGetPackSettings {
        OutputDirectory = "./bin",
        BasePath = "././"
    });
});

Task("BuildTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    CopyDirectory(OutDir, TestDir);

    var BuildTestDependency = new Action<string, DirectoryPath, string[]>((name, projectDir, extra) => {
        var args = new [] { XunitArgs };
        if (extra != null) {
            args = extra.Union(args).ToArray();
        }
        BuildProject(
            name,
            projectDir,
            TestDir,
            args);
    });

    BuildTestDependency("xunit.netcore.extensions.dll", "./external/buildtools/src/xunit.netcore.extensions", new [] { 
        "-r:System.Runtime -r:System.Threading.Tasks",
        "-d:FEATURE_CORECLR",
    });
    BuildTestDependency("UnitTests.Common.dll", "./external/wcf/src/System.Private.ServiceModel/tests/Common/Unit", new [] { 
        "-r:System.Private.ServiceModel -r:System.Runtime.Serialization",
    });

    var BuildTest = new Action<string, DirectoryPath, string[]>((name, projectDir, extra) => {
        var args = new [] { TestArgs };
        if (extra != null) {
            args = extra.Union(args).ToArray();
        }
        BuildProject(
            name,
            projectDir,
            TestDir,
            args);
    });

    BuildTest("System.Private.ServiceModel.Tests.dll", "./external/wcf/src/System.Private.ServiceModel/tests/Unit", null);
    BuildTest("System.ServiceModel.Primitives.Tests.dll", "./external/wcf/src/System.ServiceModel.Primitives/tests/", null);
    BuildTest("Infrastructure.Common.dll", "./external/wcf/src/System.Private.ServiceModel/tests/Common/Infrastructure/src", new [] { "./src/Infrastructure.Common/TestProperties.cs" });
    BuildTest("Infrastructure.Common.Tests.dll", "./external/wcf/src/System.Private.ServiceModel/tests/Common/Infrastructure/tests", new [] { "-r:Infrastructure.Common" });
    BuildTest("ScenarioTests.Common.dll", "./external/wcf/src/System.Private.ServiceModel/tests/Common/Scenarios", new [] { "-r:Infrastructure.Common" });
    BuildTest("System.ServiceModel.Duplex.Tests.dll", "./external/wcf/src/System.ServiceModel.Duplex/tests", null);
    BuildTest("System.ServiceModel.NetTcp.Tests.dll", "./external/wcf/src/System.ServiceModel.NetTcp/tests", null);
    BuildTest("System.ServiceModel.Http.Tests.dll", "./external/wcf/src/System.ServiceModel.Http/tests", null);
    BuildTest("System.ServiceModel.Security.Tests.dll", "./external/wcf/src/System.ServiceModel.Security/tests", null);

    var BuildScenario = new Action<string, string> ((name, projectDir) => {
        BuildProject(
            name,
            "./external/wcf/src/System.Private.ServiceModel/tests/Scenarios/" + projectDir,
            TestDir,
            new [] {
                "-r:System.Net.Http",
                ScenarioArgs,
            });
    });

    BuildScenario("Binding.Custom.Tests.dll", "Binding/Custom");
    BuildScenario("Binding.Http.Tests.dll", "Binding/Http");
    BuildScenario("Client.ChannelLayer.Tests.dll", "Client/ChannelLayer");
    BuildScenario("Client.ClientBase.Tests.dll", "Client/ClientBase");
    BuildScenario("Client.ExpectedExceptions.Tests.dll", "Client/ExpectedExceptions");
    BuildScenario("Client.TypedClient.Tests.dll", "Client/TypedClient");
    BuildScenario("Contract.Data.Tests.dll", "Contract/Data");
    BuildScenario("Contract.Fault.Tests.dll", "Contract/Fault");
    BuildScenario("Contract.Message.Tests.dll", "Contract/Message");
    BuildScenario("Contract.Service.Tests.dll", "Contract/Service");
    BuildScenario("Contract.XmlSerializer.Tests.dll", "Contract/XmlSerializer");
    BuildScenario("Encoding.Encoders.Tests.dll", "Encoding/Encoders");
    BuildScenario("Encoding.MessageVersion.Tests.dll", "Encoding/MessageVersion");
    BuildScenario("Extensibility.WebSockets.Tests.dll", "Extensibility/WebSockets");
    BuildScenario("Security.TransportSecurity.Tests.dll", "Security/TransportSecurity");
    
    var projectFiles = GetFiles("./external/wcf/src/**/*.csproj");
    var errorAssemblies = 0;
    foreach (var project in projectFiles) {
        var assemblyName = XmlPeek(project, "/ns:Project/ns:PropertyGroup/ns:AssemblyName/text()", new XmlPeekSettings {
            Namespaces = new Dictionary<string, string> {
                { "ns", "http://schemas.microsoft.com/developer/msbuild/2003" }
            }
        });
        if (assemblyName == null) {
            Information("Bad project: {0}", project);
        } else if (IgnoreReferences.Contains(assemblyName) || IgnoreTests.Contains(assemblyName)) {
            Information("Ignoring assembly: {0}", assemblyName);
        } else if (FileExists(TestDir.CombineWithFilePath(assemblyName + ".dll"))) {
            Information("Found assembly: {0}", assemblyName);
        } else {
            Error("Did not find assembly: {0}", assemblyName);
            errorAssemblies++;
        }
    }
    if (errorAssemblies > 0) {
        throw new FileNotFoundException("Did not find " + errorAssemblies + " of " + projectFiles.Count + " assemblies.");
    }
});

Task("Tests")
    .IsDependentOn("BuildTests")
    .Does(() =>
{
    CopyDirectory("./lib/xunit", TestDir);

    var tests = GetFiles(TestDir.FullPath + "/*.Tests.dll");
    foreach (var test in tests) {
        StartProcess(TestDir.CombineWithFilePath("xunit.console.exe"), test + " -notrait category=failing -notrait category=OuterLoop");
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .IsDependentOn("Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
