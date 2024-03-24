#Creating a Horizontal Pod Autoscaler
function New-K8sHPA {
    param (
        [string]$name,
        [string]$namespace = "default",
        [string]$targetDeployment,
        [int]$minPods,
        [int]$maxPods,
        [int]$targetCPUUtilizationPercentage
    )

    $body = @{
        apiVersion = "autoscaling/v2beta2"
        kind = "HorizontalPodAutoscaler"
        metadata = @{
            name = $name
            namespace = $namespace
        }
        spec = @{
            scaleTargetRef = @{
                apiVersion = "apps/v1"
                kind = "Deployment"
                name = $targetDeployment
            }
            minReplicas = $minPods
            maxReplicas = $maxPods
            metrics = @(
                @{
                    type = "Resource"
                    resource = @{
                        name = "cpu"
                        target = @{
                            type = "Utilization"
                            averageUtilization = $targetCPUUtilizationPercentage
                        }
                    }
                }
            )
        }
    } | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/$namespace/horizontalpodautoscalers"
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

#Listing Horizontal Pod Autoscalers
function Get-K8sHPAs {
    param (
        [string]$namespace = "default"
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/$namespace/horizontalpodautoscalers"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response.items
}
#Updating a Horizontal Pod Autoscaler
function Update-K8sHPA {
    param (
        [string]$name,
        [string]$namespace = "default",
        [PSCustomObject]$updatedHPASpec
    )

    $body = $updatedHPASpec | ConvertTo-Json -Depth 10

    $headers = @{
        Authorization = "Bearer $Global:token"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/$namespace/horizontalpodautoscalers/$name"
    $response = Invoke-RestMethod -Uri $uri -Method Put -Headers $headers -Body $body

    return $response
}
#Deleting a Horizontal Pod Autoscaler
function Remove-K8sHPA {
    param (
        [string]$name,
        [string]$namespace = "default"
    )

    $headers = @{
        Authorization = "Bearer $Global:token"
    }

    $uri = "https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/$namespace/horizontalpodautoscalers/$name"
    $response = Invoke-RestMethod -Uri $uri -Method Delete -Headers $headers

    return $response
}
<#
Considerations
Metrics Server: Ensure the Kubernetes Metrics Server is deployed and running in your cluster, as HPAs require metrics to function correctly.
RBAC Permissions: The account or service principal used needs adequate permissions to manage HPAs within the namespace.
Validation and Error Handling: Implement comprehensive validation and error handling to manage API request failures gracefully and provide clear user feedback.
#>