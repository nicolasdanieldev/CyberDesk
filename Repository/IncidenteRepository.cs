using System;
using System.Collections.Generic;
using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Exceptions;
using CyberDesk.Models;
using MySqlConnector;

namespace CyberDesk.Repositories
{
    // CRUD de incidentes e mudança de status com histórico transacional.
    public class IncidenteRepository
    {
        private readonly ConexaoMySql _banco;

        public IncidenteRepository(ConexaoMySql banco)
        {
            _banco = banco ?? throw new ArgumentNullException("banco");
        }

        public void Adicionar(Incidente incidente)
        {
            if (incidente == null)
            {
                throw new ArgumentNullException("incidente");
            }
            if (incidente.Id != 0)
            {
                throw new InvalidOperationException("O incidente já possui ID.");
            }   

            const string sql = @"INSERT INTO incidentes (ativo_id, titulo, descricao, impacto, probabilidade, status, data_abertura)
                                 VALUES (@ativo, @titulo, @descricao, @impacto, @probabilidade, @status, @data)";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                PreencherCampos(comando, incidente);
                comando.Parameters.AddWithValue("@status", (int)incidente.Status);
                comando.Parameters.AddWithValue("@data", incidente.DataAbertura);
                comando.ExecuteNonQuery();
                incidente.DefinirId(checked((int)comando.LastInsertedId));
            }
        }

