function New-KubeService {
    param (
        [string]$Namespace,
        [string]$ServiceName,
        [hashtable]$Selector,
        [string]$Type,
        [System.Collections.ArrayList]$Ports
    )

    # Constructing the service object
    $service = New-Object k8s.Models.V1Service
    $service.ApiVersion = "v1"
    $service.Kind = "Service"
    $service.Metadata = New-Object k8s.Models.V1ObjectMeta
    $service.Metadata.Name = $ServiceName
    $service.Metadata.NamespaceProperty = $Namespace
    $service.Spec = New-Object k8s.Models.V1ServiceSpec
    $service.Spec.Selector = $Selector
    $service.Spec.Type = $Type
    $service.Spec.Ports = @()

    foreach ($port in $Ports) {
        $servicePort = New-Object k8s.Models.V1ServicePort
        $servicePort.Port = $port.Port
        $servicePort.TargetPort = $port.TargetPort
        $servicePort.Protocol = $port.Protocol
        $service.Spec.Ports += $servicePort
    }

    
}
#Creating a Kubernetes Service
function New-K8sService {
    param (
        [string]$namespace = "default",
        [PSCustomObject]$serviceSpec
    )

    $body = $serviceSpec | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/services"
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

#Retrieving Kubernetes Services
function Get-K8sServices {
    param (
        [string]$namespace = "default",
        [string]$serviceName = $null
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uriBase = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/services"
    $uri = if ($serviceName) { "$uriBase/$serviceName" } else { $uriBase }

    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response
}
#Updating a Kubernetes Service
function Update-K8sService {
    param (
        [string]$namespace = "default",
        [string]$serviceName,
        [PSCustomObject]$serviceSpec
    )

    $body = $serviceSpec | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/services/$serviceName"
    $response = Invoke-RestMethod -Uri $uri -Method Put -Headers $headers -Body $body

    return $response
}
#Deleting a Kubernetes Service
function Remove-K8sService {
    param (
        [string]$namespace = "default",
        [string]$serviceName
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/services/$serviceName"
    $response = Invoke-RestMethod -Uri $uri -Method Delete -Headers $headers

    return $response
}


<#notes#>
<#
Considerations
Service Specifications: The $serviceSpec parameter in the New-K8sService and Update-K8sService functions should be a PSCustomObject that correctly maps to the Kubernetes Service manifest structure. Users of your module will need to construct these objects according to their service configurations.
Security and Permissions: Ensure that the account or token used to authenticate these requests has the necessary permissions to manage services within the specified namespace.
Validation and Error Handling: Incorporate comprehensive validation and error handling to provide clear feedback for any issues encountered during service management operations.
#>


