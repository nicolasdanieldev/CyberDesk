using System;
using CyberDesk.Exceptions;

namespace CyberDesk.Models
{
    // Objeto de resultado para preencher os indicadores do dashboard.
    public class ResumoIncidentes
    {
        public int Total { get; private set; }
        public int Abertos { get; private set; }
        public int EmAnalise { get; private set; }
        public int Resolvidos { get; private set; }
        public int Criticos { get; private set; }
        public int Altos { get; private set; }
        public int Moderados { get; private set; }
        public int Baixos { get; private set; }

        public ResumoIncidentes(int total, int abertos, int emAnalise, int resolvidos,
                               int criticos, int altos, int moderados, int baixos)
        {
            if (total < 0 || abertos < 0 || emAnalise < 0 || resolvidos < 0 || criticos < 0 || altos < 0 || moderados < 0 || baixos < 0 ||
                abertos + emAnalise + resolvidos != total ||
                criticos + altos + moderados + baixos != total)
            {
                throw new ValidacaoCyberDeskException("Indicadores do relatório inconsistentes.");
            };

            Total = total;
            Abertos = abertos;
            EmAnalise = emAnalise;
            Resolvidos = resolvidos;
            Criticos = criticos;
            Altos = altos;
            Moderados = moderados;
            Baixos = baixos;
        }
    }
}
