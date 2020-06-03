using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerDB.Models {
    public class FileManagementDbContext : DbContext {
        public FileManagementDbContext(DbContextOptions<FileManagementDbContext> options)
           : base(options) {
        }

        public DbSet<FileItem> FileItems { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
