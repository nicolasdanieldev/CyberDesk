using System;
using CyberDesk.Enums;
using CyberDesk.Exceptions;

namespace CyberDesk.Models
{
    // Auditoria: guarda uma mudança de status e seu motivo.
    public class HistoricoIncidente
    {
        private string _observacao;

        public int Id { get; private set; }
        public int IncidenteId { get; private set; }
        public StatusIncidente StatusAnterior { get; private set; }
        public StatusIncidente StatusNovo { get; private set; }
        public DateTime DataAlteracao { get; private set; }

        public string Observacao
        {
            get 
            { 
                return _observacao; 
            }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ValidacaoCyberDeskException("Informe uma observação sobre a alteração.");
                }

                if (value.Trim().Length > 500)
                {
                    throw new ValidacaoCyberDeskException("A observação deve ter até 500 caracteres.");
                }

                _observacao = value.Trim();
            }
        }

        // Novo registro de histórico (sem ID atribuído).
        public HistoricoIncidente(int incidenteId, StatusIncidente anterior,
                                  StatusIncidente novo, string observacao)
            : this(0, incidenteId, anterior, novo, observacao, DateTime.Now)
        {
        }

        // Registro recuperado do MySQL.
        public HistoricoIncidente(int id, int incidenteId, StatusIncidente anterior,
                                  StatusIncidente novo, string observacao, DateTime dataAlteracao)
        {
            if (id < 0 || incidenteId <= 0)
            {
                throw new ValidacaoCyberDeskException("Identificadores do histórico inválidos.");
            }
            if (!Enum.IsDefined(typeof(StatusIncidente), anterior) || !Enum.IsDefined(typeof(StatusIncidente), novo))
            {
                throw new ValidacaoCyberDeskException("Status do histórico inválido.");
            }
            if (anterior == novo)
            {
                throw new ValidacaoCyberDeskException("A alteração precisa mudar o status.");
            }
            if (dataAlteracao == DateTime.MinValue)
            {
                throw new ValidacaoCyberDeskException("Data do histórico inválida.");
            }

            Id = id;
            IncidenteId = incidenteId;
            StatusAnterior = anterior;
            StatusNovo = novo;
            Observacao = observacao;
            DataAlteracao = dataAlteracao;
        }
    }
}
