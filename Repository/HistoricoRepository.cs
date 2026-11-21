using System;
using System.Collections.Generic;
using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Models;
using MySqlConnector;

namespace CyberDesk.Repositories
{
    // Apenas leitura: novos históricos são criados na transação do IncidenteRepository.
    public class HistoricoRepository
    {
        private readonly ConexaoMySql _banco;

        public HistoricoRepository(ConexaoMySql banco)
        {
            _banco = banco ?? throw new ArgumentNullException("banco");
        }

        public List<HistoricoIncidente> ListarPorIncidente(int incidenteId)
        {
            if (incidenteId <= 0)
            {
                throw new ArgumentOutOfRangeException("incidenteId");
            }

            var registros = new List<HistoricoIncidente>();
            const string sql = @"SELECT id, incidente_id, status_anterior, status_novo, observacao, data_alteracao FROM historico_incidentes
                                 WHERE incidente_id = @id ORDER BY data_alteracao DESC, id DESC";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@id", incidenteId);
                using (MySqlDataReader leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        registros.Add(new HistoricoIncidente(
                            leitor.GetInt32(leitor.GetOrdinal("id")), leitor.GetInt32(leitor.GetOrdinal("incidente_id")),
                            (StatusIncidente)Convert.ToInt32(leitor["status_anterior"]),
                            (StatusIncidente)Convert.ToInt32(leitor["status_novo"]),
                            leitor.GetString(leitor.GetOrdinal("observacao")),
                            leitor.GetDateTime(leitor.GetOrdinal("data_alteracao"))));
                    }
                }
            }
            return registros;
        }
    }
}
