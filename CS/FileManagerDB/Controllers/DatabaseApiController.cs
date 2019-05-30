using DevExtreme.AspNet.Mvc.FileManagement;
using FileManagerDB.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace FileManagerDB.Controllers {
    [Route("api/[controller]")]
    public class DatabaseApiController : Controller
    {
        IHostingEnvironment HostingEnvironment;
        ArtsDBContext ArtsDBContext;
        public DatabaseApiController(ArtsDBContext context, IHostingEnvironment hostingEnvironment) {
            ArtsDBContext = context;
            HostingEnvironment = hostingEnvironment;
        }
        public IActionResult FileSystem(FileSystemCommand command, string arguments) {
            var config = new FileSystemConfiguration {
                Request = Request,
                FileSystemProvider = new DbFileProvider(ArtsDBContext),
                AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowRemove = true,
                AllowRename = true,
                AllowUpload = true,
                UploadTempPath = HostingEnvironment.ContentRootPath + "/wwwroot/UploadTemp"
            };
            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return Ok(result.GetClientCommandResult());
        }
    }
}
