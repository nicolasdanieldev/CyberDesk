using System.Collections.Generic;
using CyberDesk.Enums;
using CyberDesk.Exceptions;
using CyberDesk.Models;

namespace CyberDesk.Services
{
    // Demonstra uso de Collection em memória. A persistência MySQL virá depois.
    public class GerenciadorIncidentes
    {
        private readonly List<Incidente> _incidentes = new List<Incidente>();

        public void Adicionar(Incidente incidente)
        {
            if (incidente == null)
            {
                throw new ValidacaoCyberDeskException("Não é possível adicionar um incidente nulo.");
            }

            _incidentes.Add(incidente);
        }

        public List<Incidente> ListarTodos()
        {
            // Cópia para não permitir alteração direta da lista interna.
            return new List<Incidente>(_incidentes);
        }

        public List<Incidente> ListarCriticos()
        {
            var criticos = new List<Incidente>();

            foreach (Incidente incidente in _incidentes)
            {
                if (CalculadoraRisco.Calcular(incidente) >= 17)
                {
                    criticos.Add(incidente);
                }
            }

            return criticos;
        }

        public int ContarPorStatus(StatusIncidente status)
        {
            int total = 0;

            foreach (Incidente incidente in _incidentes)
            {
                if (incidente.Status == status)
                {
                    total++;
                }
            }

            return total;
        }
    }
}
