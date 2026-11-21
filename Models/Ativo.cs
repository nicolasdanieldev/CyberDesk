using System;
using System.Net;
using CyberDesk.Exceptions;

namespace CyberDesk.Models
{
    // Representa um computador, servidor ou outro ativo de TI.
    public class Ativo
    {
        private string _nome;
        private string _enderecoIp;
        private string _setor;
        private string _responsavel;

        public int Id { get; private set; }

        public string Nome
        {
            get 
            { 
                return _nome; 
            }
            set 
            { 
                _nome = ValidarTexto(value, "nome", 120); 
            }
        }

        public string EnderecoIp
        {
            get 
            { 
                return _enderecoIp; 
            }
            set
            {
                IPAddress ip;

                if (string.IsNullOrWhiteSpace(value) || !IPAddress.TryParse(value, out ip))
                {
                    throw new ValidacaoCyberDeskException("Informe um endereço IP válido.");
                }

                _enderecoIp = value.Trim();
            }
        }

        public string Setor
        {
            get 
            { 
                return _setor; 
            }
            set 
            { 
                _setor = ValidarTexto(value, "setor", 100); 
            }
        }

        public string Responsavel
        {
            get 
            { 
                return _responsavel; 
            }
            set 
            { 
                _responsavel = ValidarTexto(value, "responsável", 120); 
            }
        }

        // Usado para cadastrar um ativo novo (sem ID de banco de dados).
        public Ativo(string nome, string enderecoIp, string setor, string responsavel)
            : this(0, nome, enderecoIp, setor, responsavel)
        {
        }

        // Usado também para reconstruir um ativo lido do MySQL.
        public Ativo(int id, string nome, string enderecoIp, string setor, string responsavel)
        {
            if (id < 0)
            {
                throw new ValidacaoCyberDeskException("O ID do ativo não pode ser negativo.");
            }

            Id = id;
            Nome = nome;
            EnderecoIp = enderecoIp;
            Setor = setor;
            Responsavel = responsavel;
        }

        // Recebe o AUTO_INCREMENT retornado pelo MySQL após o INSERT.
        public void DefinirId(int novoId)
        {
            if (novoId <= 0)
            {
                throw new ValidacaoCyberDeskException("O ID do ativo deve ser maior que zero.");
            }

            Id = novoId;
        }

        public override string ToString()
        {
            return Nome + " (" + EnderecoIp + ")";
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
