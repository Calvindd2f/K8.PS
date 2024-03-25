# The dotnet client library is actively in use and needs to be utilized within powershell. It is located at src/depends/Kubernetes.dll

# Import the Kubernetes client library
Add-Type -Path "src/depends/Kubernetes.dll"
Add-Type -Path "src/depends/KubernetesClient.dll"
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

    # Load the kubeconfig using the Kubernetes client library
    $kubeConfig = [Kubernetes.KubeConfig]::LoadFromFile($Global:K8ModuleConfig.KubeConfigPath)

    # Find the context by name
    $context = $kubeConfig.Contexts | Where-Object { $_.Name -eq $ContextName }

    if ($context) {
        $Global:K8ModuleConfig.ApiEndpoint = $context.Cluster.Cluster.Server
        $Global:K8ModuleConfig.Token = $context.User.User.Token
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