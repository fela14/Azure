using System;
using System.Threading.Tasks;
using Azure; // For WaitUntil
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

class Program
{
    static async Task Main()
    {
        string subscriptionId = "eb6c9e17-84c8-413e-96df-a52c431f868c";
        string resourceGroupName = "myrg";
        string storageAccountName = "mystorageacctblob12345";
        string location = "eastus";

        // Authenticate using Azure CLI or environment credentials
        var credential = new DefaultAzureCredential();
        var armClient = new ArmClient(credential);

        // Get the subscription resource (✅ Fixed here)
        var subscription = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscriptionId));

        // Create the resource group
        var rgData = new ResourceGroupData(location);
        var rgLro = await subscription.GetResourceGroups()
            .CreateOrUpdateAsync(WaitUntil.Completed, resourceGroupName, rgData);
        var resourceGroup = rgLro.Value;

        // Create the storage account
        var storageSku = new StorageSku(StorageSkuName.StandardLrs);
        var parameters = new StorageAccountCreateOrUpdateContent(storageSku, StorageKind.BlobStorage, location)
        {
            AccessTier = StorageAccountAccessTier.Hot
        };

        var storageLro = await resourceGroup.GetStorageAccounts()
            .CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, parameters);
        var storageAccount = storageLro.Value;

        Console.WriteLine($"✅ Storage account '{storageAccount.Data.Name}' created in resource group '{resourceGroupName}'");
    }
}
