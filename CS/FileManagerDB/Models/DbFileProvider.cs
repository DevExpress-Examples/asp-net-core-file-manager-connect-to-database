﻿using DevExtreme.AspNet.Mvc.FileManagement;
using DevExtreme.AspNet.Mvc.FileManagement.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerDB.Models {
    public class DbFileProvider : IFileSystemItemLoader, IFileSystemItemEditor {
        const int GuestPersonId = 1;

        public DbFileProvider(FileManagementDbContext fileManagementDbContext) {
            FileManagementDbContext = fileManagementDbContext;
        }

        //public DbFileProvider(FileManagementDbContext fileManagementDbContext, IHttpContextAccessor contextAccessor, IMemoryCache memoryCache) {
        //    FileManagementDbContext = new InMemoryFileManagementDataContext(fileManagementDbContext, contextAccessor, memoryCache);
        //}

        FileManagementDbContext FileManagementDbContext { get; set; }

        public IEnumerable<FileSystemItem> GetItems(FileSystemLoadItemOptions options) {
            int parentId = ParseKey(options.Directory.Key);
            var fileItems = GetDirectoryContents(parentId);

            var clientItemList = new List<FileSystemItem>();
            foreach (var item in fileItems) {
                var clientItem = new FileSystemItem {
                    Key = item.Id.ToString(),
                    Name = item.Name,
                    IsDirectory = item.IsDirectory,
                    DateModified = item.Modified
                };

                if (item.IsDirectory) {
                    clientItem.HasSubDirectories = FileManagementDbContext.FileItems.Where(i => i.ParentId == item.Id && i.IsDirectory).Any();
                }

                clientItem.CustomFields["modifiedBy"] = item.ModifiedBy != null ? item.ModifiedBy.FullName : "";
                clientItem.CustomFields["created"] = item.Created;
                clientItemList.Add(clientItem);
            }
            return clientItemList;
        }

        public void CreateDirectory(FileSystemCreateDirectoryOptions options) {
            var parentDirectory = options.ParentDirectory;
            if (!IsFileItemExists(parentDirectory))
                ThrowItemNotFoundException(parentDirectory);

            var directory = new FileItem {
                Name = options.DirectoryName,
                Modified = DateTime.Now,
                Created = DateTime.Now,
                IsDirectory = true,
                ParentId = ParseKey(parentDirectory.Key),
                ModifiedById = GuestPersonId
            };
            FileManagementDbContext.FileItems.Add(directory);
            FileManagementDbContext.SaveChanges();
        }

        public void RenameItem(FileSystemRenameItemOptions options) {
            var item = options.Item;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);

            var fileItem = GetFileItem(item);
            fileItem.Name = options.ItemNewName;
            fileItem.ModifiedById = GuestPersonId;
            fileItem.Modified = DateTime.Now;
            FileManagementDbContext.SaveChanges();
        }

        public void MoveItem(FileSystemMoveItemOptions options) {
            var item = options.Item;
            var destinationDirectory = options.DestinationDirectory;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);
            if (!IsFileItemExists(destinationDirectory))
                ThrowItemNotFoundException(destinationDirectory);
            if (!AllowCopyOrMove(item, destinationDirectory))
                ThrowNoAccessException();

            var fileItem = GetFileItem(item);
            fileItem.ParentId = ParseKey(destinationDirectory.Key);
            fileItem.Modified = DateTime.Now;
            fileItem.ModifiedById = GuestPersonId;
            FileManagementDbContext.SaveChanges();
        }

        public void CopyItem(FileSystemCopyItemOptions options) {
            var item = options.Item;
            var destinationDirectory = options.DestinationDirectory;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);
            if (!IsFileItemExists(destinationDirectory))
                ThrowItemNotFoundException(destinationDirectory);
            if (!AllowCopyOrMove(item, destinationDirectory))
                ThrowNoAccessException();

            var sourceFileItem = GetFileItem(item);
            var copyFileItem = CreateCopy(sourceFileItem);
            copyFileItem.ParentId = ParseKey(destinationDirectory.Key);
            copyFileItem.Name = GenerateCopiedFileItemName(copyFileItem.ParentId, copyFileItem.Name, copyFileItem.IsDirectory);
            FileManagementDbContext.FileItems.Add(copyFileItem);

            if (copyFileItem.IsDirectory)
                CopyDirectoryContentRecursive(sourceFileItem, copyFileItem);
            FileManagementDbContext.SaveChanges();
        }

        void CopyDirectoryContentRecursive(FileItem sourcePathInfo, FileItem destinationPathInfo) {
            foreach (var fileItem in GetDirectoryContents(sourcePathInfo.Id)) {
                var copyItem = CreateCopy(fileItem);
                copyItem.Parent = destinationPathInfo;
                FileManagementDbContext.FileItems.Add(copyItem);
                if (fileItem.IsDirectory)
                    CopyDirectoryContentRecursive(fileItem, copyItem);
            }
        }

        public void DeleteItem(FileSystemDeleteItemOptions options) {
            var item = options.Item;

            if (!IsFileItemExists(item))
                ThrowItemNotFoundException(item);

            var fileItem = GetFileItem(item);
            FileManagementDbContext.FileItems.Remove(fileItem);

            if (fileItem.IsDirectory)
                RemoveDirectoryContentRecursive(fileItem.Id);

            FileManagementDbContext.SaveChanges();
        }

        void RemoveDirectoryContentRecursive(int parenDirectoryKey) {
            var itemsToRemove = FileManagementDbContext
                .FileItems
                .Where(item => item.ParentId == parenDirectoryKey)
                .Select(item => new FileItem {
                    Id = item.Id,
                    IsDirectory = item.IsDirectory
                });
            foreach (var item in itemsToRemove) {
                FileManagementDbContext.FileItems.Remove(item);
            }

            foreach (var item in itemsToRemove) {
                if (!item.IsDirectory) continue;

                RemoveDirectoryContentRecursive(item.Id);
            }
        }

        IEnumerable<FileItem> GetDirectoryContents(int parentKey) {
            return FileManagementDbContext.FileItems
                .OrderByDescending(item => item.IsDirectory)
                .ThenBy(item => item.Name)
                .Where(items => items.ParentId == parentKey);
        }

        FileItem GetFileItem(FileSystemItemInfo item) {
            var itemId = ParseKey(item.Key);
            return FileManagementDbContext.FileItems.FirstOrDefault(i => i.Id == itemId);
        }

        bool IsFileItemExists(FileSystemItemInfo itemInfo) {
            var pathKeys = itemInfo.PathKeys.Select(key => ParseKey(key)).ToArray();
            var foundEntries = FileManagementDbContext.FileItems
                .Where(item => pathKeys.Contains(item.Id))
                .Select(item => new { item.Id, item.ParentId, item.Name, item.IsDirectory });

            var pathNames = itemInfo.Path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var isDirectoryExists = true;
            for (var i = 0; i < pathKeys.Length && isDirectoryExists; i++) {
                var entry = foundEntries.FirstOrDefault(e => e.Id == pathKeys[i]);
                isDirectoryExists = entry != null && entry.Name == pathNames[i] &&
                                    (i == 0 && entry.ParentId == 0 || entry.ParentId == pathKeys[i - 1]);
                if (isDirectoryExists && i < pathKeys.Length - 1)
                    isDirectoryExists = entry.IsDirectory;
            }
            return isDirectoryExists;
        }

        static bool AllowCopyOrMove(FileSystemItemInfo item, FileSystemItemInfo destinationDirectory) {
            if (destinationDirectory.PathKeys.Length < item.PathKeys.Length)
                return true;

            var isValid = false;
            for (var i = 0; i < destinationDirectory.PathKeys.Length && !isValid; i++) {
                isValid = destinationDirectory.PathKeys[i] != item.PathKeys[i];
            }
            return isValid;
        }

        static FileItem CreateCopy(FileItem fileItem) {
            return new FileItem {
                Name = fileItem.Name,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsDirectory = fileItem.IsDirectory,
                ModifiedById = GuestPersonId
            };
        }

        static int ParseKey(string key) {
            return string.IsNullOrEmpty(key) ? 0 : int.Parse(key);
        }

        string GenerateCopiedFileItemName(int parentDirKey, string copiedFileItemName, bool isDirectory) {
            var dirNames = GetDirectoryContents(parentDirKey)
                .Where(i => i.IsDirectory == isDirectory)
                .Select(i => i.Name);

            string newName;
            var fileExtension = isDirectory ? "" : Path.GetExtension(copiedFileItemName);
            var copyNamePrefix =
                isDirectory ? copiedFileItemName : Path.GetFileNameWithoutExtension(copiedFileItemName);
            var index = -1;
            do {
                newName = $"{copyNamePrefix} {(index < 1 ? "" : $"copy {index}")}{fileExtension}";
                index++;
            } while (dirNames.Contains(newName));
            return newName;
        }

        void ThrowItemNotFoundException(FileSystemItemInfo item) {
            var itemType = item.IsDirectory ? "Directory" : "File";
            var errorCode = item.IsDirectory ? FileSystemErrorCode.DirectoryNotFound : FileSystemErrorCode.FileNotFound;
            string message = $"{itemType} '{item.Path}' not found.";
            throw new FileSystemException(errorCode, message);
        }

        void ThrowNoAccessException() {
            string message = "Access denied. The operation cannot be completed.";
            throw new FileSystemException(FileSystemErrorCode.NoAccess, message);
        }
    }
}