        public Incidente BuscarPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }
            const string sql = @"SELECT id, ativo_id, titulo, descricao, impacto, probabilidade, status, data_abertura
                                 FROM incidentes WHERE id = @id";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@id", id);
                using (MySqlDataReader leitor = comando.ExecuteReader())
                    return leitor.Read() ? Mapear(leitor) : null;
            }
        }

        public List<Incidente> ListarTodos()
        {
            var incidentes = new List<Incidente>();
            const string sql = @"SELECT id, ativo_id, titulo, descricao, impacto,
                                        probabilidade, status, data_abertura
                                 FROM incidentes ORDER BY data_abertura DESC, id DESC";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            using (MySqlDataReader leitor = comando.ExecuteReader())
            {
                while (leitor.Read()) incidentes.Add(Mapear(leitor));
            }
            return incidentes;
        }

        public List<Incidente> ListarPorAtivo(int ativoId)
        {
            if (ativoId <= 0)
            {
                throw new ArgumentOutOfRangeException("ativoId");
            }
            var incidentes = new List<Incidente>();
            const string sql = @"SELECT id, ativo_id, titulo, descricao, impacto, probabilidade, status, data_abertura
                                 FROM incidentes WHERE ativo_id = @ativo
                                 ORDER BY data_abertura DESC, id DESC";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                comando.Parameters.AddWithValue("@ativo", ativoId);
                using (MySqlDataReader leitor = comando.ExecuteReader())
                {
                    while (leitor.Read()) incidentes.Add(Mapear(leitor));
                }
            }
            return incidentes;
        }

        // Atualiza os dados básicos. A mudança de status tem método específico,
        // que garante que cada alteração gere um registro de auditoria.
        public bool Atualizar(Incidente incidente)
        {
            if (incidente == null)
            {
                throw new ArgumentNullException("incidente");
            }
            if (incidente.Id <= 0)
            {
                throw new InvalidOperationException("Salve o incidente primeiro.");
            }

            const string sql = @"UPDATE incidentes SET ativo_id = @ativo, titulo = @titulo,
                                 descricao = @descricao, impacto = @impacto,
                                 probabilidade = @probabilidade WHERE id = @id";
            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlCommand comando = new MySqlCommand(sql, conexao))
            {
                PreencherCampos(comando, incidente);
                comando.Parameters.AddWithValue("@id", incidente.Id);
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
            using (MySqlCommand comando = new MySqlCommand("DELETE FROM incidentes WHERE id = @id", conexao))
            {
                comando.Parameters.AddWithValue("@id", id);
                return comando.ExecuteNonQuery() > 0;
            }
        }

        // SELECT ... FOR UPDATE bloqueia o registro até o fim da transação.
        // Assim o status e seu histórico são salvos juntos ou desfeitos juntos.
        public bool AlterarStatusComHistorico(int incidenteId, StatusIncidente novoStatus,string observacao)
        {
            if (incidenteId <= 0)
            {
                throw new ArgumentOutOfRangeException("incidenteId");
            }
            if (!Enum.IsDefined(typeof(StatusIncidente), novoStatus))
            {
                throw new ValidacaoCyberDeskException("Status inválido.");
            }
            if (string.IsNullOrWhiteSpace(observacao))
            {
                throw new ValidacaoCyberDeskException("Informe o motivo da alteração de status.");
            }

            using (MySqlConnection conexao = _banco.AbrirConexao())
            using (MySqlTransaction transacao = conexao.BeginTransaction())
            {
                try
                {
                    object statusAtual;
                    using (var leitura = new MySqlCommand("SELECT status FROM incidentes WHERE id = @id FOR UPDATE", conexao, transacao))
                    {
                        leitura.Parameters.AddWithValue("@id", incidenteId);
                        statusAtual = leitura.ExecuteScalar();
                    }

                    if (statusAtual == null || statusAtual == DBNull.Value)
                    {
                        transacao.Rollback();
                        return false;
                    }

                    var statusAnterior = (StatusIncidente)Convert.ToInt32(statusAtual);
                    if (statusAnterior == novoStatus)
                    {
                        transacao.Rollback();
                        return false;
                    }

                    // Valida o registro de histórico antes de persistir.
                    var historico = new HistoricoIncidente(incidenteId, statusAnterior, novoStatus, observacao);

                    using (var atualizacao = new MySqlCommand("UPDATE incidentes SET status = @status WHERE id = @id", conexao, transacao))
                    {
                        atualizacao.Parameters.AddWithValue("@status", (int)novoStatus);
                        atualizacao.Parameters.AddWithValue("@id", incidenteId);
                        atualizacao.ExecuteNonQuery();
                    }

                    using (var registro = new MySqlCommand(@"INSERT INTO historico_incidentes (incidente_id, status_anterior, status_novo, observacao, data_alteracao)
                          VALUES (@incidente, @anterior, @novo, @observacao, @data)",
                        conexao, transacao))
                    {
                        registro.Parameters.AddWithValue("@incidente", historico.IncidenteId);
                        registro.Parameters.AddWithValue("@anterior", (int)historico.StatusAnterior);
                        registro.Parameters.AddWithValue("@novo", (int)historico.StatusNovo);
                        registro.Parameters.AddWithValue("@observacao", historico.Observacao);
                        registro.Parameters.AddWithValue("@data", historico.DataAlteracao);
                        registro.ExecuteNonQuery();
                    }

                    transacao.Commit();
                    return true;
                }
                catch
                {
                    transacao.Rollback();
                    throw;
                }
            }
        }

        private static void PreencherCampos(MySqlCommand comando, Incidente incidente)
        {
            comando.Parameters.AddWithValue("@ativo", incidente.AtivoId);
            comando.Parameters.AddWithValue("@titulo", incidente.Titulo);
            comando.Parameters.AddWithValue("@descricao", incidente.Descricao);
            comando.Parameters.AddWithValue("@impacto", incidente.Impacto);
            comando.Parameters.AddWithValue("@probabilidade", incidente.Probabilidade);
        }

        private static Incidente Mapear(MySqlDataReader leitor)
        {
            return new Incidente(leitor.GetInt32(leitor.GetOrdinal("id")), leitor.GetInt32(leitor.GetOrdinal("ativo_id")),
                leitor.GetString(leitor.GetOrdinal("titulo")), leitor.GetString(leitor.GetOrdinal("descricao")),
                leitor.GetInt32(leitor.GetOrdinal("impacto")), leitor.GetInt32(leitor.GetOrdinal("probabilidade")),
                (StatusIncidente)Convert.ToInt32(leitor["status"]),
                leitor.GetDateTime(leitor.GetOrdinal("data_abertura")));
        }
    }
}
