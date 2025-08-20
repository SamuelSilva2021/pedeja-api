using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.Enuns
{
    public enum AddonGroupType
    {
        Single = 0,    // Permite apenas uma seleção (ex: "Escolha 1 cobertura")
        Multiple = 1   // Permite múltiplas seleções (ex: "Escolha até 3 frutas")
    }
}
