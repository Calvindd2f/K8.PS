## In Kubernetes C# Client folder

>> Source: https://github.com/kubernetes-client/csharp

`dotnet new classlib`
`dotnet add package KubernetesClient`
`dotnet tool install --global Microsoft.VisualStudio.SlnGen.Tool`
`dotnet msbuild /t:slngen`

# PowerShell just for admin functions and copying the source folder from github
`try{IWR "https://github.com/kubernetes-client/csharp/archive/refs/tags/v13.0.26.zip" -Outfile .\v13.0.26.zip}catch{throw "error";exit};if(test-path ".\v13.0.26.zip"){expand-archive .\v13.0.26.zip} ;rename-item .\csharp-13.0.26\ kubernetes-client_csharp;rm .\v13.0.26.zip`

# Compiling the C# project into a .NET Core assembly (dll) for usage in the powershell module.
`.\compile_csproj.ps1 -Path ".\KubeOps.Operator.PowerShell"`

