using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Nnn.ApplicationCore.Services.BlobService
{
    public class BlobService : IBlobService
    {
        private readonly IOptions<MyConfig> _config;
        public BlobService(IOptions<MyConfig> config)
        {
            _config = config;
        }
        private string GetImageName()
        {
            return Guid.NewGuid().ToString().Replace("-", "") + ".jpg";
        }
        public async Task<string> InsertFile(IFormFile asset)
        {
            string containerUrl = "saallacontainer";
            string url =
                "DefaultEndpointsProtocol=https;AccountName=saallaaccount;AccountKey=UQ2MqW7o5U0b+JDIblqxC8X9dphmnrVbVFONn+XjnqJkKSUcY8htXmeJvrpRsHIyv8KBNdH7EGMHiSNsOn4ypw==;EndpointSuffix=core.windows.net";
            try
            {
                var imageName = GetImageName();
                if (CloudStorageAccount.TryParse(url, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference(containerUrl);

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

                    await blockBlob.UploadFromStreamAsync(asset.OpenReadStream());

                    return imageName;
                }
                else
                {
                    return "YUKLENNEMEDI";
                }
            }
            catch
            {
                return "YUKLENEMEDI";
            }
        }

        public static string GetImageUrl(string path)
        {
            return "https://saallaaccount.blob.core.windows.net/saallacontainer/" + path;
        }
    }
}