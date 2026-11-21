using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Models;
using CyberDesk.Repository;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberDesk
{
    public partial class frmHistorico : Form
    {
        // Mesmas cores das telas anteriores.
        private static readonly Color Fundo = Color.FromArgb(10, 17, 30);
        private static readonly Color Menu = Color.FromArgb(16, 27, 45);
        private static readonly Color Card = Color.FromArgb(23, 36, 57);
        private static readonly Color Campo = Color.FromArgb(31, 48, 69);
        private static readonly Color Borda = Color.FromArgb(44, 63, 86);
        private static readonly Color Texto = Color.FromArgb(235, 243, 252);
        private static readonly Color Muted = Color.FromArgb(153, 174, 194);
        private static readonly Color Verde = Color.FromArgb(64, 212, 165);
        private static readonly Color Azul = Color.FromArgb(75, 164, 255);
        private static readonly Color Vermelho = Color.FromArgb(248, 108, 113);
        private static readonly Color Laranja = Color.FromArgb(255, 181, 83);

        private readonly List<HistoricoDetalhado> _todos = new List<HistoricoDetalhado>();
        private readonly List<HistoricoDetalhado> _visiveis = new List<HistoricoDetalhado>();
        private bool _carregado;
        private bool _atualizandoGrade;

        private TextBox txtBusca;
        private ComboBox cmbStatus;
        private ComboBox cmbPeriodo;
        private Button btnAtualizar;
        private Button btnExportar;
        private DataGridView dgvHistorico;
        private TextBox txtDetalhe;
        private Label lblRegistros;
        private Label lblResolvidos;
        private Label lblIncidentes;
        private Label lblQuantidade;
        private Label lblConexao;
        private Label lblSemDados;

        public frmHistorico()
        {
            InitializeComponent();
            ConfigurarJanela();
            CriarInterface();
            Shown += (s, e) => CarregarHistorico();
        }

        private void ConfigurarJanela()
        {
            Text = "CyberDesk | Historico e Auditoria";
            BackColor = Fundo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1420, 860);
            MinimumSize = new Size(1100, 690);
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    CarregarHistorico();
                    e.Handled = true;
                }
            };
        }

        private void CriarInterface()
        {
            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(25, 15, 25, 12),
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 105F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 127F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            raiz.Controls.Add(CriarCabecalho(), 0, 0);
            raiz.Controls.Add(CriarIndicadores(), 0, 1);
            raiz.Controls.Add(CriarFiltros(), 0, 2);
            raiz.Controls.Add(CriarTabela(), 0, 3);
            raiz.Controls.Add(CriarDetalhe(), 0, 4);
            raiz.Controls.Add(CriarRodape(), 0, 5);
            Controls.Add(raiz);
        }

        private Control CriarCabecalho()
        {
            var tabela = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
            tabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var textos = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            textos.Controls.Add(new Label
            {
                Text = "Historico e auditoria",
                AutoSize = true,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Texto,
                Location = new Point(0, 0)
            });
            textos.Controls.Add(new Label
            {
                Text = "Acompanhe as mudancas de status e seus motivos registrados no MySQL",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Muted,
                Location = new Point(2, 55)
            });
            tabela.Controls.Add(textos, 0, 0);

            var acoes = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            var btnVoltar = CriarBotao("←  Voltar ao painel", Menu, Texto);
            btnVoltar.Size = new Size(183, 42);
            btnVoltar.Location = new Point(1, 17);
            btnVoltar.Click += (s, e) => Close();
            acoes.Controls.Add(btnVoltar);
            tabela.Controls.Add(acoes, 1, 0);
            return tabela;
        }

        private Control CriarIndicadores()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Fundo,
                Margin = new Padding(0, 0, 0, 13)
            };
            for (int i = 0; i < 3; i++)
                painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            painel.Controls.Add(CriarCard("ALTERACOES CARREGADAS", "0", Azul, out lblRegistros), 0, 0);
            painel.Controls.Add(CriarCard("MUDANCAS PARA RESOLVIDO", "0", Verde, out lblResolvidos), 1, 0);
            painel.Controls.Add(CriarCard("INCIDENTES COM ALTERACOES", "0", Laranja, out lblIncidentes), 2, 0);
            return painel;
        }

        private Control CriarCard(string titulo, string inicial, Color destaque, out Label numero)
        {
            var painel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Margin = new Padding(0, 0, 12, 0)
            };
            painel.Controls.Add(new Label
            {
                Text = titulo,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 12)
            });
            numero = new Label
            {
                Text = inicial,
                ForeColor = destaque,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 34)
            };
            painel.Controls.Add(numero);
            return painel;
        }

        private Control CriarFiltros()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = Card,
                Padding = new Padding(13, 12, 13, 10),
                Margin = new Padding(0, 0, 0, 13)
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtBusca = new TextBox
            {
                Dock = DockStyle.Fill,
                MaxLength = 150,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Campo,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 11F)
            };
            cmbStatus = CriarCombo();
            cmbStatus.Items.AddRange(new object[]
            {
                "Todos", "Aberto", "Em analise", "Resolvido"
            });
            cmbStatus.SelectedIndex = 0;

            cmbPeriodo = CriarCombo();
            cmbPeriodo.Items.AddRange(new object[]
            {
                "Todo o periodo", "Ultimos 7 dias", "Ultimos 30 dias", "Ultimos 90 dias"
            });
            cmbPeriodo.SelectedIndex = 0;

            painel.Controls.Add(CriarCampo("BUSCAR INCIDENTE / ATIVO / MOTIVO", txtBusca), 0, 0);
            painel.Controls.Add(CriarCampo("NOVO STATUS", cmbStatus), 1, 0);
            painel.Controls.Add(CriarCampo("PERIODO", cmbPeriodo), 2, 0);

            btnAtualizar = CriarBotao("↻  Atualizar", Menu, Texto);
            btnAtualizar.Dock = DockStyle.Fill;
            btnAtualizar.Margin = new Padding(6, 24, 6, 3);
            btnAtualizar.Click += (s, e) => CarregarHistorico();
            painel.Controls.Add(btnAtualizar, 3, 0);

            btnExportar = CriarBotao("Exportar CSV", Verde, Fundo);
            btnExportar.Dock = DockStyle.Fill;
            btnExportar.Margin = new Padding(6, 24, 0, 3);
            btnExportar.Enabled = false;
            btnExportar.Click += (s, e) => ExportarCsv();
            painel.Controls.Add(btnExportar, 4, 0);

            txtBusca.TextChanged += (s, e) => AplicarFiltros();
            cmbStatus.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cmbPeriodo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            return painel;
        }

        private Control CriarTabela()
        {
            var painel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Padding = new Padding(13, 12, 13, 12),
                Margin = new Padding(0, 0, 0, 13)
            };
            dgvHistorico = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Card,
                BorderStyle = BorderStyle.None,
                GridColor = Borda,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 36 }
            };
            dgvHistorico.ColumnHeadersDefaultCellStyle.BackColor = Campo;
            dgvHistorico.ColumnHeadersDefaultCellStyle.ForeColor = Texto;
            dgvHistorico.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvHistorico.DefaultCellStyle.BackColor = Card;
            dgvHistorico.DefaultCellStyle.ForeColor = Texto;
            dgvHistorico.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 68, 92);
            dgvHistorico.DefaultCellStyle.SelectionForeColor = Texto;
            dgvHistorico.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(27, 42, 62);

            AdicionarColuna("colId", "ID", 50);
            AdicionarColuna("colData", "DATA/HORA", 145);
            AdicionarColuna("colIncidenteId", "INC. #", 65);
            AdicionarColuna("colTitulo", "INCIDENTE", 200);
            AdicionarColuna("colAtivo", "EQUIPAMENTO", 160);
            AdicionarColuna("colAnterior", "DE", 95);
            AdicionarColuna("colNovo", "PARA", 95);
            AdicionarColuna("colObservacao", "MOTIVO", 250);
            dgvHistorico.SelectionChanged += (s, e) => ExibirLinhaSelecionada();
            painel.Controls.Add(dgvHistorico);

            lblSemDados = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Nenhuma alteracao encontrada.\nAltere os filtros ou registre uma mudanca de status no frmIncidentes.",
                Visible = false
            };
            painel.Controls.Add(lblSemDados);
            return painel;
        }

        private Control CriarDetalhe()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(15, 8, 15, 9),
                Margin = new Padding(0, 0, 0, 10)
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            painel.Controls.Add(new Label
            {
                Text = "DETALHES DA ALTERACAO SELECIONADA",
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 0, 0);
            txtDetalhe = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                BackColor = Card,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 10F),
                Text = "Selecione uma linha na tabela para consultar os detalhes."
            };
            painel.Controls.Add(txtDetalhe, 0, 1);
            return painel;
        }

        private Control CriarRodape()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 410F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lblConexao = new Label
            {
                Text = "●  Aguardando consulta MySQL",
                Dock = DockStyle.Fill,
                ForeColor = Laranja,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            painel.Controls.Add(lblConexao, 0, 0);
            lblQuantidade = new Label
            {
                Text = "Nenhum registro carregado",
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleRight
            };
            painel.Controls.Add(lblQuantidade, 1, 0);
            return painel;
        }

        private static ComboBox CriarCombo()
        {
            return new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Campo,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat
            };
        }

        private static Control CriarCampo(string titulo, Control controle)
        {
            var coluna = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Card,
                Margin = new Padding(0, 0, 10, 0)
            };
            coluna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            coluna.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            coluna.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            coluna.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.2F, FontStyle.Bold)
            }, 0, 0);
            coluna.Controls.Add(controle, 0, 1);
            return coluna;
        }

        private static Button CriarBotao(string texto, Color fundo, Color corTexto)
        {
            var botao = new Button
            {
                Text = texto,
                BackColor = fundo,
                ForeColor = corTexto,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderSize = 0;
            return botao;
        }

        private void AdicionarColuna(string nome, string cabecalho, float peso)
        {
            dgvHistorico.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nome,
                HeaderText = cabecalho,
                FillWeight = peso,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private void CarregarHistorico()
        {
            Cursor = Cursors.WaitCursor;
            btnAtualizar.Enabled = false;
            btnExportar.Enabled = false;
            try
            {
                var banco = ConexaoMySql.DoAmbiente();
                var consulta = new ConsultaHistoricoRepository(banco);
                List<HistoricoDetalhado> dados = consulta.ListarRecentes(1000);

                _todos.Clear();
                _todos.AddRange(dados);
                _carregado = true;
                lblConexao.Text = "●  MySQL conectado — historico consultado";
                lblConexao.ForeColor = Verde;
                AtualizarIndicadores();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                _carregado = false;
                _todos.Clear();
                _visiveis.Clear();
                AtualizarIndicadores();
                _atualizandoGrade = true;
                try { dgvHistorico.Rows.Clear(); }
                finally { _atualizandoGrade = false; }
                lblSemDados.Visible = true;
                lblSemDados.BringToFront();
                txtDetalhe.Text = "Nao foi possivel consultar os registros.";
                lblConexao.Text = "●  Falha ao consultar o MySQL";
                lblConexao.ForeColor = Vermelho;
                lblQuantidade.Text = "Erro na consulta";
                string erro = ex is MySqlException
                    ? "A consulta ao banco MySQL falhou. Verifique a conexao, as permissoes e as tabelas.\n\n" + ex.Message
                    : ex.Message;
                MessageBox.Show(this, erro, "CyberDesk | Falha no historico",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAtualizar.Enabled = true;
                btnExportar.Enabled = _carregado && _visiveis.Count > 0;
                Cursor = Cursors.Default;
            }
        }

        private void AtualizarIndicadores()
        {
            lblRegistros.Text = _todos.Count.ToString();
            lblResolvidos.Text = _todos.Count(h => h.Registro.StatusNovo == StatusIncidente.Resolvido).ToString();
            lblIncidentes.Text = _todos.Select(h => h.Registro.IncidenteId).Distinct().Count().ToString();
        }

        private void AplicarFiltros()
        {
            if (!_carregado || dgvHistorico == null) return;
            string busca = txtBusca.Text.Trim();
            int status = cmbStatus.SelectedIndex;
            int periodo = cmbPeriodo.SelectedIndex;
            DateTime agora = DateTime.Now;
            DateTime? inicio = periodo == 1 ? agora.AddDays(-7)
            : periodo == 2 ? agora.AddDays(-30)
            : periodo == 3 ? agora.AddDays(-90)
            : (DateTime?)null;

            var filtrados = _todos.Where(item =>
            {
                if (status > 0 && (int)item.Registro.StatusNovo != status - 1)
                    return false;
                if (inicio.HasValue && item.Registro.DataAlteracao < inicio.Value)
                    return false;
                if (busca.Length == 0) return true;
                return Contem(item.TituloIncidente, busca)
                    || Contem(item.NomeAtivo, busca)
                    || Contem(item.Registro.Observacao, busca)
                    || item.Registro.IncidenteId.ToString().Contains(busca);
            }).ToList();

            _visiveis.Clear();
            _visiveis.AddRange(filtrados);
            _atualizandoGrade = true;
            try
            {
                dgvHistorico.Rows.Clear();
                foreach (HistoricoDetalhado item in _visiveis)
                {
                    HistoricoIncidente h = item.Registro;
                    int linha = dgvHistorico.Rows.Add(
                        h.Id,
                        h.DataAlteracao.ToString("dd/MM/yyyy HH:mm:ss"),
                        h.IncidenteId,
                        item.TituloIncidente,
                        item.NomeAtivo,
                        NomeStatus(h.StatusAnterior),
                        NomeStatus(h.StatusNovo),
                        h.Observacao
                    );
                    dgvHistorico.Rows[linha].Tag = item;
                    dgvHistorico.Rows[linha].Cells["colNovo"].Style.ForeColor =
                        h.StatusNovo == StatusIncidente.Resolvido ? Verde :
                        h.StatusNovo == StatusIncidente.EmAnalise ? Azul : Laranja;
                }
                dgvHistorico.ClearSelection();
            }
            finally { _atualizandoGrade = false; }

            lblSemDados.Visible = _visiveis.Count == 0;
            if (lblSemDados.Visible) lblSemDados.BringToFront();
            txtDetalhe.Text = "Selecione uma linha na tabela para consultar os detalhes.";
            lblQuantidade.Text = "Exibindo " + _visiveis.Count + " de " + _todos.Count +
                                 " registro(s) carregados" + (_todos.Count == 1000 ? " (limite de 1000)" : "");
            btnExportar.Enabled = _visiveis.Count > 0;
        }

        private void ExibirLinhaSelecionada()
        {
            if (_atualizandoGrade || dgvHistorico.SelectedRows.Count != 1) return;
            var selecionado = dgvHistorico.SelectedRows[0].Tag as HistoricoDetalhado;
            if (selecionado == null) return;

            HistoricoIncidente h = selecionado.Registro;
            txtDetalhe.Text =
                "Incidente #" + h.IncidenteId + ": " + selecionado.TituloIncidente +
                "   |   Equipamento: " + selecionado.NomeAtivo + Environment.NewLine +
                "Transicao: " + NomeStatus(h.StatusAnterior) + " → " + NomeStatus(h.StatusNovo) +
                "   |   Data: " + h.DataAlteracao.ToString("dd/MM/yyyy HH:mm:ss") + Environment.NewLine +
                "Motivo: " + h.Observacao;
        }

        private static bool Contem(string campo, string busca)
        {
            return campo != null &&
                   campo.IndexOf(busca, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static string NomeStatus(StatusIncidente status)
        {
            switch (status)
            {
                case StatusIncidente.Aberto: return "Aberto";
                case StatusIncidente.EmAnalise: return "Em analise";
                case StatusIncidente.Resolvido: return "Resolvido";
                default: return "Desconhecido";
            }
        }

        private void ExportarCsv()
        {
            if (_visiveis.Count == 0) return;
            using (var dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Exportar historico do CyberDesk";
                dialogo.Filter = "Arquivo CSV (*.csv)|*.csv";
                dialogo.DefaultExt = "csv";
                dialogo.AddExtension = true;
                dialogo.FileName = "cyberdesk_historico_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    // UTF-8 com BOM facilita leitura de acentos no Excel do Windows.
                    using (var arquivo = new StreamWriter(dialogo.FileName, false, new UTF8Encoding(true)))
                    {
                        arquivo.WriteLine("ID;Data;ID Incidente;Incidente;Equipamento;Status anterior;Status novo;Motivo");
                        foreach (HistoricoDetalhado item in _visiveis)
                        {
                            var h = item.Registro;
                            arquivo.WriteLine(string.Join(";", new[]
                            {
                                Csv(h.Id.ToString()),
                                Csv(h.DataAlteracao.ToString("dd/MM/yyyy HH:mm:ss")),
                                Csv(h.IncidenteId.ToString()),
                                Csv(item.TituloIncidente),
                                Csv(item.NomeAtivo),
                                Csv(NomeStatus(h.StatusAnterior)),
                                Csv(NomeStatus(h.StatusNovo)),
                                Csv(h.Observacao)
                            }));
                        }
                    }
                    MessageBox.Show(this,
                        "CSV exportado com " + _visiveis.Count + " registro(s).",
                        "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Nao foi possivel salvar o CSV.\n\n" + ex.Message,
                        "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string Csv(string valor)
        {
            // Escapa aspas/quebras de linha e evita interpretacao de campos
            // provenientes do usuario como formulas no Excel.
            string texto = (valor ?? "").Replace("\r", " ").Replace("\n", " ");
            string semEspacos = texto.TrimStart();
            if (semEspacos.Length > 0 && "=+-@".IndexOf(semEspacos[0]) >= 0)
                texto = "'" + texto;
            return "\"" + texto.Replace("\"", "\"\"") + "\"";
        }
    }
}
