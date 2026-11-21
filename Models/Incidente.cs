using System;
using CyberDesk.Enums;
using CyberDesk.Exceptions;

namespace CyberDesk.Models
{
    // Representa uma ocorrência de segurança associada a um ativo.
    public class Incidente
    {
        private int _ativoId;
        private string _titulo;
        private string _descricao;
        private int _impacto;
        private int _probabilidade;

        public int Id { get; private set; }

        public int AtivoId
        {
            get 
            { 
                return _ativoId; 
            }
            set
            {
                if (value <= 0)
                {
                    throw new ValidacaoCyberDeskException("Selecione um ativo cadastrado.");
                }
                _ativoId = value;
            }
        }

        public string Titulo
        {
            get 
            { 
                return _titulo; 
            }
            set 
            { 
                _titulo = ValidarTexto(value, "título", 150); 
            }
        }

        public string Descricao
        {
            get 
            { 
                return _descricao; 
            }
            set 
            { 
                _descricao = ValidarTexto(value, "descrição", 4000); 
            }
        }

        public int Impacto
        {
            get 
            { 
                return _impacto; 
            }
            set
            {
                ValidarNota(value, "impacto");
                _impacto = value;
            }
        }

        public int Probabilidade
        {
            get 
            { 
                return _probabilidade; 
            }
            set
            {
                ValidarNota(value, "probabilidade");
                _probabilidade = value;
            }
        }

        public StatusIncidente Status { get; private set; }
        public DateTime DataAbertura { get; private set; }

        // Construtor para um incidente novo.
        public Incidente(int ativoId, string titulo, string descricao, int impacto, int probabilidade)
            : this(0, ativoId, titulo, descricao, impacto, probabilidade,
                  StatusIncidente.Aberto, DateTime.Now)
        {
        }

        // Construtor para um incidente que já existe no MySQL.
        public Incidente(int id, int ativoId, string titulo, string descricao,
                         int impacto, int probabilidade, StatusIncidente status,
                         DateTime dataAbertura)
        {
            if (id < 0)
            {
                throw new ValidacaoCyberDeskException("O ID do incidente não pode ser negativo.");
            }
            if (dataAbertura == DateTime.MinValue)
            {
                throw new ValidacaoCyberDeskException("A data de abertura é inválida.");
            }

            Id = id;
            AtivoId = ativoId;
            Titulo = titulo;
            Descricao = descricao;
            Impacto = impacto;
            Probabilidade = probabilidade;
            AlterarStatus(status);
            DataAbertura = dataAbertura;
        }

        public void AlterarStatus(StatusIncidente novoStatus)
        {
            if (!Enum.IsDefined(typeof(StatusIncidente), novoStatus))
            {
                throw new ValidacaoCyberDeskException("Status do incidente inválido.");
            }

            Status = novoStatus;
        }

        public void DefinirId(int novoId)
        {
            if (novoId <= 0)
            {
                throw new ValidacaoCyberDeskException("O ID do incidente deve ser maior que zero.");
            }

            Id = novoId;
        }

        private static void ValidarNota(int valor, string campo)
        {
            if (valor < 1 || valor > 5)
            {
                throw new ValidacaoCyberDeskException("O campo " + campo + " deve estar entre 1 e 5.");
            }
        }

        private static string ValidarTexto(string valor, string campo, int tamanhoMaximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ValidacaoCyberDeskException("O campo " + campo + " é obrigatório.");
            }

            if (valor.Trim().Length > tamanhoMaximo)
            {
                throw new ValidacaoCyberDeskException("O campo " + campo + " deve ter no máximo " + tamanhoMaximo + " caracteres.");
            }

            return valor.Trim();
        }
    }
}
