
using K8S;
using K8S.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault.Secrets.Models;
using Microsoft.Azure.Management.ContainerService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Rest;

namespace K8Module.KubeClient
{
    protected class Global
    {
        readonly var KUBECONFIG = Environment.GetEnvironmentVariable("KUBECONFIG");
        readonly var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeConfigPath);
        readonly var client = new Kubernetes(config);
        readonly var token = config.AccessToken;
    }


}


namespace K8Module
{
    // Get-AKSCredentials;
    [Cmdlet(VerbsCommon.Get, "AKSCredentials")]
    public class GetAksCredentials : PSCmdlet
    {
        private global::System.String resourceGroupName;

        [Parameter(Mandatory = true)]
        public string ResourceGroupName { get => resourceGroupName; set => resourceGroupName = value; }

        [Parameter(Mandatory = true)]
         public string AKSClusterName { get; set; }
         
        protected override void ProcessRecord()
        {
            var azAks = GetAzAksCluster(ResourceGroupName, AKSClusterName);
            SetAzAksKubectlContext(azAks);
            var kubeConfigPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".kube", "config");
            WriteObject(kubeConfigPath);
        }

        private AzAksCluster GetAzAksCluster(string resourceGroupName, string aksClusterName)
        {
            // Logic to retrieve AKS cluster using Azure SDK
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            var azure = Azure.Configure().Authenticate(credentials).WithDefaultSubscription();
            var azAks = azure.AksClusters.GetByResourceGroup(resourceGroupName, aksClusterName);
            return azAks;
        }

