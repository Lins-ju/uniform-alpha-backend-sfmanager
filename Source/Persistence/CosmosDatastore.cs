using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SFManager.Source.Domain;
using SFManager.Source.Models;
using SFManager.Source.Models.Repository;
using System.Net;
using Utils.Source.Models;
using static Amazon.S3.Util.S3EventNotification;

namespace SFManager.Source.Persistence
{
    public class CosmosDatastore
    {
        private readonly Database _database;
        private readonly Container _container;
        private const int maxNumberOftries = 1;
        private const int delayTimeMilliseconds = 100;

        private readonly Serilog.ILogger logger;

        public CosmosDatastore(CosmosClient cosmosClient, IOptions<CosmosOptions> options)
        { 
            _database = cosmosClient.GetDatabase(options.Value.DatabaseName);
            _container = _database.GetContainer(options.Value.ContainerName);
        }

        public async Task<Status> Save(string folderId, string userId, string folderName, int tryCount = 0)
        {
            try
            {
                FolderI folderInfo = new FolderI(folderId, userId, folderName);
                var saveResult = await _container.CreateItemAsync(folderInfo, new PartitionKey(folderInfo.UserId));

                if (saveResult.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (tryCount > maxNumberOftries)
                    {
                        saveResult.Resource.Status = Status.Unsuccessful;
                        return Status.Success;
                    }
                    if (tryCount <= maxNumberOftries)
                    {
                        tryCount++;
                        await Task.Delay(delayTimeMilliseconds);
                        await Save(folderInfo.FolderId, folderInfo.UserId, folderInfo.FolderName);
                    }
                }
                if (saveResult.StatusCode == HttpStatusCode.Created)
                {
                    return Status.Success;
                }

                return Status.Unsuccessful;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Status.ErrorWithException;
            }
        }

        public async Task<FolderI> Get(string userId, string folderId, int tryCount = 0)
        {
            try
            {
                var getResult = await _container.ReadItemAsync<FolderI>(folderId, new PartitionKey(userId));

                if (getResult.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (tryCount > maxNumberOftries)
                    {
                        getResult.Resource.Status = Status.Unsuccessful;
                        return getResult.Resource;
                    }
                    if (tryCount <= maxNumberOftries)
                    {
                        tryCount++;
                        await Task.Delay(delayTimeMilliseconds);
                        await Get(folderId, userId);
                    }
                }
                if (getResult.StatusCode == HttpStatusCode.OK)
                {
                    getResult.Resource.Status = Status.Success;
                    return getResult.Resource;
                }

                getResult.Resource.Status = Status.Unsuccessful;
                return getResult.Resource;
            }
            catch (Exception ex)
            {
                FolderI folderInfo = new FolderI();
                logger.Error(ex.ToString());
                folderInfo.Status = Status.ErrorWithException;
                return folderInfo;
            }
        }

        public async Task<Status> Update(string folderId, string userId, string folderName, int tryCount = 0)
        {
            try
            {
                FolderI folderInfo = new FolderI(folderId, userId, folderName);
                var updateResult = await _container.UpsertItemAsync(folderInfo, new PartitionKey(folderInfo.UserId));

                if (updateResult.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (tryCount > maxNumberOftries)
                    {
                        updateResult.Resource.Status = Status.Unsuccessful;
                        return Status.Success;
                    }
                    if (tryCount <= maxNumberOftries)
                    {
                        tryCount++;
                        await Task.Delay(delayTimeMilliseconds);
                        await Update(folderInfo.FolderId, folderInfo.UserId, folderInfo.FolderName);
                    }
                }

                if (updateResult.StatusCode == HttpStatusCode.OK || updateResult.StatusCode == HttpStatusCode.Created)
                {
                    return Status.Success;
                }

                return Status.Unsuccessful;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Status.ErrorWithException;
            }
        }

        public async Task<Status> Delete(string folderId, string userId, int tryCount = 0)
        {
            try
            {
                var deleteResult = await _container.DeleteItemAsync<FolderI>(folderId, new PartitionKey(userId));

                if (deleteResult.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (tryCount > maxNumberOftries)
                    {
                        deleteResult.Resource.Status = Status.Unsuccessful;
                        return Status.Deleted;
                    }
                    if (tryCount <= maxNumberOftries)
                    {
                        tryCount++;
                        await Task.Delay(delayTimeMilliseconds);
                        await Delete(folderId, userId);
                    }
                }
                if (deleteResult.StatusCode == HttpStatusCode.NoContent)
                {
                    return Status.Deleted;
                }

                return Status.Unsuccessful;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Status.ErrorWithException;
            }
        }

        public async Task<List<FolderI>> Query(string userId)
        {
            FeedIterator<FolderI> feedIterator;
            FeedResponse<FolderI> feedResponse;
            var userIdWithDoubleQuotes = '"' + userId + '"';

            List<FolderI> entityList = new List<FolderI>();
            var queryText = $"SELECT * FROM FolderInfo c WHERE c.UserId = {userIdWithDoubleQuotes}";

            try
            {
                feedIterator = _container.GetItemQueryIterator<FolderI>(queryText: queryText);

                do
                {
                    feedResponse = await feedIterator.ReadNextAsync();

                    foreach (var item in feedResponse)
                    {
                        entityList.Add(item);
                    }

                } while (feedIterator.HasMoreResults);

                return entityList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return entityList;
            }
        }
    }
}
