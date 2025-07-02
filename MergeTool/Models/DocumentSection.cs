using System;

namespace MergeTool.Models
{
    public class DocumentSection
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public Document Document { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int SectionIndex { get; set; }

        public DateTime LastEditedAt { get; set; } = DateTime.UtcNow;
    }

}
