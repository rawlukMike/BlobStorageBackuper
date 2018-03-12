using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobBackuper.Helper
{
    class BlobManager
    {
        // Parse the connection string and return a reference to the storage account.
        private static CloudBlobContainer _container;
        private static CloudBlobClient _blobClient;

        // CHECK
        public BlobManager(string key=null)
        {
            CloudStorageAccount storageAccount;
            if (key == null)
            {
                storageAccount=CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                storageAccount = CloudStorageAccount.Parse(key);
            }
            _blobClient = storageAccount.CreateCloudBlobClient();
            _container = _blobClient.GetContainerReference("backuper");
            _container.CreateIfNotExists();
        }

        private static string ConvertToRelativeUri(string filePath, string baseDir)
        {
            var uri = new Uri(filePath);
            if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseDir += Path.DirectorySeparatorChar.ToString();
            }
            var baseUri = new Uri(baseDir);
            return baseUri.MakeRelativeUri(uri).ToString();
        }

        // CHECK
        public void UploadFolder(string srcPath, int concurrent = 5, int threads = 1)
        {
            var entries = Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories);

            BlobRequestOptions bro = new BlobRequestOptions()
            {
                SingleBlobUploadThresholdInBytes = 5 * 1024 * 1024, //5MB, the minimum is 1MB
                ParallelOperationThreadCount = threads
            };
            AccessCondition acc = new AccessCondition();
            OperationContext context = new OperationContext();

            Parallel.ForEach(entries, new ParallelOptions { MaxDegreeOfParallelism = concurrent }, file =>
            {
                var blobName = ConvertToRelativeUri(file, srcPath);
                var blob = _container.GetBlockBlobReference(@"folder/" + blobName);
                blob.UploadFromFile(file, acc, bro, context);
            });
        }

        // CHECK
        public async Task UploadFile(string filePath, int threads = 1, int loops = 1)
        {
            BlobRequestOptions bro = new BlobRequestOptions()
            {
                SingleBlobUploadThresholdInBytes = 5 * 1024 * 1024, //5MB, the minimum is 1MB
                ParallelOperationThreadCount = threads
            };
            AccessCondition acc = new AccessCondition();
            OperationContext context = new OperationContext();

            CloudBlockBlob myblob = _container.GetBlockBlobReference("file/" + Path.GetFileName(filePath));

            for (int i = 0; i < loops; i++) await myblob.UploadFromFileAsync(filePath, acc, bro, context);
        }

        // CHECK
        public async Task DownloadFile(string desDirName, string fileName, int threads = 1, int loops = 1)
        {
            BlobRequestOptions bro = new BlobRequestOptions()
            {
                SingleBlobUploadThresholdInBytes = 5 * 1024 * 1024, //5MB, the minimum is 1MB
                ParallelOperationThreadCount = threads
            };
            AccessCondition acc = new AccessCondition();
            OperationContext context = new OperationContext();
            Directory.CreateDirectory(desDirName);
            var myblob = _container.GetBlockBlobReference("file/" + fileName);
            for (int i = 0; i < loops; i++) await myblob.DownloadToFileAsync(desDirName + @"\" + fileName, FileMode.Create, acc, bro, context);
        }

        // CHECK comment
        public void DownloadFolder(string desDirName, int concurrent = 5, int threads = 1)
        {
            BlobRequestOptions bro = new BlobRequestOptions()
            {
                SingleBlobUploadThresholdInBytes = 5 * 1024 * 1024, //5MB, the minimum is 1MB
                ParallelOperationThreadCount = threads
            };
            AccessCondition acc = new AccessCondition();
            OperationContext context = new OperationContext();


            var myblobdir = _container.GetDirectoryReference("folder");
            Parallel.ForEach(myblobdir.ListBlobs(true), new ParallelOptions { MaxDegreeOfParallelism = concurrent }, fileBlobNameMapper =>
            {
                var blob = (CloudBlockBlob)fileBlobNameMapper;
                // Save blob contents to a file.
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(desDirName + @"\" + blob.Name));
                using (var fileStream = File.OpenWrite(desDirName + @"\" + blob.Name))
                {
                    blob.DownloadToStream(fileStream, acc, bro, context);
                }
            });
        }

    }
}
