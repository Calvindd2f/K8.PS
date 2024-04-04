## In Kubernetes C# Client folder  

> Source: https://github.com/kubernetes-client/csharp  

`Update 29.03.2024` - the first update

# Binary PowerShell module in C# for managing K8s

It got a bit messy when trying to do pure pwsh so no point fighting upward battle.
Compiled the dlls and now focusing primarily on C# binary module.

PS7 because it is `%currentyear%`

PowerShell-based module will be kept and maintained however it is not a priority and thus; nor are the issues.

```powershell
#region Skeletonized
function Get-AKSCredentials {
function New-K8sHPA {
function Get-K8sHPAs {
function Update-K8sHPA {
function Remove-K8sHPA {
function New-K8sSecret {
function Get-K8sSecret {
#endregion Skeletonized
```

In the interest of clarity; by skeleton I do not mean this [guy](https://www.google.com/url?sa=i&url=https%3A%2F%2Ftwitter.com%2Fdguspodcast&psig=AOvVaw1jQBZkUjLo4bkv9KN4IBlJ&ust=1712288882934000&source=images&cd=vfe&opi=89978449&ved=0CBIQjRxqFwoTCPjHpafTp4UDFQAAAAAdAAAAABAJ)

```cs
using k8s;
using System.Management.Automation;

namespace K8Module.Classes
{
    [Cmdlet(VerbsCommon.Get, "AKSCredentials")]
    public class GetAKSCredentials : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            // Process AKS credentials
            WriteObject("AKS credentials retrieved successfully.");
        }
    }
    [Cmdlet(VerbsCommon.New, "K8sHPAs")]
    public class NewK8sHPAs : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            // Retrieve HPAs
        }
    }
    // Get-K8sHPAs
    [Cmdlet(VerbsCommon.Get, "K8sHPAs")]
    public class GetK8sHPAs : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            // Retrieve HPAs
        }
    }
    // Update-K8sHPA
    [Cmdlet(VerbsData.Update, "K8sHPA")]
    public class UpdateK8sHPA : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            // Update an HPA
        }
    }
    // Remove-K8sHPA
    [Cmdlet(VerbsCommon.Remove, "K8sHPA")]
    public class RemoveK8sHPA : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            // Remove an HPA
        }
    }
    // End of Horizontal Autoscaling classes..
    // Start of ConfigMap classes:
    //
    // New-K8sConfigMap
```

```powershell
#region TODOskeleton
function Set-K8ModuleConfiguration {
function Get-K8ModuleConfiguration {
function Set-K8AuthenticationHeader {
function Switch-K8Context {
function Write-K8Log {
function Invoke-K8CommandSafely {
function Invoke-K8CommandSafely {
function New-K8sConfigMap {
function Get-K8sConfigMap {
function Get-K8sIngresses {
function New-K8sIngress {
function Update-K8sIngress {
function Remove-K8sIngress {
function Set-K8AuthenticationHeader {
function Switch-K8Context {
function Write-K8Log {
function Get-KubeResource {
function New-KubeDeployment {
function Remove-KubeResource {
function Get-K8sToken {
function Get-K8sPods {
function New-K8sPod {
function New-KubeService {
function New-K8sService {
function Get-K8sServices {
function Update-K8sService {
function Remove-K8sService {
function New-K8sPV {
function Get-K8sPVs {
function New-K8sPVC {
function Get-K8sPVCs {
#endregion TODOskeleton

#region memedebugfunc
function init{				# as it sounds
function EnumAssMeth {		# Enumerate Assemblies Methods - I can do funny acronyms too
function main {				# Ash
function valid8{			# :(
#endregion memedebugfunc
```

## The functions all primarily use the following libraries  
Name : KubernetesClient.Aot.dll  
Name : KubernetesClient.Classic.dll  
Name : KubernetesClient.dll  
Name : KubernetesClient.Kubectl.dll  
Name : KubernetesClient.ModelConverter.dll  
Name : LibKubernetesGenerator.Automapper.dll  
Name : LibKubernetesGenerator.dll  
