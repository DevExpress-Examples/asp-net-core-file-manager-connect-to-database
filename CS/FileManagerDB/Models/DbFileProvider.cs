using DevExtreme.AspNet.Mvc.FileManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace FileManagerDB.Models {
    public class DbFileProvider : IFileProvider {
        const int DbRootItemId = -1;
        static readonly char[] PossibleDirectorySeparators = { '\\', '/' };
        ArtsDBContext DataContext { get; }
        public DbFileProvider(ArtsDBContext _context) {
            DataContext = _context;
        }
        public void Copy(string sourceKey, string destinationKey) {
            Arts sourceItem = GetDbItemByFileKey(sourceKey);
            Arts targetItem = GetDbItemByFileKey(Path.GetDirectoryName(destinationKey));
            if (targetItem.Id == sourceItem.ParentId)
                throw new SecurityException("You can't copy to the same folder.");
            List<Arts> childItems = DataContext.Arts.Where(p => p.ParentId == targetItem.Id).ToList();
            if (childItems.Select(i => i.Name).Contains(sourceItem.Name))
                throw new SecurityException("The folder contains an item with the same name.");
            CopyFolderInternal(sourceItem, targetItem);
        }
        void CopyFolderInternal(Arts sourceItem, Arts targetFolder) {
            if (!(bool)sourceItem.IsFolder)
                copy(sourceItem, targetFolder);
            else {
                List<Arts> childItems = DataContext.Arts.Where(p => p.ParentId == sourceItem.Id).ToList();
                var newFolder = copy(sourceItem, targetFolder);
                foreach (Arts item in childItems)
                    CopyFolderInternal(item, newFolder);
            }
        }
        Arts copy(Arts sourceItem, Arts targetItem) {
            Arts copyItem = new Arts {
                Data = sourceItem.Data,
                LastWriteTime = DateTime.Now,
                IsFolder = sourceItem.IsFolder,
                Name = sourceItem.Name,
                ParentId = targetItem.Id
            };
            DataContext.Arts.Add(copyItem);
            DataContext.SaveChanges();
            return copyItem;
        }
        public void CreateDirectory(string rootKey, string name) {
            Arts parentItem = GetDbItemByFileKey(rootKey);
            Arts newFolderItem = new Arts {
                Name = name,
                ParentId = parentItem.Id,
                IsFolder = true,
                LastWriteTime = DateTime.Now
            };
            DataContext.Arts.Add(newFolderItem);
            DataContext.SaveChanges();
        }

        public IList<IClientFileSystemItem> GetDirectoryContents(string dirKey) {
            List<IClientFileSystemItem> dirClientFileSystemItems = new List<IClientFileSystemItem>();
            Arts parent = GetDbItemByFileKey(dirKey);
            return DataContext.Arts
                .Where(p => p.ParentId == parent.Id)
                .Select(CreateDbFileSystemItem)
                .ToList<IClientFileSystemItem>();
        }

        public void Move(string sourceKey, string destinationKey) {
            Arts sourceItem = GetDbItemByFileKey(sourceKey);
            Arts targetItem = GetDbItemByFileKey(Path.GetDirectoryName(destinationKey));
            if (targetItem.Id == sourceItem.ParentId)
                throw new SecurityException("You can't copy to the same folder.");
            List<Arts> childItems = DataContext.Arts.Where(p => p.ParentId == targetItem.Id).ToList();
            if (childItems.Select(i => i.Name).Contains(sourceItem.Name))
                throw new SecurityException("The folder contains an item with the same name.");
            sourceItem.ParentId = targetItem.Id;
            DataContext.SaveChanges();
        }

        public void MoveUploadedFile(FileInfo file, string destinationKey) {
            string itemName = Path.GetFileName(destinationKey);
            byte[] data = new byte[file.Length];
            using (FileStream fs = file.OpenRead()) {
                fs.Read(data, 0, data.Length);
            }
            file.Delete();
            Arts parentItem = GetDbItemByFileKey(Path.GetDirectoryName(destinationKey));
            Arts item = new Arts {
                Name = itemName,
                ParentId = parentItem.Id,
                Data = data,
                IsFolder = false,
                LastWriteTime = DateTime.Now
            };
            DataContext.Arts.Add(item);
            DataContext.SaveChanges();
        }

        public void Remove(string key) {
            Arts item = GetDbItemByFileKey(key);
            if (item.Id == DbRootItemId)
                throw new SecurityException("You can't delete the root folder.");
            RemoveInternal(item);
        }

        void RemoveInternal(Arts sourceItem) {
            if (!(bool)sourceItem.IsFolder) {
                remove(sourceItem);
            } else {
                List<Arts> childItems = DataContext.Arts.Where(p => p.ParentId == sourceItem.Id).ToList();
                remove(sourceItem);
                foreach (Arts item in childItems) 
                    RemoveInternal(item);
            }
        }
        void remove(Arts item) {
            DataContext.Arts.Remove(item);
            DataContext.SaveChanges();
        }

        public void RemoveUploadedFile(FileInfo file) {
            file.Delete();
        }

        public void Rename(string key, string newName) {
            Arts item = GetDbItemByFileKey(key);
            if (item.ParentId == DbRootItemId)
                throw new SecurityException("You can't rename the root folder.");
            DataContext.Arts.Find(item.Id).Name = newName;
            DataContext.SaveChanges();
        }

        Arts GetDbItemByFileKey(string fileKey) {
            if (string.IsNullOrEmpty(fileKey) || fileKey == "\\")
                return DataContext.Arts.Where(p => p.ParentId == DbRootItemId).FirstOrDefault();
            string[] pathParts = fileKey.Split(PossibleDirectorySeparators);
            var query = DataContext.Arts.Where(item => (bool)item.IsFolder && item.Name == pathParts.First());
            var childItemsQuery = DataContext.Arts.Where(item => item.ParentId != null);
            for (int i = 1; i < pathParts.Length; i++) {
                string itemName = pathParts[i];
                query = childItemsQuery.
                 Join(query,
                  childItem => childItem.ParentId,
                  parentItem => parentItem.Id,
                  (childItem, parentItem) => childItem).
                 Where(item => item.Name == itemName);
            }
            return query.FirstOrDefault();

        }
        DbFileSystemItem CreateDbFileSystemItem(Arts dbItem) {
            return new DbFileSystemItem {
                Id = dbItem.Id.ToString(),
                ParentId = dbItem.ParentId == null ? DbRootItemId : (int)dbItem.ParentId,
                Name = dbItem.Name,
                DateModified = (DateTime)dbItem.LastWriteTime,
                IsDirectory = (bool)dbItem.IsFolder,
                FileData = dbItem.Data,
                Size = dbItem.Data == null ? 0 : dbItem.Data.Length,
                HasSubDirectories = DataContext.Arts.Where(i => i.ParentId == dbItem.Id && i.IsFolder == true).Count() > 0
            };
        }
    }
}