        private void SetAzAksKubectlContext(AzAksCluster azAks)
        {
            // Logic to set AKS kubectl context using Azure SDK
            var kubeConfigContent = azAks.GetAdminKubeConfigContent();
            var kubeConfigPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".kube", "config");
            File.WriteAllText(kubeConfigPath, kubeConfigContent);
        }
    }


    // New-K8SHPAs
    [Cmdlet(VerbsCommon.New, "K8SHPAs")]
    public class NewK8Shpas : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string TargetDeployment { get; set; }

        [Parameter(Mandatory = true)]
        public int MinPods { get; set; }

        [Parameter(Mandatory = true)]
        public int MaxPods { get; set; }

        [Parameter(Mandatory = true)]
        public int TargetCPUUtilizationPercentage { get; set; }

        protected override void ProcessRecord()
        {
            var body = new
            {
                apiVersion = "autoscaling/v2beta2",
                kind = "HorizontalPodAutoscaler",
                metadata = new
                {
                    name = Name,
                    namespace = Namespace
                },
                spec = new
                {
                    scaleTargetRef = new
                    {
                        apiVersion = "apps/v1",
                        kind = "Deployment",
                        name = TargetDeployment
                    },
                    minReplicas = MinPods,
                    maxReplicas = MaxPods,
                    metrics = new[]
                    {
                        new
                        {
                            type = "Resource",
                            resource = new
                            {
                                name = "cpu",
                                target = new
                                {
                                    type = "Utilization",
                                    averageUtilization = TargetCPUUtilizationPercentage
                                }
                            }
                        }
                    }
                }
            }

            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            }

            var uri = $"https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/{Namespace}/horizontalpodautoscalers";
            var response = InvokeRestMethod(uri, "POST", headers, body);

            WriteObject(response);
        }
    }
    // Get-K8SHPAs
    [Cmdlet(VerbsCommon.Get, "K8SHPAs")]
    public class GetK8Shpas : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {Global.token}" }
            };

            var uri = $"https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/{Namespace}/horizontalpodautoscalers";
            var response = HttpClientHelper.Get(uri, headers);

            // Process the response
            var items = response.items;
            foreach (var item in items)
            {
                // Process each HPA
            }
        }
    }
    // Skeleton for Update-K8SHPA
    [Cmdlet(VerbsData.Update, "K8SHPA")]
    public class UpdateK8Shpa : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            // Implementation to update an HPA
            var kubernetesClient = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
            var hpa = kubernetesClient.ReadNamespacedHorizontalPodAutoscaler(Name, Namespace);
            
            // Modify the properties of the hpa object here
            
            var updatedHpa = kubernetesClient.ReplaceNamespacedHorizontalPodAutoscaler(hpa, Name, Namespace);
            
            // Process the updated HPA object as needed
        }
    }
    // Remove-K8SHPA
    [Cmdlet(VerbsCommon.Remove, "K8SHPA")]
    public class RemoveK8Shpa : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {Global.token}" }
            };

            var uri = $"https://kubernetes.default.svc/apis/autoscaling/v2beta2/namespaces/{Namespace}/horizontalpodautoscalers/{Name}";
            var httpClient = new HttpClient();
            var response = httpClient.DeleteAsync(uri, headers).Result;

            // Handle the response here
        }
    }
    // End of Horizontal Autoscaling classes..
    // Start of ConfigMap classes:
    //
    // New-K8SConfigMap
    [Cmdlet(VerbsCommon.New, "K8SConfigMap")]
    public class NewK8SConfigMap : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string ConfigMapName { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable Data { get; set; }

        protected override void ProcessRecord()
        {
            var kubernetesClient = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
            var configMap = new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = ConfigMapName,
                    Namespace = Namespace
                },
                Data = Data
            };
            var createdConfigMap = kubernetesClient.CreateNamespacedConfigMap(configMap, Namespace);
        }
    }
    // Get-K8SConfigMap
    [Cmdlet(VerbsCommon.Get, "K8SConfigMap")]
    public class GetK8SConfigMap : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string ConfigMapName { get; set; }

        protected override void ProcessRecord()
        {
            string token = Global.token; // Assuming Global.token is accessible

            string authorizationHeader = $"Bearer {token}";
            string uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/configmaps/{ConfigMapName}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authorizationHeader);

            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            // Process the response body as needed
        }
    }
    // Start of Secret classes:
    //
    //
    // New-K8SSecret
    [Cmdlet(VerbsCommon.New, "K8SSecret")]
    public class NewK8SSecret : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string SecretName { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable Data { get; set; }

        protected override void ProcessRecord()
        {
            var body = new
            {
                apiVersion = "v1",
                kind = "Secret",
                metadata = new
                {
                    name = SecretName
                },
                type = "Opaque",
                data = Data
            };

            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.token);

            var uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/secrets";
            var response = await client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // Process the response as needed
            }
            else
            {
                // Handle the error case
            }
        }
    }
    // Get-K8SSecret
    [Cmdlet(VerbsCommon.Get, "K8SSecret")]
    public class GetK8SSecret : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string SecretName { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {Global.token}" }
            };

            var uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/secrets/{SecretName}";
            var response = HttpClientHelper.Get(uri, headers);

            // Optionally decode base64 data here if necessary for display or use
            WriteObject(response);
        }
    }
    // Start of ingress classes:
    //
    //
    // Skeleton for Get-K8SIngresses
    [Cmdlet(VerbsCommon.Get, "K8SIngresses")]
    public class GetK8SIngresses : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        protected override void ProcessRecord()
        {
            string token = "YOUR_TOKEN"; // Replace with your actual token
            string uri = $"https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/{Namespace}/ingresses";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = client.GetAsync(uri).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;
            // Process the response body as needed
        }
    }
    // New-K8SIngresses
    [Cmdlet(VerbsCommon.New, "K8SIngresses")]
    public class NewK8SIngresses : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public PSCustomObject IngressSpec { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = $"https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/{Namespace}/ingresses";
            var body = JsonConvert.SerializeObject(IngressSpec, Formatting.None);
            var response = HttpClientHelper.Post(uri, headers, body);

            WriteObject(response);
        }
    }
    // Update-K8SIngresses
    [Cmdlet(VerbsCommon.Update, "K8SIngresses")]
    public class UpdateK8SIngresses : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string IngressName { get; set; }

        [Parameter(Mandatory = true)]
        public PSCustomObject IngressSpec { get; set; }

        protected override void ProcessRecord()
        {
            string token = "$Global:token"; // Replace with the actual token
            string uri = $"https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/{Namespace}/ingresses/{IngressName}";
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(IngressSpec);
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            System.Net.Http.HttpResponseMessage response = client.PutAsync(uri, new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json")).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            WriteObject(responseContent);
        }
    }
    // Remove-K8SIngresses
    [Cmdlet(VerbsCommon.Remove, "K8SIngresses")]
    public class RemoveK8SIngresses : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string IngressName { get; set; }

        protected override void ProcessRecord()
        {
            string token = $Global:token; // Assuming you have the token available globally

            string uri = $"https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/{Namespace}/ingresses/{IngressName}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.DeleteAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                // Ingress successfully deleted
            }
            else
            {
                // Handle error response
            }
        }
    }
    // Start of AuthCOntext classes:
    //
    //
    // Skeleton for Set-K8AuthenticationHeader
    [Cmdlet(VerbsCommon.Set, "K8AuthenticationHeader")]
    protected override void ProcessRecord()
        {
            Global.K8AuthHeader = new Hashtable
            {
                { "Authorization", $"Bearer {Global.K8ModuleConfig.Token}" }
            };
        }
    }
    // Switch-K8Context
    [Cmdlet(VerbsCommon.Switch, "K8Context")]
    public class SwitchK8Context : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ContextName { get; set; }

        protected override void ProcessRecord()
        {
            // Load the kubeconfig using the Kubernetes client library
            var kubeConfig = Kubernetes.KubeConfig.LoadFromFile(Global.K8ModuleConfig.KubeConfigPath);

            // Find the context by name
            var context = kubeConfig.Contexts.FirstOrDefault(c => c.Name == ContextName);

            if (context != null)
            {
                Global.K8ModuleConfig.ApiEndpoint = context.Cluster.Cluster.Server;
                Global.K8ModuleConfig.Token = context.User.User.Token;
                SetK8AuthenticationHeader();
                WriteObject($"Switched to context: {ContextName}");
            }
            else
            {
                WriteObject($"Context not found: {ContextName}");
            }
        }
    }
    // Get-K8Token
    [Cmdlet(VerbsCommon.Get, "K8Token")]
    public class GetK8Token : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var token = Global.K8ModuleConfig.Token;
            WriteObject(token);
        }
    }
    // Start of ResourceMgMt classes:
    //
    //
    // Start of PodMgMt classes:
    //
    //
    // Skeleton for  Get-K8SPods
    [Cmdlet(VerbsCommon.Get, "K8SPods")] 
    public class GetK8SPods : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        protected override void ProcessRecord()
        {
            string accessToken = "your_access_token";
            string uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/pods";
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Skeleton for  New-K8SPod
    [Cmdlet(VerbsCommon.New, "K8SPods")] 
    public class NewK8SPod : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public PSCustomObject PodSpec { get; set; }

        protected override void ProcessRecord()
        {
            string accessToken = "your_access_token";
            string uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/pods";
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" },
                { "Content-Type", "application/json" }
            };

            var body = PodSpec.ConvertToJson();
            var response = HttpClientHelper.Post(uri, headers, body);

            WriteObject(response);
        }
    }
    // Start of SvcMgmt classes:
    //
    //
    // Skeleton for  New-K8SService
    [Cmdlet(VerbsCommon.New, "K8SServices")]
    public class NewK8SServices : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        [Parameter(Mandatory = true)]
        public string ServiceName { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable Selector { get; set; }

        [Parameter(Mandatory = true)]
        public string Type { get; set; }

        [Parameter(Mandatory = true)]
        public ArrayList Ports { get; set; }

        protected override void ProcessRecord()
        {
            // Constructing the service object
            var service = new k8s.Models.V1Service
            {
                ApiVersion = "v1",
                Kind = "Service",
                Metadata = new k8s.Models.V1ObjectMeta
                {
                    Name = ServiceName,
                    NamespaceProperty = Namespace
                },
                Spec = new k8s.Models.V1ServiceSpec
                {
                    Selector = Selector,
                    Type = Type,
                    Ports = new List<k8s.Models.V1ServicePort>()
                }
            };

            foreach (var port in Ports)
            {
                var servicePort = new k8s.Models.V1ServicePort
                {
                    Port = port.Port,
                    TargetPort = port.TargetPort,
                    Protocol = port.Protocol
                };
                service.Spec.Ports.Add(servicePort);
            }

            // Convert service object to JSON
            var json = JsonConvert.SerializeObject(service);

            // Create the Kubernetes service
            var token = "YOUR_KUBE_TOKEN"; // Replace with your actual token
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {token}" },
                { "Content-Type", "application/json" }
            };

            var uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/services";
            var response = HttpClientHelper.Post(uri, json, headers);

            // Handle the response as needed
            // ...

            // Return the response
            WriteObject(response);
        }
    }
    // Get-K8SServices
    [Cmdlet(VerbsCommon.Get, "K8SServices")]
    public class GetK8SServices : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = false)]
        public string ServiceName { get; set; }

        protected override void ProcessRecord()
        {
            string uriBase = "https://kubernetes.default.svc/api/v1/namespaces/" + Namespace + "/services";
            string uri = string.IsNullOrEmpty(ServiceName) ? uriBase : $"{uriBase}/{ServiceName}";

            // TODO: Implement the logic to retrieve services

            // Implementation to retrieve Services using Invoke-RestMethod or HttpClient
            //string token = $Global:token; // Assuming you have the token available globally
            //string uri = $"https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/{Namespace}/ingresses/{IngressName}";


            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//
            //HttpResponseMessage response = await client.DeleteAsync(uri);
//
            //if (response.IsSuccessStatusCode)
            //{
            //    // Ingress successfully deleted
            //}
            //else
            //{
            //    // Handle error response
            //}
        }
    }

    // Update-K8SService
    [Cmdlet(VerbsCommon.Update, "K8SService")]
    public class UpdateK8SService : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string ServiceName { get; set; }

        [Parameter(Mandatory = true)]
        public PSCustomObject ServiceSpec { get; set; }

        protected override void ProcessRecord()
        {
            string uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/services/{ServiceName}";

            // Convert ServiceSpec to JSON string using Newtonsoft.Json.JsonConvert.SerializeObject
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(ServiceSpec);
            
            // TODO: Implement the logic to update services
            // Implementation to update a Service using Invoke-RestMethod or HttpClient
        }
    }

    // Remove-K8SService
    [Cmdlet(VerbsCommon.Remove, "K8SService")]
    public class RemoveK8SService : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Namespace { get; set; } = "default";

        [Parameter(Mandatory = true)]
        public string ServiceName { get; set; }

        protected override void ProcessRecord()
        {
            string uri = $"https://kubernetes.default.svc/api/v1/namespaces/{Namespace}/services/{ServiceName}";

            // Implementation to delete a Service using Invoke-RestMethod or HttpClient

            // TODO: Implement the logic to delete services
        }
    }
    // Start of Pv classes:
    //
    //
    // New-K8SPV
    [Cmdlet(VerbsCommon.New, "K8SPV")]
    public class NewK8SPv : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var pvSpec = new
            {
                // Define your PV spec here
            };

            var body = JsonConvert.SerializeObject(pvSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/api/v1/persistentvolumes";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }

    // Get-K8SPV
    [Cmdlet(VerbsCommon.Get, "K8SPV")]
    public class GetK8SPv : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/api/v1/persistentvolumes";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of PVC classes:
    //
    //
    // New-K8SPVC
    [Cmdlet(VerbsCommon.New, "K8SPVC")]
    public class NewK8SPvc : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var pvcSpec = new
            {
                // Define your PVC spec here
            };

            var body = JsonConvert.SerializeObject(pvcSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/api/v1/persistentvolumeclaims";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }
    // Get-K8SPVC
    [Cmdlet(VerbsCommon.Get, "K8SPVC")]
    public class GetK8SPvc : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/api/v1/persistentvolumeclaims";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of Deployment classes:
    //
    // New-K8SDeployment
    [Cmdlet(VerbsCommon.New, "K8SDeployment")]
    public class NewK8SDeployment : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var deploymentSpec = new
            {
                // Define your deployment spec here
            };

            var body = JsonConvert.SerializeObject(deploymentSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/apps/v1/namespaces/" + Namespace + "/deployments";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }
    // Get-K8SDeploy
    [Cmdlet(VerbsCommon.Get, "K8SDeploy")]
    public class GetK8SDeploy : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/apps/v1/namespaces/" + Namespace + "/deployments";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of StatefulSet classes:
    //
    //
    // New-K8SStatefulSet
    [Cmdlet(VerbsCommon.New, "K8SStatefulSet")]
    public class NewK8SStatefulSet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var statefulSetSpec = new
            {
                // Define your StatefulSet spec here
            };

            var body = JsonConvert.SerializeObject(statefulSetSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/apps/v1/namespaces/" + Namespace + "/statefulsets";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }

    // Get-K8SStatefulSet
    [Cmdlet(VerbsCommon.Get, "K8SStatefulSet")]
    public class GetK8SStatefulSet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/apps/v1/namespaces/" + Namespace + "/statefulsets";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of Job classes:
    //
    //
    // New-K8SJob
    [Cmdlet(VerbsCommon.New, "K8SJob")]
    public class NewK8SJob : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var jobSpec = new
            {
                // Define your Job spec here
            };

            var body = JsonConvert.SerializeObject(jobSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/batch/v1/namespaces/" + Namespace + "/jobs";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }
    // Get-K8SJob
    [Cmdlet(VerbsCommon.Get, "K8SJob")]
    public class GetK8SJob : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/batch/v1/namespaces/" + Namespace + "/jobs";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of CronJob classes:
    //
    //
    // New-K8SCronJob
    [Cmdlet(VerbsCommon.New, "K8SCronJob")]
    public class NewK8SCronJob : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var cronJobSpec = new
            {
                // Define your CronJob spec here
            };

            var body = JsonConvert.SerializeObject(cronJobSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/batch/v1/namespaces/" + Namespace + "/cronjobs";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }
    // Get-K8SCronJob
    [Cmdlet(VerbsCommon.Get, "K8SCronJob")]
    public class GetK8SCronJob : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/batch/v1/namespaces/" + Namespace + "/cronjobs";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of Namespace classes:
    //
    //
    // New-K8SNamespace
    [Cmdlet(VerbsCommon.New, "K8SNamespace")]
    public class NewK8SNamespace : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var namespaceSpec = new
            {
                // Define your Namespace spec here
            };

            var body = JsonConvert.SerializeObject(namespaceSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/api/v1/namespaces";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }
    // Get-K8SNamespace
    [Cmdlet(VerbsCommon.Get, "K8SNamespace")]
    public class GetK8SNamespace : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/api/v1/namespaces";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of Role classes:
    //
    //
    // New-K8SRole
    [Cmdlet(VerbsCommon.New, "K8SRole")]
    public class NewK8SRole : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var roleSpec = new
            {
                // Define your Role spec here
            };

            var body = JsonConvert.SerializeObject(roleSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/namespaces/" + Namespace + "/roles";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }

    // Get-K8SRole
    [Cmdlet(VerbsCommon.
    Get, "K8SRole")]
    public class GetK8SRole : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/namespaces/" + Namespace + "/roles";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
    // Start of RoleBinding classes:
    //
    //
    // New-K8SRoleBinding
    [Cmdlet(VerbsCommon.New, "K8SRoleBinding")]
    public class NewK8SRoleBinding : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var roleBindingSpec = new
            {
                // Define your RoleBinding spec here
            };

            var body = JsonConvert.SerializeObject(roleBindingSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/namespaces/" + Namespace + "/rolebindings";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }

    // Get-K8SRoleBinding
    [Cmdlet(VerbsCommon.Get, "K8SRoleBinding")]
    public class GetK8SRoleBinding : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/namespaces/" + Namespace + "/rolebindings";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }

    // Start of ClusterRole classes:
    //
    //
    // New-K8SClusterRole
    [Cmdlet(VerbsCommon.New, "K8SClusterRole")]
    public class NewK8SClusterRole : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var clusterRoleSpec = new
            {
                // Define your ClusterRole spec here
            };

            var body = JsonConvert.SerializeObject(clusterRoleSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/clusterroles";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }

    // Get-K8SClusterRole
    [Cmdlet(VerbsCommon.Get, "K8SClusterRole")]
    public class GetK8SClusterRole : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/clusterroles";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }

    // Start of ClusterRoleBinding classes:
    //
    //
    // New-K8SClusterRoleBinding
    [Cmdlet(VerbsCommon.New, "K8SClusterRoleBinding")]
    public class NewK8SClusterRoleBinding : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var clusterRoleBindingSpec = new
            {
                // Define your ClusterRoleBinding spec here
            };

            var body = JsonConvert.SerializeObject(clusterRoleBindingSpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/clusterrolebindings";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }

    // Get-K8SClusterRoleBinding
    [Cmdlet(VerbsCommon.Get, "K8SClusterRoleBinding")]
    public class GetK8SClusterRoleBinding : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token }
            };

            var uri = "https://kubernetes.default.svc/apis/rbac.authorization.k8s.io/v1/clusterrolebindings";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }

    // Start of NetworkPolicy classes:
    //
    //
    // New-K8SNetworkPolicy
    [Cmdlet(VerbsCommon.New, "K8SNetworkPolicy")]
    public class NewK8SNetworkPolicy : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            var networkPolicySpec = new
            {
                // Define your NetworkPolicy spec here
            };

            var body = JsonConvert.SerializeObject(networkPolicySpec);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + Global.token },
                { "Content-Type", "application/json" }
            };

            var uri = "https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/" + Namespace + "/networkpolicies";
            var response = HttpClientHelper.Post(uri, body, headers);

            WriteObject(response);
        }
    }


    // Get-K8SNetworkPolicy
    [Cmdlet(VerbsCommon.Get, "K8SNetworkPolicy")]
    public record GetK8SNetworkPolicy([property: Parameter(Mandatory = true)] string Name, [property: Parameter(Mandatory = true)] string Namespace) : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer " + Global.token }
                };

            var uri = "https://kubernetes.default.svc/apis/networking.k8s.io/v1/namespaces/" + Namespace + "/networkpolicies";
            var response = HttpClientHelper.Get(uri, headers);

            WriteObject(response.items);
        }
    }
}