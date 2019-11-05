using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfAlbus
{ /// <summary>
  ///     This class represents a collection of photos in a directory.
  /// </summary>
    public class PhotoCollection : ObservableCollection<Photo>
    {
        private DirectoryInfo _directory;
        private string container_name ="containerservice";
        private string folder_name;
        public string event_name;

        public PhotoCollection()
        {
        }

       
        //public PhotoCollection(String containerName)
        //{
        //    container_name = containerName;
        //    Update(container_name);
        //}
        public string getEventName
        {
           get { return event_name; }
        }
        public string Path(string userId, string eventName)
        {
            Update(userId, eventName);
            
            event_name = eventName;
            //set
            //{

            //    folder_name = value;

            //    Update(folder_name);
            //}
            //get { return folder_name; }
            return null;
        }

        

        private void Update(string userId, string eventName)
        {
            Clear();
            try
            {
                //foreach (var f in _directory.GetFiles("*.jpg"))
                //    Add(new Photo(f.FullName));

                // Add(new Photo("https://albus.blob.core.windows.net/samplecontainer/detection6.jpg"));

                CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString("DefaultEndpointsProtocol=https;AccountName=albus;AccountKey=V6tfipu99EwHGMhaxSiYNAE3m1FE43jSKHtNpbr3Z76Pf7nsbRFfJOsleI2Po3UWObTX/7c9R6h5JYS1EfqlGQ==;EndpointSuffix=core.windows.net");

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(container_name);
                CloudBlobDirectory eventurl = container.GetDirectoryReference(userId+"/"+eventName+"/EventImages");
                foreach (IListBlobItem blob in eventurl.ListBlobs())
                {
                    // Blob type will be CloudBlockBlob, CloudPageBlob or CloudBlobDirectory
                    // Use blob.GetType() and cast to appropriate type to gain access to properties specific to each type
                    try
                    {
                        Add(new Photo(blob.Uri.AbsoluteUri));

                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("----"+e.ToString());

                    }
                }

            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("No Such Directory");
            }
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                System.Diagnostics.Debug.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            catch (ArgumentException)
            {
                System.Diagnostics.Debug.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }


    }
}
