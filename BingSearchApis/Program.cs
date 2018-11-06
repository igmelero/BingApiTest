using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace BingSearchTest
{
    class Program
    {

        const string subscriptionKey = "<subscriptionkey>";
        const string uriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";
        const string searchTerm = "shoe";

        static void Main(string[] args)
        {

            SearchResult result = BingImageSearch(searchTerm);

            //deserialize the JSON response from the Bing Image Search API
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.jsonResult);

            var firstJsonObj = jsonObj["value"][0];
            Console.WriteLine("Title for the first image result: " + firstJsonObj["name"] + "\n");
            //After running the application, copy the output URL into a browser to see the image.
            Console.WriteLine("URL for the first image result: " + firstJsonObj["webSearchUrl"] + "\n");
            UploadImage_URL((String)firstJsonObj["thumbnailUrl"], "test1.jpeg");
            Console.WriteLine(firstJsonObj);

        }
        struct SearchResult
        {
            public String jsonResult;
            public Dictionary<String, String> relevantHeaders;
        }

        static SearchResult BingImageSearch(string toSearch)
        {

            var uriQuery = uriBase + "?q=" + Uri.EscapeDataString(toSearch) + "site:zara.com";

            WebRequest request = WebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = subscriptionKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Create the result object for return
            var searchResult = new SearchResult()
            {
                jsonResult = json,
                relevantHeaders = new Dictionary<String, String>()
            };

            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }
            return searchResult;

        }

        public static void UploadImage_URL(string file, string ImageName)
        {
            string accountname = "<accountname>";

            string accesskey = "<accesskey>";

            try
            {

                StorageCredentials creden = new StorageCredentials(accountname, accesskey);

                CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);

                CloudBlobClient client = acc.CreateCloudBlobClient();

                CloudBlobContainer cont = client.GetContainerReference("<containername>");

                cont.CreateIfNotExists();

                cont.SetPermissions(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob

                });
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(file);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream inputStream = response.GetResponseStream();
                CloudBlockBlob cblob = cont.GetBlockBlobReference(ImageName);
                cblob.UploadFromStream(inputStream);
            }
            catch (Exception ex) {}

        }
    }
}
