using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Mvc.FileManagement;

namespace FileManagerDB.Models {
    public class DbFileSystemItem: IClientFileSystemItem {

        public string Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDirectory { get; set; }
        public byte[] FileData { get; set; }
        public long Size { get; set; }
        public IDictionary<string, object> CustomFields { get; set; }
        public bool HasSubDirectories { get; set; }
        public DbFileSystemItem() {
            CustomFields = new Dictionary<string, object>();
        }
    }
}
