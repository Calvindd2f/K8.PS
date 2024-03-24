#Managing Persistent Volumes
##Creating a Persistent Volume
function New-K8sPV {
    param (
        [PSCustomObject]$pvSpec
    )

    $body = $pvSpec | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/persistentvolumes"
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

##Listing Persistent Volumes
function Get-K8sPVs {
    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/api/v1/persistentvolumes"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response.items
}



#Managing Persistent Volume Claims (PVC)
##Creating a Persistent Volume Claim
function New-K8sPVC {
    param (
        [string]$namespace = "default",
        [PSCustomObject]$pvcSpec
    )

    $body = $pvcSpec | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/persistentvolumeclaims"
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

##Listing Persistent Volume Claims
function Get-K8sPVCs {
    param (
        [string]$namespace = "default"
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/persistentvolumeclaims"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response.items
}

<#
Considerations
Spec Construction: For both PV and PVC creation functions, the $pvSpec and $pvcSpec parameters must be constructed carefully by the caller to match the Kubernetes API's expected structure for these resources. This includes details like storage class, access modes, and capacity for PVCs, and for PVs, additional configurations like volume type and parameters.
Namespace for PVCs: Remember that PVCs are namespaced resources, whereas PVs are not. Ensure that your functions handle namespaces appropriately.
Security and Permissions: Ensure appropriate RBAC permissions are set for the user or token performing these operations, as managing storage components often requires elevated privileges.
Error Handling: Implement error handling to manage API request failures gracefully, providing meaningful feedback to the user.
#>