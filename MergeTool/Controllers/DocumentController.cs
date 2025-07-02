using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MergeTool.Data;
using MergeTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordDoc = DocumentFormat.OpenXml.Wordprocessing;

namespace MergeTool.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var docs = await _context.Documents.ToListAsync();
            return View(docs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title)
        {
            var doc = new MergeTool.Models.Document { Title = title }; 
            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = doc.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Sections)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();

            doc.Sections = doc.Sections.OrderBy(s => s.SectionIndex).ToList();

            return View(doc);
        }

        public async Task<IActionResult> EditSection(int sectionId)
        {
            var section = await _context.DocumentSections.FindAsync(sectionId);
            if (section == null) return NotFound();
            return View(section);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSection(DocumentSection section)
        {
            var dbSection = await _context.DocumentSections.FindAsync(section.Id);
            if (dbSection == null)
            {
                _context.DocumentSections.Add(section);
            }
            else
            {
                dbSection.Title = section.Title;
                dbSection.Content = section.Content;
                dbSection.SectionIndex = section.SectionIndex;
                dbSection.LastEditedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = section.DocumentId });
        }

        public async Task<IActionResult> Download(int documentId)
        {
            var doc = await _context.Documents
                .Include(d => d.Sections)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null) return NotFound();

            var orderedSections = doc.Sections.OrderBy(s => s.SectionIndex).ToList();

            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new WordDoc.Document(new WordDoc.Body());

                int index = 0;
                foreach (var section in orderedSections)
                {
                    var altChunkId = $"AltChunkId{index++}";

                    string html = $@"
                    <html xmlns='http://www.w3.org/1999/xhtml'>
                        <head><meta charset='utf-8' /></head>
                        <body>
                            <h2>{section.Title}</h2>
                            {section.Content}
                            <br/>
                        </body>
                    </html>";

                    var altChunkPart = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Html, altChunkId);
                    using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
                    {
                        altChunkPart.FeedData(ms);
                    }

                    var altChunk = new AltChunk { Id = altChunkId };
                    mainPart.Document.Body.Append(altChunk);
                }

                mainPart.Document.Save();
            }

            stream.Position = 0;
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"{doc.Title}.docx"
            );
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Sections)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();

            _context.DocumentSections.RemoveRange(doc.Sections); 
            _context.Documents.Remove(doc); 

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewSection(int id)
        {
            var section = await _context.DocumentSections.FindAsync(id);
            if (section == null) return NotFound();

            return View(section); 
        }

    }
}
