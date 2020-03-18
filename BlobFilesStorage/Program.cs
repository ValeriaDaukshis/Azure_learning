using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace BlobFilesStorage
{
    class Program
    {
        static string AccountKey = "1QSAxS7dXYPHfUGBKS+OWojc4T7LxjLYzbM/vz8GU58/7V/o/AWYMZv216SS14kdcA8tzAUCPRrKSR3Rv7L0vA==";
        static string AccountName = "blobstorage25";
        static string ImageContainer = "blobcontainer";

        static async Task Main(string[] args)
        {
            await PutIntoStorage("./smith.jpg", "smith.jpg");
        }

        private static async Task PutIntoStorage(string path, string fileName)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                //await UploadFileToStorage(stream, fileName);
                await DownloadFileFromStorage(fileName);
            }
        }

        public static async Task<CloudBlockBlob> UploadFileToStorage(Stream fileStream, string fileName)
        {
            StorageCredentials storageCredentials = new StorageCredentials(AccountName, AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(ImageContainer);
            CloudBlockBlob reference = container.GetBlockBlobReference(fileName);

            await reference.UploadFromStreamAsync(fileStream);

            return await Task.FromResult(reference);
        }

        public static async Task DownloadFileFromStorage(string fileName)
        {
            StorageCredentials storageCredentials = new StorageCredentials(AccountName, AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(ImageContainer);
            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            MemoryStream memStream = new MemoryStream();

            await blockBlob.DownloadToStreamAsync(memStream);

            using (FileStream downloadFileStream = File.OpenWrite("./smith-download.jpg"))
            {
                memStream.WriteTo(downloadFileStream);
            }

            memStream.Close();
            memStream.Dispose();
        }
    }
}
