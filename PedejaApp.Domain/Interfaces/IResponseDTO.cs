using PedejaApp.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.Interfaces
{
    public interface IResponseDTO
    {
        public bool Succeeded { get; }
        public int Code { get; }
        public IList<ErrorDTO> Errors { get; }
        public IDictionary<string, string> Headers { get; }
        dynamic? GetData();
        public string? RequestUrl { get; }
        public string? RequestBody { get; }
    }
}
