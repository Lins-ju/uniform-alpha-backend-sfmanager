using Microsoft.AspNetCore.Mvc;
using SFManager.Source.Domain;
using SFManager.Source.Models.Service;

namespace SFManager.Source
{
    [ApiController]
    [Route("api/filemanager")]
    public class FileManagerService : ControllerBase
    {
        FileManagerController _fileManagerController;

        public FileManagerService(FileManagerController fileManagerController)
        {
            _fileManagerController = fileManagerController;
        }

        [HttpPost]
        [Route("savefile")]

        public async Task<IdResponse> SaveFile(SaveFileRequest request)
        {
            var result = await _fileManagerController.SaveFileToFolder(request.UserId, request.FolderId, request.FileContent, request.FileName);
            return new IdResponse(result.Id, result.Status);
        }

        [HttpPost]
        [Route("createfolder")]

        public async Task<IdResponse> CreateFolder(CreateFolderRequest request)
        {
            var result = await _fileManagerController.CreateFolder(request.UserId, request.FolderName);
            return new IdResponse(result.Id, result.Status);
        }

        [HttpPost]
        [Route("getfolderandfiles")]

        public async Task<Dictionary<string, List<FileICResponse>>> GetFolderAndFiles(GetFolderAndFilesRequest request)
        {
            var result = await _fileManagerController.GetAllFolderAndFilesByUserId(request.UserId);
            var transformResult = new GetFolderAndFilesResponse(result);
            return transformResult.FolderAndFiles;
        }

        [HttpPost]
        [Route("deletefolder")]

        public async Task<StatusResponse> DeleteFolder(DeleteFolderRequest request)
        {
            var result = await _fileManagerController.DeleteFolder(request.UserId, request.FolderId);
            return new StatusResponse(result);
        }

        [HttpPost]
        [Route("deletefile")]

        public async Task<StatusResponse> DeleteFile(DeleteFileRequest request)
        {
            var result = await _fileManagerController.DeleteFile(request.UserId, request.FolderId, request.FileId);
            return new StatusResponse(result);
        }

        [HttpGet]
        [Route("apitest")]

        public string ReturnString()
        {
            return "You are running this app...";
        }
    }
}
