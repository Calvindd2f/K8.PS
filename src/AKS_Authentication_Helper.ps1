# AKS Authentication Helper Function
function Get-AKSCredentials {
    param (
        [string]$ResourceGroupName,
        [string]$AKSClusterName
    )

    $azAks = Get-AzAksCluster -ResourceGroupName $ResourceGroupName -Name $AKSClusterName
    $azAks | Set-AzAksKubectlContext -Force
    $kubeConfigPath = [System.IO.Path]::Combine($env:USERPROFILE, '.kube', 'config')
    return $kubeConfigPath
}

# Updated AKS Authentication in Module Script
$kubeConfigPath = Get-AKSCredentials -ResourceGroupName 'YourResourceGroup' -AKSClusterName 'YourClusterName'
$config = KubernetesClientConfiguration.BuildConfigFromConfigFile($kubeConfigPath)
$client = new k8s.Kubernetes($config)
