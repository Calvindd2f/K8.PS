#Managing ConfigMaps

##Creating a ConfigMap
function New-K8sConfigMap {
    param (
        [string]$namespace = "default",
        [string]$configMapName,
        [hashtable]$data
    )

    $body = @{
        apiVersion = "v1"
        kind = "ConfigMap"
        metadata = @{ name = $configMapName }
        data = $data
    } | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/configmaps"
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}
##Retrieving a ConfigMap
function Get-K8sConfigMap {
    param (
        [string]$namespace = "default",
        [string]$configMapName
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/configmaps/$configMapName"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response
}


#Managing Secrets
##Creating a Secret
function New-K8sSecret {
    param (
        [string]$namespace = "default",
        [string]$secretName,
        [hashtable]$data # Note: Data must be base64 encoded
    )

    $body = @{
        apiVersion = "v1"
        kind = "Secret"
        metadata = @{ name = $secretName }
        type = "Opaque"
        data = $data
    } | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/secrets"
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

##Retrieving a Secret
function Get-K8sSecret {
    param (
        [string]$namespace = "default",
        [string]$secretName
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/secrets/$secretName"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    # Optionally decode base64 data here if necessary for display or use
    return $response
}


<# Notes #>
<#
Considerations
Data Encoding: Remember that data in Secrets must be base64 encoded before being sent to the Kubernetes API and decoded when retrieved.
Security: Be cautious with handling and displaying sensitive information retrieved from Secrets. Ensure that such data is handled securely in your scripts and applications.
Validation and Error Handling: Incorporate validation and error handling to ensure robust and user-friendly interactions with the Kubernetes API.
Updating Resources: Similar to creating resources, updating ConfigMaps and Secrets can be achieved by using the HTTP PUT method with the appropriate resource URI and body content.
#>