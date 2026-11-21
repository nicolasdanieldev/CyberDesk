using System;
using System.Collections.Generic;
using CyberDesk.Enums;
using CyberDesk.Models;

namespace CyberDesk.Services
{
    // Agrega indicadores a partir dos incidentes cadastrados.
    public class RelatorioService
    {
        public ResumoIncidentes GerarResumo(IEnumerable<Incidente> incidentes)
        {
            if (incidentes == null)
            {
                throw new ArgumentNullException("incidentes");
            }

            int total = 0, abertos = 0, emAnalise = 0, resolvidos = 0;
            int criticos = 0, altos = 0, moderados = 0, baixos = 0;

            foreach (Incidente incidente in incidentes)
            {
                if (incidente == null)
                {
                    throw new ArgumentException("A lista contém um incidente nulo.");
                }

                total++;
                switch (incidente.Status)
                {
                    case StatusIncidente.Aberto: abertos++; break;
                    case StatusIncidente.EmAnalise: emAnalise++; break;
                    case StatusIncidente.Resolvido: resolvidos++; break;
                }

                int pontuacao = CalculadoraRisco.Calcular(incidente);

                if (pontuacao >= 17) 
                {
                    criticos++;
                }
                else if (pontuacao >= 10) 
                {
                    altos++;
                }
                else if (pontuacao >= 5) 
                {
                    moderados++;
                }
                else 
                {
                    baixos++;
                }
            }

            return new ResumoIncidentes(total, abertos, emAnalise, resolvidos, criticos, altos, moderados, baixos);
        }
    }
}
