using System;
using MySqlConnector;

namespace CyberDesk.Data
{
    // Centraliza a abertura de conexões. Nunca coloque senhas fixas no GitHub.
    public class ConexaoMySql
    {
        private readonly string _stringConexao;

        public ConexaoMySql(string stringConexao)
        {
            if (string.IsNullOrWhiteSpace(stringConexao))
            {
                throw new ArgumentException("Informe a string de conexão MySQL.", "stringConexao");
            }

            _stringConexao = stringConexao;
        }

        public static ConexaoMySql DoAmbiente()
        {
            string cyberDeskMysqlConnection = Environment.GetEnvironmentVariable("CYBERDESK_MYSQL_CONNECTION");

            if (string.IsNullOrWhiteSpace(cyberDeskMysqlConnection))
            {
                throw new InvalidOperationException("Configure a variável CYBERDESK_MYSQL_CONNECTION antes de abrir o banco.");
            }

            return new ConexaoMySql(cyberDeskMysqlConnection);
        }

        public MySqlConnection AbrirConexao()
        {
            var conexao = new MySqlConnection(_stringConexao);
            try
            {
                conexao.Open();
                return conexao;
            }
            catch
            {
                conexao.Dispose();
                throw;
            }
        }
    }
}
