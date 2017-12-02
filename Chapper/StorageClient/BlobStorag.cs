using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage type
using Microsoft.Azure; //Namespace for CloudConfigurationManager

namespace StorageClient
{
    public class StorageClient : IStorageClient
    {
        private CloudAppendBlob _blockBlob;

        /// <summary>
        /// Create the blob storage refrences
        /// </summary>
        /// <param name="containerref">name of the container references
        ///</param>
        /// <param name="blockref">name of the blob reference</param>
        public StorageClient(string containerref, string blockref)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerref);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            // Retrieve reference to a blob named "myblob".
            _blockBlob = container.GetAppendBlobReference(blockref);
            if(!_blockBlob.Exists())
                _blockBlob.CreateOrReplace();
        }
        /// <summary>
        /// Store user message to storage
        /// </summary>
        /// <param name="name">name of the user who is sending the message</param>
        /// <param name="message">message sent by the user</param>
        public void StorageMessage(string name, string message)
        {
            Write(String.Format("[Timestamp:{0}][user:{1}][msg:{2}]\r\n",
                DateTime.UtcNow.ToString(), name, message));
        }
        /// <summary>
        /// Store login of a user
        /// </summary>
        /// <param name="name">name of the user who has logged in</param>
        public void StorageLogin(string name)
        {
            Write(String.Format("[Timestamp:{0}][user:{1}][LOGIN]\r\n",
                DateTime.UtcNow.ToString(), name));
        }
        /// <summary>
        /// Store logout of a user
        /// </summary>
        /// <param name="name">name of the user who has logged out</param>
        public void StorageLogout(string name)
        {

            Write(String.Format("[Timestamp:{0}][user:{1}][LOGOUT]\r\n",
                DateTime.UtcNow.ToString(), name));
        }

        /// <summary>
        /// Write data to blob storage
        /// </summary>
        /// <param name="txt">text to be written to storage</param>
        private void Write(string txt)
        {
            _blockBlob.AppendText(txt);
        }
    }
}
