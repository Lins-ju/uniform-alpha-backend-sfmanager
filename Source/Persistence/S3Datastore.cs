using Amazon.S3.Model;
using Amazon.S3;
using System.Text.Json;
using Utils.Source.Models;
using System.Net;
using SFManager.Source.Models.Repository;
using Microsoft.Extensions.Options;
using SFManager.Source.Domain;

namespace SFManager.Source.Persistence
{
    public class S3Datastore
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        public AmazonS3Client _s3Buckets;
        public string bucketParameter;
        public S3Datastore(AmazonS3Client s3Client, IOptions<S3Options> options)
        {
            bucketParameter = options.Value.BucketName;
            _s3Buckets = s3Client;
        }

        public async Task<string> GetFileSerialized(string objectKey)
        {
            var objectRequest = new GetObjectRequest
            {
                BucketName = bucketParameter,
                Key = objectKey
            };

            try
            {
                var getResponse = await _s3Buckets.GetObjectAsync(objectRequest);

                if(getResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    using var reader = new StreamReader(getResponse.ResponseStream);
                    var fileContent = await reader.ReadToEndAsync();

                    return fileContent;
                }
                return "NotFound";
            }
            catch (Exception ex)
            {
                if(ex.Message == "The specified key does not exist.")
                {
                    return "NotFound";
                }

                Console.WriteLine("Exception Message:" + ex.Message);
                return "Error With Exception";
            }

        }

        public async Task<Status> SaveFile(string objectKey, string fileContent, string fileName)
        {
            try
            {
                var fileInfo = new FileICRepository(objectKey, fileContent, fileName);
                var serializedFileInfo = JsonSerializer.Serialize(fileInfo, options);

                var objectRequest = new PutObjectRequest
                {
                    BucketName = bucketParameter,
                    Key = objectKey,
                    ContentBody = serializedFileInfo,
                    ContentType = "application/json"
                };

                var result = await _s3Buckets.PutObjectAsync(objectRequest);

                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    return Status.Success;
                }

                return Status.Error;
            }
            catch (Exception ex)
            {
                return Status.ErrorWithException;
            }
        }

        public async Task<Status> DeleteFile(string filePath)
        {
            try
            {
                var result = await _s3Buckets.DeleteObjectAsync(bucketParameter, filePath);

                if (result.HttpStatusCode == HttpStatusCode.NoContent)
                {
                    return Status.Deleted;
                }

                return Status.Unsuccessful;
            }
            catch (Exception ex)
            {
                return Status.ErrorWithException;
            }
        }

        public async Task<List<string>> ListFilesInFolder(string folderPath)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketParameter,
                Prefix = folderPath
            };

            var fileIds = new List<string>();

            ListObjectsV2Response response;

            try
            {
                do
                {
                    response = await _s3Buckets.ListObjectsV2Async(request);
                    foreach (var obj in response.S3Objects)
                    {
                        fileIds.Add(obj.Key);
                        Console.WriteLine($"Key: {obj.Key}");
                    }

                    foreach (var commonPrefix in response.CommonPrefixes)
                    {
                        Console.WriteLine($"Common Prefixes: {commonPrefix}");
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while (response.IsTruncated);

                return fileIds;
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
                var emptyList = new List<string>();
                emptyList.Add("|| Error With Exception Thrown || ListFilesInFolder");
                return emptyList;
            }
        }
    }
}
