**# File Manager - How to connect to a database using IFIleSystemProvider**

This examples shows how to get data from a database in the File Manager or implement custom processing logic. To implement server-side interaction with a file system, pass a class that uses [file management interfaces](https://docs.devexpress.com/AspNetCore/401686/devextreme-based-controls/concepts/file-management#file-system-provider) to the [FileSystemProvider](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.FileSystemConfiguration.FileSystemProvider) property. 

**### Follow these steps:**
1. Add the FileManager to your page and setup it on the client side.
2. Set the [fileProvider.endpointUrl](https://js.devexpress.com/DevExtreme/ApiReference/UI_Widgets/dxFileManager/Configuration/#fileProvider) option so that it points to your API controller:
 

```cs 
@(Html.DevExtreme().FileManager()
    .CurrentPath("Documents/Reports")
    .FileSystemProvider(provider => provider.Remote()
        .Url(Url.RouteUrl("FileManagerApi")))

```

3. Specify File Manager [Permissions](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.Builders.FileManagerBuilder.Permissions(System.Action-DevExtreme.AspNet.Mvc.Builders.FileManagerPermissionsBuilder-)?p=netframework) according to your requirements.

4. Implement required file management interfaces. For example, if you want only to display files and folders, implement [IFileSystemItemLoader](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileSystemItemLoader). If you want to copy, move, delete items - implement [IFileItemEditor](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileSystemItemEditor) and so on. You can find a full implementation example in the [DbFileProvider.cs](CS%5CFileManagerDB%5CModels%5CDbFileProvider.cs) file.

5. Create a method in your [API Controller](CS%5CFileManagerDB%5CControllers%5CDatabaseApiController.cs), which will handle File Manager operations. Use your custom provider there:

```cs
 public FileManagerApiController(DbFileProvider dbFileProvider) {
            _dbFileProvider = dbFileProvider ?? throw new ArgumentNullException(nameof(dbFileProvider));
        }

        DbFileProvider _dbFileProvider { get; }

        [Route("api/file-manager-db", Name = "FileManagerApi")]
        public IActionResult Process(FileSystemCommand command, string arguments) {
            var config = new FileSystemConfiguration {
                Request = Request,
                FileSystemProvider = _dbFileProvider,
                AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowedFileExtensions = new string[0]
            };
            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return Ok(result.GetClientCommandResult());
        }
```
