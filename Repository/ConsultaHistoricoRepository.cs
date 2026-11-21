using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberDesk.Repository
{
    public sealed class ConsultaHistoricoRepository
    {
        private readonly ConexaoMySql _banco;

        public ConsultaHistoricoRepository(ConexaoMySql banco)
        {
            _banco = banco ?? throw new ArgumentNullException("banco");
        }

        // Limite para evitar carregamento irrestrito num projeto desktop.
        // A tela deixa claro que exibe os ultimos 1000 registros, no maximo.
        public List<HistoricoDetalhado> ListarRecentes(int limite = 1000)
        {
            if (limite < 1 || limite > 1000)
            {
                throw new ArgumentOutOfRangeException("limite", "Informe um limite entre 1 e 1000.");
            }

            const string sql = @"SELECT h.id AS historico_id,
                                        h.incidente_id,
                                        h.status_anterior,
                                        h.status_novo,
                                        h.observacao,
                                        h.data_alteracao,
                                        i.titulo AS titulo_incidente,
                                        a.nome AS nome_ativo
                                 FROM historico_incidentes h
                                 INNER JOIN incidentes i ON i.id = h.incidente_id
                                 INNER JOIN ativos a ON a.id = i.ativo_id
                                 ORDER BY h.data_alteracao DESC, h.id DESC
                                 LIMIT @limite";

            var resultado = new List<HistoricoDetalhado>();
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@limite", limite);
                using (MySqlDataReader leitor = comando.ExecuteReader())
                {
                    while (leitor.Read())
                    {
                        var registro = new HistoricoIncidente(
                            leitor.GetInt32(leitor.GetOrdinal("historico_id")),
                            leitor.GetInt32(leitor.GetOrdinal("incidente_id")),
                            (StatusIncidente)Convert.ToInt32(leitor["status_anterior"]),
                            (StatusIncidente)Convert.ToInt32(leitor["status_novo"]),
                            leitor.GetString(leitor.GetOrdinal("observacao")),
                            leitor.GetDateTime(leitor.GetOrdinal("data_alteracao"))
                        );
                        resultado.Add(new HistoricoDetalhado(
                            registro,
                            leitor.GetString(leitor.GetOrdinal("titulo_incidente")),
                            leitor.GetString(leitor.GetOrdinal("nome_ativo"))));
                    }
                }
            }
            return resultado;
        }
    }
}
