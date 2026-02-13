<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/189187563/20.1.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T828689)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
# File Manager for ASP.NET Core - How to connect to a database

This example demonstrates how to get data from a database and display it in the [File Manager](https://docs.devexpress.com/AspNetCore/401320/devextreme-based-controls/controls/file-manager) or implement custom processing logic. To implement server interaction with a file system, pass a class that uses [file management interfaces](https://docs.devexpress.com/AspNetCore/401686/devextreme-based-controls/concepts/file-management#file-system-provider) to the [FileSystemProvider](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.FileSystemConfiguration.FileSystemProvider) property.

## Implementation Details

1. Add the [FileManager](https://docs.devexpress.com/AspNetCore/401320/devextreme-based-controls/controls/file-manager) control to your page and configure it according to your requirements.
2. Set the `Url` option so that it points to your API controller.

```cs 
@(Html.DevExtreme().FileManager()
    .CurrentPath("Documents/Reports")
    .FileSystemProvider(provider => provider.Remote()
        .Url(Url.RouteUrl("FileManagerApi")))

```

3. Specify File Manager [Permissions](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.Builders.FileManagerBuilder.Permissions(System.Action-DevExtreme.AspNet.Mvc.Builders.FileManagerPermissionsBuilder-)?p=netframework) according to your requirements.

4. Implement required file management interfaces.
* If you want only to display files and folders, implement [IFileSystemItemLoader](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileSystemItemLoader).
* If you want to copy, move, and delete items - implement [IFileSystemItemEditor](https://docs.devexpress.com/AspNetCore/DevExtreme.AspNet.Mvc.FileManagement.IFileSystemItemEditor).

You can find a full implementation example in the [DbFileProvider.cs](CS/FileManagerDB/Models/DbFileProvider.cs) file.

5. Create a method in your [API Controller](CS/FileManagerDB/Controllers/FileManagerApiController.cs) that will handle File Manager operations. Use your custom provider there.

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
<!-- feedback -->
## Does This Example Address Your Development Requirements/Objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=asp-net-core-file-manager-connect-to-database&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=asp-net-core-file-manager-connect-to-database&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
