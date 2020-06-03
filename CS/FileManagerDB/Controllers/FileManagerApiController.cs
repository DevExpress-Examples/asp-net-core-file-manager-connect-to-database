using System;
using DevExtreme.AspNet.Mvc.FileManagement;
using FileManagerDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FileManagerDB.Controllers {

    public class FileManagerApiController : Controller {
        public FileManagerApiController(DbFileProvider dbFileProvider) {
            _dbFileProvider = dbFileProvider ?? throw new ArgumentNullException(nameof(dbFileProvider));
        }

        DbFileProvider _dbFileProvider { get; }

        [Route("api/file-manager-db", Name = "FileManagerApi")]
        public IActionResult Process(FileSystemCommand command, string arguments) {
            var config = new FileSystemConfiguration {
                Request = Request,
                FileSystemProvider = _dbFileProvider,
                AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowedFileExtensions = new string[0]
            };
            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return Ok(result.GetClientCommandResult());
        }
    }
}