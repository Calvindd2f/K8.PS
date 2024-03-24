#Module Configuration and Initialization
function Set-K8ModuleConfiguration {
    param (
        [string]$ApiEndpoint,
        [string]$Token,
        [string]$KubeConfigPath
    )

    # Save the configuration to a user-specific or module-specific location
    $config = @{
        ApiEndpoint = $ApiEndpoint
        Token = $Token
        KubeConfigPath = $KubeConfigPath
    }

    $config | ConvertTo-Json | Set-Content -Path "$env:USERPROFILE\.k8ModuleConfig"
    
    Write-Host "Module configuration saved."
}
# Loading Module Configuration
function Get-K8ModuleConfiguration {
    $configPath = "$env:USERPROFILE\.k8ModuleConfig"
    if (Test-Path $configPath) {
        $config = Get-Content -Path $configPath | ConvertFrom-Json
        $Global:K8ModuleConfig = $config
        Write-Host "Module configuration loaded."
    }
    else {
        Write-Host "No module configuration found."
    }
}
#Utility Function to Set Global Authentication Header
function Set-K8AuthenticationHeader {
    $Global:K8AuthHeader = @{
        Authorization = "Bearer $($Global:K8ModuleConfig.Token)"
    }
}
#Kubernetes Context Switcher
function Switch-K8Context {
    param (
        [string]$ContextName
    )

    # This example assumes kubeconfig is formatted in a way that can be directly manipulated;
    # consider using a Kubernetes client library or `kubectl` command for a more robust approach.
    $kubeConfig = Get-Content -Path $Global:K8ModuleConfig.KubeConfigPath | ConvertFrom-Json
    $context = $kubeConfig.contexts | Where-Object { $_.name -eq $ContextName }
    
    if ($context) {
        $Global:K8ModuleConfig.ApiEndpoint = $context.cluster.cluster.server
        $Global:K8ModuleConfig.Token = $context.user.user.token
        Set-K8AuthenticationHeader
        Write-Host "Switched to context: $ContextName"
    }
    else {
        Write-Host "Context not found: $ContextName"
    }
}
#Logging and Debugging
function Write-K8Log {
    param (
        [string]$Message,
        [string]$Level = "INFO" # Other levels might be DEBUG, WARN, ERROR
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "$timestamp [$Level] $Message"
    Write-Host $logMessage
}
#Error Handling Wrapper
function Invoke-K8CommandSafely {
    param (
        [scriptblock]$ScriptBlock
    )

    try {
        . $ScriptBlock
    }
    catch {
        Write-K8Log -Message $_ -Level "ERROR"
        # Additional error handling logic here
    }
}