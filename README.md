# File Manager - How to connect to a database using IFIleSystemProvider

This examples shows how to implement get data from a database in the File Manager or implement custom processing logic. You can use the **[IFileProvider](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileProvider)** interface for this. 

### Follow these steps:
1. Add the FileManager to your page and setup it on the client side.
2. Set the [fileProvider.endpointUrl](https://js.devexpress.com/DevExtreme/ApiReference/UI_Widgets/dxFileManager/Configuration/#fileProvider) option so that it points to your API controller:
 ```js
  $("#file-manager").dxFileManager({
            name: "fileManager",
            fileProvider: new DevExpress.FileProviders.WebApi({
                endpointUrl: '@Url.RouteUrl("Default", new { controller = "api/DataBaseApi" })'
            }),
 ```
3. Specify File Manager [permissions](https://js.devexpress.com/DevExtreme/ApiReference/UI_Widgets/dxFileManager/Configuration/permissions/) according to your requirements.
> **NOTE:** You can map your database object properties to File Manager item using client-side mapping options: [nameExpr](https://js.devexpress.com/DevExtreme/ApiReference/UI_Widgets/dxFileManager/File_Providers/Ajax/Configuration/#nameExpr), [sizeExpr](https://js.devexpress.com/DevExtreme/ApiReference/UI_Widgets/dxFileManager/File_Providers/Ajax/Configuration/#sizeExpr) and others. 



4. Implement the [IFileProvider](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileProvider) interface. You can find an example of implementation in the [Models/DbFileSystemProvider.cs](./CS/FileManagerDB/Models/DbFileProvider.cs) file.

> **NOTE:** If you need to implement custom mapping logic of your database object to File Manager and cannot use the client-side mapping, use the **IClientFileSystemItem** interface. 

5. Create a method in your [API Controller](./CS/FileManagerDB/Controllers/DatabaseApiController.cs) that will handle the File Manager operations. Use your custom provider there:
```cs
public IActionResult FileSystem(FileSystemCommand command, string arguments) {
            var config = new FileSystemConfiguration {
                Request = Request,
                FileSystemProvider = new DbFileProvider(ArtsDBContext),
                AllowCopy = true,
                ...
                AllowCreate = true,
                UploadTempPath = HostingEnvironment.ContentRootPath + "/wwwroot/UploadTemp"
            };
            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return Ok(result.GetClientCommandResult());
        }
```

To test this example on your side, add the Arts.mdf database to your server and modify the connection string in config.json, if necessary. 