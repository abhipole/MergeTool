using System;
using System.Collections.Generic;

namespace MergeTool.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<DocumentSection> Sections { get; set; } = new();
    }

}
