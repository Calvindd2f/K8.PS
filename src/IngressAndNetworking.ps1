#Listing Ingress Resources
function Get-K8sIngresses {
    param (
        [string]$namespace = "default"
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/$namespace/ingresses"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response.items
}

#Creating an Ingress Resource
function New-K8sIngress {
    param (
        [string]$namespace = "default",
        [PSCustomObject]$ingressSpec
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/$namespace/ingresses"
    $body = $ingressSpec | ConvertTo-Json -Depth 10
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

#Updating an Ingress Resource
function Update-K8sIngress {
    param (
        [string]$namespace = "default",
        [string]$ingressName,
        [PSCustomObject]$ingressSpec
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/$namespace/ingresses/$ingressName"
    $body = $ingressSpec | ConvertTo-Json -Depth 10
    $response = Invoke-RestMethod -Uri $uri -Method Put -Headers $headers -Body $body

    return $response
}

#Deleting an Ingress Resource
function Remove-K8sIngress {
    param (
        [string]$namespace = "default",
        [string]$ingressName
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/$namespace/ingresses/$ingressName"
    $response = Invoke-RestMethod -Uri $uri -Method Delete -Headers $headers

    return $response
}

#Considerations

<# Notes #>
<#  
Parameterization: Consider extending these functions to support additional parameters for more complex Ingress configurations, such as TLS settings and multiple rules.
Validation and Error Handling: Implement comprehensive validation and error handling to ensure robust interaction with the Kubernetes API.
Security: Be mindful of the security implications of managing Ingress resources, especially concerning exposing services to external traffic.

#>

