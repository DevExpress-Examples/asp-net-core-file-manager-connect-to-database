using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerDB.Models {
    public class User {
        [Key]
        public int UserId { get; set; }
        public string FullName { get; set; }
    }
}
