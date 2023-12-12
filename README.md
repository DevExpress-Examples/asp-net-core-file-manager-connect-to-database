<!-- default badges list -->
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T828689)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
# File Manager for ASP.NET Core - How to connect control to a database

This examples demonstrates how to get data from a database and display it in the [File Manager](https://docs.devexpress.com/AspNetCore/401320/devextreme-based-controls/controls/file-manager) or implement custom processing logic. To implement server interaction with a file system, pass a class that uses [file management interfaces](https://docs.devexpress.com/AspNetCore/401686/devextreme-based-controls/concepts/file-management#file-system-provider) to the [FileSystemProvider](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.FileSystemConfiguration.FileSystemProvider) property.

## Implementation Details

1. Add the [FileManager](https://docs.devexpress.com/AspNetCore/401320/devextreme-based-controls/controls/file-manager) control to your page and setup it on the client side.
2. Set the `Url` option so that it points to your API controller.

```cs 
@(Html.DevExtreme().FileManager()
    .CurrentPath("Documents/Reports")
    .FileSystemProvider(provider => provider.Remote()
        .Url(Url.RouteUrl("FileManagerApi")))

```

3. Specify File Manager [Permissions](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.Builders.FileManagerBuilder.Permissions(System.Action-DevExtreme.AspNet.Mvc.Builders.FileManagerPermissionsBuilder-)?p=netframework) according to your requirements.

4. Implement required file management interfaces.
* If you want only to display files and folders, implement [IFileSystemItemLoade](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileSystemItemLoader). 
* If you want to copy, move, delete items - implement [IFileSystemItemEditor](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileSystemItemEditor).

You can find a full implementation example in the [DbFileProvider.cs](CS/FileManagerDB/Models/DbFileProvider.cs) file.

5. Create a method in your [API Controller](CS/FileManagerDB/Controllers/FileManagerApiController.cs), which will handle File Manager operations. Use your custom provider there.

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

## Files to Review

* [Index.cshtml](./CS/FileManagerDB/Views/Home/Index.cshtml)
* [DbFileProvider.cs](./CS/FileManagerDB/Models/DbFileProvider.cs)
* [FileManagerApiController.cs](./CS/FileManagerDB/Controllers/FileManagerApiController.cs)
