using Microsoft.AspNetCore.Http.HttpResults;
using SFManager.Source.Models.Controller;
using SFManager.Source.Persistence;
using System.Text.Json;
using Utils.Source.Models;

namespace SFManager.Source.Domain
{
    public class FileManagerController
    {
        private S3Datastore _s3Datastore;
        CosmosDatastore _cosmosDatastore;

        protected string RandomGuid() => Guid.NewGuid().ToString();

        public FileManagerController(S3Datastore s3Datastore, CosmosDatastore cosmosDatastore)
        {
            _s3Datastore = s3Datastore;
            _cosmosDatastore = cosmosDatastore;
        }

        public async Task<IdResult> SaveFileToFolder(string userId, string folderId, string fileContent, string fileName)
        {
            var objectKey = RandomGuid();
            var fileNameWithPath = $"{userId}/{folderId}/{objectKey}";

            try
            {
                var checkIfFolderExists = await _cosmosDatastore.Get(userId, folderId);

                if (checkIfFolderExists.Status == Status.Success)
                {
                    var s3result = await _s3Datastore.SaveFile(fileNameWithPath, fileContent, fileName);

                    if (s3result == Status.Success)
                    {
                        return new IdResult(objectKey, Status.Success);
                    }

                    return new IdResult(Status.InternalError);
                }

                return new IdResult(Status.EntityDoesNotExist); //If FolderId does not exist
            }
            catch (Exception ex)
            {
                return new IdResult(Status.ErrorWithException);
            }
        }

        public async Task<IdResult> CreateFolder(string userId, string folderName)
        {
            try
            {
                var folderId = RandomGuid();
                var cosmosResult = await _cosmosDatastore.Save(folderId, userId, folderName);

                if (cosmosResult == Status.Success)
                {
                    return new IdResult(folderId, Status.Success);
                }

                return new IdResult(Status.Error);
            }
            catch (Exception ex)
            {
                return new IdResult(Status.ErrorWithException);
            }
        }

        public async Task<Dictionary<string, List<FileIC>>> GetAllFolderAndFilesByUserId(string userId) // Returning Dictionary with FileId : { (FileIC) }
        {
            try
            {
                var getAllFolders = await _cosmosDatastore.Query(userId);

                Dictionary<string, List<FileIC>> keyList = new Dictionary<string, List<FileIC>>();

                foreach (var cosmosItem in getAllFolders)
                {
                    List<FileIC> fileIcs = new List<FileIC>();

                    var folderId = cosmosItem.FolderId;
                    var folderName = cosmosItem.FolderName;
                    var s3Result = await _s3Datastore.ListFilesInFolder($"{userId}/{folderId}/"); //Id is FolderId

                    if (s3Result.Count > 0)
                    {
                        foreach (var objectKey in s3Result)
                        {
                            var getFileInfo = await GetFileContent(objectKey);

                            if (getFileInfo != null)
                            {
                                fileIcs.Add(getFileInfo);
                            }
                        }
                        keyList.Add(folderId + "/" + folderName, fileIcs);
                    }
                    if (s3Result.Count == 0)
                    {
                        keyList.Add(folderId + "/" + folderName, new List<FileIC>());
                    }

                }

                return keyList;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, List<FileIC>>();
            }
        }

        public async Task<FileIC> GetFileContent(string fileNameWithPath)
        {
            try
            {
                var s3result = await _s3Datastore.GetFileSerialized(fileNameWithPath);
                var deserializedFileContent = JsonSerializer.Deserialize<FileIC>(s3result);

                if (deserializedFileContent != null)
                {
                    return deserializedFileContent;
                }

                return new FileIC(Status.InternalError);
            }
            catch (Exception ex)
            {
                return new FileIC(Status.ErrorWithException);
            }
        }


        public async Task<Status> DeleteFolder(string userId, string folderId)
        {
            var folderPath = $"{userId}/{folderId}/";

            try
            {
                var getAllFolders = await _cosmosDatastore.Query(userId);
                var getAllFilesInFolder = await _s3Datastore.ListFilesInFolder(folderPath);

                foreach (var folderIdFromS3 in getAllFolders)
                {
                    var deleteFolder = await _cosmosDatastore.Delete(folderId, userId);

                    if (deleteFolder == Status.Unsuccessful)
                    {
                        return Status.InternalError;
                    }
                }

                if (getAllFilesInFolder.Count > 0)
                {
                    foreach (var objectKey in getAllFilesInFolder)
                    {
                        var deleteAllFilesInFolder = await _s3Datastore.DeleteFile(objectKey);

                        if (deleteAllFilesInFolder == Status.Unsuccessful)
                        {
                            return Status.InternalError;
                        }
                    }
                }


                return Status.Success;
            }
            catch (Exception ex)
            {
                return Status.ErrorWithException;
            }
        }

        public async Task<Status> DeleteFile(string userId, string folderId, string fileId)
        {
            var filePath = $"{userId}/{folderId}/{fileId}";

            try
            {
                var checkIfFileExists = await _s3Datastore.GetFileSerialized(filePath);

                if(checkIfFileExists != "NotFound" && checkIfFileExists != null)
                {
                    var fileNameWithPath = $"{userId}/{folderId}/{fileId}";

                    var deleteResult = await _s3Datastore.DeleteFile(fileNameWithPath);

                    if (deleteResult == Status.Deleted)
                    {
                        return Status.Success;
                    }

                    return Status.InternalError;
                }

                return Status.EntityDoesNotExist;
            }
            catch (Exception ex)
            {
                return Status.ErrorWithException;
            }
        }
    }
}
