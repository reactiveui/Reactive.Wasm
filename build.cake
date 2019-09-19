#load nuget:https://pkgs.dev.azure.com/dotnet/ReactiveUI/_packaging/ReactiveUI/nuget/v3/index.json?package=ReactiveUI.Cake.Recipe&prerelease

Environment.SetVariableNames();

// Whitelisted Packages
var packageWhitelist = new[] 
{ 
    MakeAbsolute(File("./src/System.Reactive.Wasm/System.Reactive.Wasm.csproj")),
};

var packageTestWhitelist = new FilePath[]
{
    MakeAbsolute(File("./src/System.Reactive.Wasm.Tests/System.Reactive.Wasm.Tests.csproj")),
};

BuildParameters.SetParameters(context: Context, 
                            buildSystem: BuildSystem,
                            title: "Reactive.Wasm",
                            whitelistPackages: packageWhitelist,
                            whitelistTestPackages: packageTestWhitelist,
                            artifactsDirectory: "./artifacts",
                            sourceDirectory: "./src");

ToolSettings.SetToolSettings(context: Context);

Build.Run();
