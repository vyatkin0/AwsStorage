using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AwsStorage.Infrastructure
{
    /// <summary>
    /// Class of an object that communicates with Amazon S3 storage.
    /// </summary>
    public class AwsProxy
    {
        ILogger _logger;
        IConfiguration _configuration; // Configuration interface
        string _awsBucketName;

        public AwsProxy(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _awsBucketName = _configuration["Settings:AWS_BUCKET"];
        }

        /// <summary>
        /// Deletes objects from storage
        /// </summary>
        /// <param name="prefix">Object's key prefix</param>
        /// <returns>Deleted objects' prefix</returns>
        public Task<string> AwsDeleteObjects(string prefix) => AwsFunc<string>(
            async (client) =>
            {
                try
                {
                    _logger.LogInformation("Delete objects {0} on Amazon S3 store bucket.", prefix);

                    ListObjectsRequest listRequest = new ListObjectsRequest { BucketName = _awsBucketName, Prefix = prefix };
                    ListObjectsResponse listResponse = null;
                    do
                    {
                        listResponse = await client.ListObjectsAsync(listRequest);

                        string logMessage = $"Status code {listResponse.HttpStatusCode} for Amazon S3 request";
                        if (listResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _logger.LogInformation(logMessage);

                            foreach (S3Object o in listResponse.S3Objects)
                            {
                                // Create a DeleteObject request
                                DeleteObjectRequest request = new DeleteObjectRequest { BucketName = _awsBucketName, Key = o.Key };
                                // Issue request
                                DeleteObjectResponse response = await client.DeleteObjectAsync(request);

                                logMessage = $"Status code {response.HttpStatusCode} for Amazon S3 request";
                                if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent
                                    || response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    _logger.LogInformation(logMessage);
                                }
                                else
                                {
                                    _logger.LogError(logMessage);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError(logMessage);
                            break;
                        }

                    } while (listResponse.IsTruncated);

                    return prefix;
                }
                catch (AmazonS3Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }

                return null;
            });

        /// <summary>
        /// List objects with specified prefix
        /// </summary>
        /// <param name="prefix">Prefix of objects key</param>
        /// <param name="maxKeys">Maximum objects to return</param>
        /// <returns>List of S3 objects</returns>
        public Task<List<S3Object>> AwsListObjects(string prefix, int maxKeys) => AwsFunc<List<S3Object>>(
            async (client) =>
            {
                try
                {
                    _logger.LogInformation("List objects {0} on Amazon S3 store bucket.", prefix);

                    List<S3Object> objects = new List<S3Object>();

                    ListObjectsRequest listRequest = new ListObjectsRequest { BucketName = _awsBucketName, MaxKeys = maxKeys, Prefix = prefix };
                    ListObjectsResponse listResponse = null;
                    do
                    {
                        listResponse = await client.ListObjectsAsync(listRequest);

                        string logMessage = $"Status code {listResponse.HttpStatusCode} for Amazon S3 request";
                        if (listResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _logger.LogInformation(logMessage);

                            if (listResponse.IsTruncated)
                            {
                                objects.AddRange(listResponse.S3Objects);

                                // Set the marker property
                                listRequest.Marker = listResponse.NextMarker;
                            }
                            else
                            {
                                objects = listResponse.S3Objects;
                                break;
                            }

                            //foreach (S3Object o in listResponse.S3Objects)
                            //{
                            //Console.WriteLine("Object - " + o.Key);
                            //Console.WriteLine(" Size - " + o.Size);
                            //Console.WriteLine(" LastModified - " + o.LastModified);
                            //Console.WriteLine(" Storage class - " + o.StorageClass);
                            ///}
                        }
                        else
                        {
                            _logger.LogError(logMessage);
                            break;
                        }

                    } while (listResponse.IsTruncated && maxKeys < objects.Count);

                    return objects;
                }
                catch (AmazonS3Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }

                return null;
            });

        /// <summary>
        /// Removes an object
        /// </summary>
        /// <param name="objectKey">Object's key</param>
        /// <returns>Removed object's key</returns>
        public Task<string> AwsDeleteObject(string objectKey) => AwsFunc<string>(
            async (client) =>
            {
                try
                {
                    _logger.LogInformation("Delete {0} from Amazon S3 store bucket.", objectKey);

                    DeleteObjectRequest request = new DeleteObjectRequest { BucketName = _awsBucketName, Key = objectKey };
                    DeleteObjectResponse response = await client.DeleteObjectAsync(request);

                    string logMessage = $"Status code {response.HttpStatusCode} for Amazon S3 request";
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent
                          || response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation(logMessage);
                        return objectKey;
                    }
                    else
                        _logger.LogError(logMessage);
                }
                catch (AmazonS3Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
                }
                return null;
            });

        /// <summary>
        /// Puts object to store
        /// </summary>
        /// <param name="objectKey">Object's key</param>
        /// <param name="objectValue">Object's value</param>
        /// <returns>Object's key</returns>
        public Task<string> AwsPutObject(string objectKey, string objectValue) => AwsFunc<string>(
            async (client) =>
            {
                if(null==objectKey) return null;

                try
                {
                    _logger.LogInformation("Put {0} to Amazon S3 store bucket.", objectKey);

                    PutObjectRequest request = new PutObjectRequest { BucketName = _awsBucketName, Key = objectKey, ContentBody = objectValue };
                    PutObjectResponse response = await client.PutObjectAsync(request);

                    string logMessage = $"Status code {response.HttpStatusCode} for Amazon S3 request";
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation(logMessage);
                        return objectKey;
                    }
                    else
                        _logger.LogError(logMessage);
                }
                catch (AmazonS3Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                }
                return null;
            });

        /// <summary>
        /// Gets object's value
        /// </summary>
        /// <param name="objectKey">Object's key</param>
        /// <returns>Object's string value</returns>
        public Task<string> AwsGetObject(string objectKey) => AwsFunc<string>(
            async (client) =>
            {
                try
                {
                    _logger.LogInformation("Get {0} from Amazon S3 store bucket.", objectKey);

                    GetObjectRequest request = new GetObjectRequest { BucketName = _awsBucketName, Key = objectKey };

                    // Issue request and remember to dispose of the response
                    using (GetObjectResponse response = await client.GetObjectAsync(request))
                    {
                        string logMessage = $"Status code {response.HttpStatusCode} for Amazon S3 request";
                        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _logger.LogInformation(logMessage);

                            using (StreamReader reader = new StreamReader(response.ResponseStream))
                            {
                                string contents = reader.ReadToEnd();
                                return contents;
                            }
                        }
                        else
                            _logger.LogError(logMessage);
                    }
                }
                catch (AmazonS3Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when reading an object", e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error encountered on server. Message:'{0}' when reading an object", e.Message);
                }

                return null;
            });

        /// <summary>
        /// Executes generic async request to Amazon S3
        /// </summary>
        /// <param name="func">Concrete action</param>
        /// <returns>Created and started task</returns>
        private async Task<T> AwsFunc<T>(Func<AmazonS3Client, Task<T>> func)
        {
            string awsAccessKeyId = _configuration["Settings:AWS_ACCESS_KEY_ID"];
            string awsSecretAccessKey = null;

            try
            {
                awsSecretAccessKey = Encoding.UTF8.GetString(System.IO.File.ReadAllBytes("/run/secrets/tbx-aws-s3-key-platformstorage"));
            }
            catch (Exception)
            {
            }

            if (null == awsSecretAccessKey)
            {
                awsSecretAccessKey = _configuration["Settings:AWS_SECRET_ACCESS_KEY"];
            }

            RegionEndpoint bucketRegion = RegionEndpoint.GetBySystemName(_configuration["Settings:AWS_DEFAULT_REGION"]);
            using (AmazonS3Client _awsClient = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, bucketRegion))
            {
                return await func(_awsClient);
            }
        }
    }
}
