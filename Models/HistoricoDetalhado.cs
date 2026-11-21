using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberDesk.Models
{
    public sealed class HistoricoDetalhado
    {
        public HistoricoIncidente Registro { get; private set; }
        public string TituloIncidente { get; private set; }
        public string NomeAtivo { get; private set; }

        public HistoricoDetalhado(HistoricoIncidente registro, string tituloIncidente, string nomeAtivo)
        {
            if (registro == null)
            {
                throw new ArgumentNullException("registro");
            }
          
            if (string.IsNullOrWhiteSpace(tituloIncidente))
            {
                throw new ArgumentException("Titulo do incidente obrigatorio.", "tituloIncidente");
            }
               
            if (string.IsNullOrWhiteSpace(nomeAtivo))
            {
                throw new ArgumentException("Nome do ativo obrigatorio.", "nomeAtivo");
            }

            Registro = registro;
            TituloIncidente = tituloIncidente;
            NomeAtivo = nomeAtivo;
        }
    }
}
