using MergeTool.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MergeTool.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentSection> DocumentSections { get; set; }
    }
}
