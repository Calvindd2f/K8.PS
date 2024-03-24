# Documented in Issue #1


#region auth
# Set variables for API interaction
$appID = "$env:AppId" # Adapt as needed for Kubernetes
$tenantID = "$env:TenantId" # Adapt if applicable
$appSecret = "$env:AppSecret" # Adapt as needed for Kubernetes

function Get-K8sToken {
    param (
        [string]$scope = "https://kubernetes.default.svc" # Adapt scope as needed
    )

    # Body for token request, adapt as necessary
    $body = @{
        grant_type    = "client_credentials"
        client_id     = $appID
        client_secret = $appSecret
        scope         = $scope
    }

    # Token endpoint for Kubernetes, adapt as needed
    $tokenEndpoint = "https://login.kubernetes.example.com/oauth2/token"
    $response = Invoke-RestMethod -Uri $tokenEndpoint -Method Post -Body $body

    return $response.access_token
}

$accessToken = Get-K8sToken
$Token = (ConvertTo-SecureString -String  $accessToken -AsPlainText -Force)
$Global:token = $Token
#endregion auth



#region ListingPods
# Listing Pods
function Get-K8sPods {
    param (
        [string]$namespace = "default"
    )

    $headers = @{
        Authorization = "Bearer $accessToken"
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/pods"
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers

    return $response.items
}
#endregion ListingPods

#region podinteraction
function New-K8sPod {
    param (
        [string]$namespace = "default",
        [PSCustomObject]$podSpec
    )

    $headers = @{
        Authorization = "Bearer $accessToken"
        'Content-Type' = 'application/json'
    }

    $uri = "https://kubernetes.default.svc/api/v1/namespaces/$namespace/pods"
    $body = $podSpec | ConvertTo-Json -Depth 10
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    return $response
}

#endregion podinteraction

Export-ModuleMember -Function Get-K8sPods,Get-K8sToken,New-K8sPod

<# Notes #>
<#
Initial Draft. Below are some aspects to review.

* The token is stored in a file. This is a potential security risk. Consider using a secure storage mechanism.
- Authentication: The current implementation uses a static token. Consider using a more secure method, such as Azure AD authentication.
- The module is not signed. This may be a security concern. Consider signing the module to ensure integrity.
- There is no error handling in this script. Consider adding error handling to manage API request failures or unexpected results gracefully.
- Error handling in functions like Get-K8sPods and New-K8sPod may be more appropriate. Right now it just returns whatever comes back from the API.
- Error handling in functions: Each function should have its own try/catch block. If an error occurs within a function, it should be handled gracefully.
- Error handling in functions like Get-K8sPods and New-K8sPod should be more comprehensive. Right now it just returns whatever comes back from the API.
- Authentication should be handled in a more robust manner. Right now it just returns whatever comes back from the API.

Error Handling: Incorporate comprehensive error handling to manage API request failures or unexpected results gracefully.

Parameter Validation: Validate function parameters to ensure they meet the Kubernetes API's expectations.

Performance: Test for performance implications, especially in large clusters.

Security: Ensure secure storage and handling of credentials or tokens.
#>