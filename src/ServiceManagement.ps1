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

    # Code to apply the service object to the cluster
}

# Similar patterns would be followed for Update-KubeService and Remove-KubeService
