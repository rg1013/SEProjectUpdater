using System.Runtime.InteropServices;
namespace ExampleCloud
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Proxy Class for Updater Team
    /// </summary>
    public class UpdaterCloudProxy
    {
        Cloud cloudService;
        string containerName = "updater";   /// specific container details
        string _sasToken = "";  /// SAS Token for authentication

                                /// <summary>
                                /// Set up cloud service
                                /// </summary>
        public UpdaterCloudProxy()
        {
            cloudService = new Cloud(_sasToken, containerName);
            _sasToken = SasToken;
        }

        /// <summary>
        /// GET functionality from cloud
        /// <param name="dataUri">data URI to access</param>
        /// <returns>Dictionary containing dataURI and data</returns>
        /// </summary>
        public Dictionary<string, object> Get(string dataUri)
        {
            return cloudService.Get(_sasToken, containerName, dataUri);
        }

        /// <summary>
        /// POST functionality for cloud
        /// <param name="dataUri">Data URI to post</param>
        /// <param name="data">Data to post</param>
        /// <returns>New data URI as string</returns>
        /// </summary>
        public string Post(string dataUri, object data)
        {
            Dictionary<string, string> responseDict = cloudService.Post(_sasToken, containerName, dataUri, data);
            return responseDict.ContainsKey("dataUri") ? responseDict["dataUri"] : null;
        }

        /// <summary>
        /// PUT functionality for cloud
        /// <param name="oldDataUri">Data URI to update</param>
        /// <param name="data">Data to update</param>
        /// <returns>Boolean indicating whether the update was successful</returns>
        /// </summary>
        public bool Put(string oldDataUri, object data)
        {
            Dictionary<string, bool> responseDict = cloudService.Put(_sasToken, containerName, oldDataUri, data);
            return responseDict.ContainsKey("success") && responseDict["success"];
        }

        /// <summary>
        /// DELETE functionality for cloud
        /// <param name="dataUri">Data URI to delete</param>
        /// <returns>Boolean indicating whether the delete was successful</returns>
        /// </summary>
        public bool Delete(string dataUri)
        {
            Dictionary<string, bool> responseDict = cloudService.Delete(_sasToken, containerName, dataUri);
            return responseDict.ContainsKey("success") && responseDict["success"];
        }
    }

}
