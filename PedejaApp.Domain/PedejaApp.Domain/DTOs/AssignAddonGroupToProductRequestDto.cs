using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.DTOs
{
    public class AssignAddonGroupToProductRequestDto
    {
        public int DisplayOrder { get; set; } = 0;
        public bool IsRequired { get; set; } = false;
        public int? MinSelectionsOverride { get; set; }
        public int? MaxSelectionsOverride { get; set; }
    }
}
