using System;
using System.Collections.Generic;
using CyberDesk.Data;
using CyberDesk.Models;
using MySqlConnector;

namespace CyberDesk.Repositories
{
    // CRUD de ativos no MySQL. Não contém regras de interface gráfica.
    public class AtivoRepository
    {
        private readonly ConexaoMySql _banco;

        public AtivoRepository(ConexaoMySql banco)
        {
            _banco = banco ?? throw new ArgumentNullException("banco");
        }

        public void Adicionar(Ativo ativo)
        {
            if (ativo == null)
            {
                throw new ArgumentNullException("ativo");
            }
            if (ativo.Id != 0)
            {
                throw new InvalidOperationException("O ativo já possui ID.");
            }

            const string sql = @"INSERT INTO ativos (nome, endereco_ip, setor, responsavel) VALUES (@nome, @ip, @setor, @responsavel)";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@nome", ativo.Nome);
                comando.Parameters.AddWithValue("@ip", ativo.EnderecoIp);
                comando.Parameters.AddWithValue("@setor", ativo.Setor);
                comando.Parameters.AddWithValue("@responsavel", ativo.Responsavel);
                comando.ExecuteNonQuery();
                ativo.DefinirId(checked((int)comando.LastInsertedId));
            }
        }

        public Ativo BuscarPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }

            const string sql = @"SELECT id, nome, endereco_ip, setor, responsavel FROM ativos WHERE id = @id";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@id", id);
                using (MySqlDataReader leitor = comando.ExecuteReader())
                    return leitor.Read() ? Mapear(leitor) : null;
            }
        }

        public List<Ativo> ListarTodos()
        {
            var ativos = new List<Ativo>();

            const string sql = @"SELECT id, nome, endereco_ip, setor, responsavel FROM ativos ORDER BY nome";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            using (MySqlDataReader leitor = comando.ExecuteReader())
            {
                while (leitor.Read()) ativos.Add(Mapear(leitor));
            }
            return ativos;
        }

        public bool Atualizar(Ativo ativo)
        {
            if (ativo == null)
            {
                throw new ArgumentNullException("ativo");
            }

            if (ativo.Id <= 0)
            {
                throw new InvalidOperationException("Salve o ativo primeiro.");
            }

            const string sql = @"UPDATE ativos SET nome = @nome, endereco_ip = @ip, setor = @setor, responsavel = @responsavel WHERE id = @id";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@id", ativo.Id);
                comando.Parameters.AddWithValue("@nome", ativo.Nome);
                comando.Parameters.AddWithValue("@ip", ativo.EnderecoIp);
                comando.Parameters.AddWithValue("@setor", ativo.Setor);
                comando.Parameters.AddWithValue("@responsavel", ativo.Responsavel);
                return comando.ExecuteNonQuery() > 0;
            }
        }

        public bool Excluir(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand("DELETE FROM ativos WHERE id = @id", conexao))
            {
                comando.Parameters.AddWithValue("@id", id);
                return comando.ExecuteNonQuery() > 0;
            }
        }

        private static Ativo Mapear(MySqlDataReader leitor)
        {
            return new Ativo(leitor.GetInt32(leitor.GetOrdinal("id")), leitor.GetString(leitor.GetOrdinal("nome")),
                             leitor.GetString(leitor.GetOrdinal("endereco_ip")), leitor.GetString(leitor.GetOrdinal("setor")),
                             leitor.GetString(leitor.GetOrdinal("responsavel")));
        }
    }
}
