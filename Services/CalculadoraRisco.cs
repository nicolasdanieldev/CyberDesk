using System;
using CyberDesk.Models;

namespace CyberDesk.Services
{
    // Concentra a regra de negócio de pontuação e classificação de riscos.
    public static class CalculadoraRisco
    {
        public static int Calcular(Incidente incidente)
        {
            if (incidente == null)
            {
                throw new ArgumentNullException("incidente");
            }

            return incidente.Impacto * incidente.Probabilidade;
        }

        public static string Classificar(Incidente incidente)
        {
            int pontuacao = Calcular(incidente);

            if (pontuacao <= 4)
            {
                return "Baixo";
            }
            if (pontuacao <= 9)
            {
                return "Moderado";
            }
            if (pontuacao <= 16)
            {
                return "Alto";
            }
            return "Crítico";
        }
    }
}
