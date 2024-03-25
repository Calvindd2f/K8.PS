<# REMOVE
┌──(c㉿CALVIN)-[C:\Users\c\Documents\K8.PS\src]
└─PS> dir .\depends\ -Filter "*.dll"|select FullName^C
C:\Users\c\Documents\K8.PS\src\depends\IdentityModel.dll
C:\Users\c\Documents\K8.PS\src\depends\IdentityModel.OidcClient.dll
C:\Users\c\Documents\K8.PS\src\depends\KubernetesClient.dll
C:\Users\c\Documents\K8.PS\src\depends\Microsoft.Extensions.DependencyInjection.Abstractions.dll
C:\Users\c\Documents\K8.PS\src\depends\Microsoft.Extensions.DependencyInjection.dll
#>

function AssDebug($File) {
    $asm = [Reflection.Assembly]::LoadFrom($File)
    $k8types = $asm.GetExportedTypes()
    $k8types | Get-Member -Static
}
@(
    'C:\Users\c\Documents\K8.PS\src\depends\KubernetesClient.dll'
    'C:\Users\c\Documents\K8.PS\src\depends\IdentityModel.dll'
    'C:\Users\c\Documents\K8.PS\src\depends\IdentityModel.OidcClient.dll'
    'C:\Users\c\Documents\K8.PS\src\depends\Microsoft.Extensions.DependencyInjection.Abstractions.dll'
    'C:\Users\c\Documents\K8.PS\src\depends\Microsoft.Extensions.DependencyInjection.dll'
).ForEach{AssDebug $_}|Out-File -FilePath C:\Users\c\Documents\K8.PS\src\depends\Debug.txt -Append -Force
