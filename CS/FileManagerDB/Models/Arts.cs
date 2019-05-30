using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileManagerDB.Models {
    public partial class Arts {
        public DateTime? LastWriteTime { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public bool? IsFolder { get; set; }
        public byte[] Data { get; set; }
        public int? OptimisticLockField { get; set; }
        public int? Gcrecord { get; set; }
        public byte[] SsmaTimeStamp { get; set; }
    }
}
