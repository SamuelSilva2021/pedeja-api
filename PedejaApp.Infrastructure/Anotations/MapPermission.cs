using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Infrastructure.Anotations
{
    /// <summary>
    /// Mapeia a permissão necessária para acessar o método ou classe.
    /// </summary>
    public class MapPermission: Attribute
    {
        /// <summary>
        /// Módulo ao qual a permissão pertence.
        /// </summary>
        public string Modulo { get; set; }
        /// <summary>
        /// Operação específica dentro do módulo.
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// Construtor que inicializa a permissão com o módulo e a operação.
        /// </summary>
        /// <param name="modulo"></param>
        /// <param name="operation"></param>
        public MapPermission(string modulo, string operation)
        {
            Modulo = modulo;
            Operation = operation;
        }
    }
}
