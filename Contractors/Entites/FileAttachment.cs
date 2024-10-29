
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contractors.Entites
{
    public class FileAttachment : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }
        public int StatusId { get; set; }
        public int FileTypeId { get; set; }
        // Computed property to expose Status as an enum
        public FileType Status
        {
            get => (FileType)StatusId;
            set => FileTypeId = (int)value;
        }
    }
}
