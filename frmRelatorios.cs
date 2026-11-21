using CyberDesk.Controls;
using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Models;
using CyberDesk.Repositories;
using CyberDesk.Services;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberDesk
{
    public partial class frmRelatorios : Form
    {
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
        private static readonly Color Roxo = Color.FromArgb(158, 139, 255);

        private readonly RelatorioService _servico = new RelatorioService();
        private List<Incidente> _todos = new List<Incidente>();
        private List<Ativo> _ativos = new List<Ativo>();
        private List<Incidente> _filtrados = new List<Incidente>();
        private Dictionary<int, string> _nomesAtivos = new Dictionary<int, string>();
        private bool _carregado;
        private bool _atualizandoFiltros;

        private ComboBox cmbPeriodo;
        private ComboBox cmbStatus;
        private ComboBox cmbAtivo;
        private Button btnAtualizar;
        private Button btnExportar;
        private Label lblTotal;
        private Label lblAbertos;
        private Label lblCriticos;
        private Label lblResolvidos;
        private Label lblTaxa;
        private Label lblPeriodo;
        private Label lblStatus;
        private Label lblTabela;
        private DataGridView dgvEquipamentos;
        private GraficoBarras graficoRiscos;
        private GraficoBarras graficoStatus;

        public frmRelatorios()
        {
            InitializeComponent();
            ConfigurarJanela();
            CriarInterface();
            Shown += (s, e) => CarregarDados();
        }

        private void ConfigurarJanela()
        {
            Text = "CyberDesk | Relatórios e Indicadores";
            BackColor = Fundo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 10F);
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1420, 850);
            MinimumSize = new Size(1120, 735);
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    CarregarDados();
                    e.Handled = true;
                }
            };
        }

        private void CriarInterface()
        {
            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                Padding = new Padding(24, 13, 24, 10),
                ColumnCount = 1,
                RowCount = 6,
                Margin = Padding.Empty
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            raiz.Controls.Add(CriarCabecalho(), 0, 0);
            raiz.Controls.Add(CriarIndicadores(), 0, 1);
            raiz.Controls.Add(CriarFiltros(), 0, 2);
            raiz.Controls.Add(CriarGraficos(), 0, 3);
            raiz.Controls.Add(CriarTabela(), 0, 4);
            raiz.Controls.Add(CriarRodape(), 0, 5);
            Controls.Add(raiz);
        }

        private Control CriarCabecalho()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var titulo = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            titulo.Controls.Add(new Label
            {
                Text = "Relatórios de segurança",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Texto,
                Location = new Point(0, 2)
            });
            titulo.Controls.Add(new Label
            {
                Text = "Indicadores e distribuição de incidentes cadastrados no MySQL",
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(1, 56)
            });
            painel.Controls.Add(titulo, 0, 0);

            var voltar = CriarBotao("←  Voltar ao painel", Menu, Texto);
            voltar.Size = new Size(181, 42);
            voltar.Location = new Point(4, 17);
            voltar.Click += (s, e) => Close();
            var botoes = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            botoes.Controls.Add(voltar);
            painel.Controls.Add(botoes, 1, 0);
            return painel;
        }

        private Control CriarIndicadores()
        {
            var tabela = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Fundo
            };
            for (int i = 0; i < 4; i++)
                tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabela.Controls.Add(CriarCard("INCIDENTES NO FILTRO", Azul, out lblTotal), 0, 0);
            tabela.Controls.Add(CriarCard("EM ABERTO", Laranja, out lblAbertos), 1, 0);
            tabela.Controls.Add(CriarCard("RISCO CRÍTICO", Vermelho, out lblCriticos), 2, 0);
            tabela.Controls.Add(CriarCard("RESOLVIDOS", Verde, out lblResolvidos), 3, 0);
            return tabela;
        }

        private Control CriarCard(string titulo, Color cor, out Label numero)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Margin = new Padding(0, 0, 12, 0)
            };
            card.Controls.Add(new Label
            {
                Text = titulo,
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.3F, FontStyle.Bold),
                Location = new Point(15, 10)
            });
            numero = new Label
            {
                Text = "0",
                AutoSize = true,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = cor,
                Location = new Point(12, 31)
            };
            card.Controls.Add(numero);
            return card;
        }

        private Control CriarFiltros()
        {
            var area = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Padding = new Padding(14, 10, 14, 8),
                ColumnCount = 5,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 12)
            };
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            area.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            cmbPeriodo = CriarCombo();
            cmbPeriodo.Items.AddRange(new object[]
            {
                "Todo o período", "Últimos 7 dias", "Últimos 30 dias", "Últimos 90 dias"
            });
            cmbPeriodo.SelectedIndex = 0;

            cmbStatus = CriarCombo();
            cmbStatus.Items.AddRange(new object[]
            {
                "Todos", "Aberto", "Em análise", "Resolvido"
            });
            cmbStatus.SelectedIndex = 0;

            cmbAtivo = CriarCombo();
            cmbAtivo.Items.Add("Todos os equipamentos");
            cmbAtivo.SelectedIndex = 0;

            area.Controls.Add(CriarCampo("PERÍODO DE ABERTURA", cmbPeriodo), 0, 0);
            area.Controls.Add(CriarCampo("STATUS ATUAL", cmbStatus), 1, 0);
            area.Controls.Add(CriarCampo("EQUIPAMENTO", cmbAtivo), 2, 0);

            btnAtualizar = CriarBotao("↻  Atualizar", Menu, Texto);
            btnAtualizar.Dock = DockStyle.Fill;
            btnAtualizar.Margin = new Padding(7, 25, 7, 2);
            btnAtualizar.Click += (s, e) => CarregarDados();
            area.Controls.Add(btnAtualizar, 3, 0);

            btnExportar = CriarBotao("Exportar CSV", Verde, Fundo);
            btnExportar.Dock = DockStyle.Fill;
            btnExportar.Margin = new Padding(7, 25, 0, 2);
            btnExportar.Enabled = false;
            btnExportar.Click += (s, e) => ExportarCsv();
            area.Controls.Add(btnExportar, 4, 0);

            cmbPeriodo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cmbStatus.SelectedIndexChanged += (s, e) => AplicarFiltros();
            cmbAtivo.SelectedIndexChanged += (s, e) => AplicarFiltros();
            return area;
        }

        private static Control CriarCampo(string titulo, Control controle)
        {
            var campo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Card,
                Margin = new Padding(0, 0, 12, 0)
            };
            campo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            campo.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            campo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            campo.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            controle.Dock = DockStyle.Fill;
            controle.Margin = new Padding(0, 0, 0, 5);
            campo.Controls.Add(controle, 0, 1);
            return campo;
        }

        private static ComboBox CriarCombo()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Campo,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 10F),
                IntegralHeight = false
            };
        }

        private static Button CriarBotao(string titulo, Color fundo, Color texto)
        {
            var botao = new Button
            {
                Text = titulo,
                BackColor = fundo,
                ForeColor = texto,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            botao.FlatAppearance.BorderSize = 0;
            return botao;
        }

        private Control CriarGraficos()
        {
            var tabela = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = new Padding(0, 0, 0, 11)
            };
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            graficoRiscos = new GraficoBarras { Dock = DockStyle.Fill };
            graficoStatus = new GraficoBarras { Dock = DockStyle.Fill };
            tabela.Controls.Add(CriarPainelGrafico("INCIDENTES POR NÍVEL DE RISCO", graficoRiscos, true), 0, 0);
            tabela.Controls.Add(CriarPainelGrafico("INCIDENTES POR STATUS", graficoStatus, false), 1, 0);
            return tabela;
        }

        private static Control CriarPainelGrafico(string titulo, GraficoBarras grafico, bool primeiro)
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Card,
                Padding = new Padding(15, 10, 15, 8),
                Margin = primeiro ? new Padding(0, 0, 7, 0) : new Padding(7, 0, 0, 0)
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            painel.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
            }, 0, 0);
            painel.Controls.Add(grafico, 0, 1);
            return painel;
        }

        private Control CriarTabela()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(16, 10, 16, 8),
                BackColor = Card,
                Margin = Padding.Empty
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var linha = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Card
            };
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            linha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            linha.Controls.Add(new Label
            {
                Text = "Equipamentos com mais incidentes",
                Dock = DockStyle.Fill,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            }, 0, 0);
            lblTabela = new Label
            {
                Text = "Sem registros",
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                TextAlign = ContentAlignment.TopRight,
                Font = new Font("Segoe UI", 9F)
            };
            linha.Controls.Add(lblTabela, 1, 0);
            painel.Controls.Add(linha, 0, 0);

            dgvEquipamentos = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                BackgroundColor = Card,
                GridColor = Borda,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 31 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5F),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Card,
                    ForeColor = Texto,
                    SelectionBackColor = Color.FromArgb(45, 67, 91),
                    SelectionForeColor = Texto,
                    Padding = new Padding(7, 2, 7, 2)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Menu,
                    ForeColor = Muted,
                    SelectionBackColor = Menu,
                    SelectionForeColor = Muted,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Padding = new Padding(7, 2, 7, 2)
                }
            };
            AdicionarColuna("Equipamento", 47);
            AdicionarColuna("Total", 12);
            AdicionarColuna("Abertos", 12);
            AdicionarColuna("Críticos", 14);
            AdicionarColuna("Resolvidos", 15);
            painel.Controls.Add(dgvEquipamentos, 0, 1);
            return painel;
        }

        private void AdicionarColuna(string titulo, float peso)
        {
            dgvEquipamentos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = titulo,
                Name = titulo,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = peso,
                MinimumWidth = 60,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private Control CriarRodape()
        {
            var linha = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            linha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lblStatus = new Label
            {
                Text = "Aguardando consulta ao MySQL...",
                ForeColor = Muted,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F)
            };
            lblPeriodo = new Label
            {
                Text = "Taxa de resolução: 0%",
                ForeColor = Verde,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            lblTaxa = lblPeriodo;
            linha.Controls.Add(lblStatus, 0, 0);
            linha.Controls.Add(lblPeriodo, 1, 0);
            return linha;
        }

        private void CarregarDados()
        {
            if (btnAtualizar == null) return;
            btnAtualizar.Enabled = false;
            btnExportar.Enabled = false;
            lblStatus.Text = "Consultando incidentes e equipamentos...";
            Cursor = Cursors.WaitCursor;

            try
            {
                var banco = ConexaoMySql.DoAmbiente();
                // Usamos as consultas reais das classes já desenvolvidas na Etapa 2.
                var incidentes = new IncidenteRepository(banco).ListarTodos();
                var ativos = new AtivoRepository(banco).ListarTodos();

                _todos = incidentes;
                _ativos = ativos;
                _nomesAtivos = ativos.ToDictionary(a => a.Id, a => a.Nome);
                _carregado = true;

                PreencherComboAtivos();
                AplicarFiltros();
                lblStatus.Text = "MySQL conectado • atualizado em " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            }
            catch (Exception ex)
            {
                _carregado = false;
                _todos.Clear();
                _ativos.Clear();
                _filtrados.Clear();
                _nomesAtivos.Clear();
                LimparResultados();
                lblStatus.Text = "Não foi possível carregar os relatórios.";
                string mensagem = ex is MySqlException
                    ? "Falha na consulta ao MySQL. Verifique se o servidor está ativo, se o banco cyberdesk existe e se a conexão tem permissão de leitura.\n\nDetalhes: " + ex.Message
                    : "Não foi possível obter os relatórios.\n\nDetalhes: " + ex.Message;
                MessageBox.Show(mensagem, "CyberDesk | Relatórios",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAtualizar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void PreencherComboAtivos()
        {
            var selecionado = cmbAtivo.SelectedItem as Ativo;
            int idAnterior = selecionado == null ? 0 : selecionado.Id;
            _atualizandoFiltros = true;
            try
            {
                cmbAtivo.BeginUpdate();
                try
                {
                    cmbAtivo.Items.Clear();
                    cmbAtivo.Items.Add("Todos os equipamentos");
                    foreach (Ativo ativo in _ativos.OrderBy(a => a.Nome))
                        cmbAtivo.Items.Add(ativo);
                    int indice = 0;
                    for (int i = 1; i < cmbAtivo.Items.Count; i++)
                    {
                        var item = cmbAtivo.Items[i] as Ativo;
                        if (item != null && item.Id == idAnterior) { indice = i; break; }
                    }
                    cmbAtivo.SelectedIndex = indice;
                }
                finally { cmbAtivo.EndUpdate(); }
            }
            finally { _atualizandoFiltros = false; }
        }

        private void AplicarFiltros()
        {
            if (!_carregado || _atualizandoFiltros) return;

            IEnumerable<Incidente> consulta = _todos;
            if (cmbPeriodo.SelectedIndex > 0)
            {
                int dias = cmbPeriodo.SelectedIndex == 1 ? 7 :
                           cmbPeriodo.SelectedIndex == 2 ? 30 : 90;
                DateTime inicio = DateTime.Today.AddDays(1 - dias);
                DateTime fim = DateTime.Today.AddDays(1);
                consulta = consulta.Where(i => i.DataAbertura >= inicio && i.DataAbertura < fim);
            }
            if (cmbStatus.SelectedIndex > 0)
            {
                var escolhido = (StatusIncidente)(cmbStatus.SelectedIndex - 1);
                consulta = consulta.Where(i => i.Status == escolhido);
            }
            var ativoSelecionado = cmbAtivo.SelectedItem as Ativo;
            if (ativoSelecionado != null)
                consulta = consulta.Where(i => i.AtivoId == ativoSelecionado.Id);

            _filtrados = consulta.OrderByDescending(i => i.DataAbertura).ThenByDescending(i => i.Id).ToList();
            AtualizarResultados();
        }

        private void AtualizarResultados()
        {
            var resumo = _servico.GerarResumo(_filtrados);
            lblTotal.Text = resumo.Total.ToString("N0");
            lblAbertos.Text = resumo.Abertos.ToString("N0");
            lblCriticos.Text = resumo.Criticos.ToString("N0");
            lblResolvidos.Text = resumo.Resolvidos.ToString("N0");
            double taxa = resumo.Total == 0 ? 0 : 100.0 * resumo.Resolvidos / resumo.Total;
            lblTaxa.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"),
                "Taxa de resolução: {0:N1}%", taxa);

            graficoRiscos.AtualizarItens(new[]
            {
                new ItemGrafico("Crítico", resumo.Criticos, Vermelho),
                new ItemGrafico("Alto", resumo.Altos, Laranja),
                new ItemGrafico("Moderado", resumo.Moderados, Azul),
                new ItemGrafico("Baixo", resumo.Baixos, Verde)
            });
            graficoStatus.AtualizarItens(new[]
            {
                new ItemGrafico("Aberto", resumo.Abertos, Laranja),
                new ItemGrafico("Em análise", resumo.EmAnalise, Roxo),
                new ItemGrafico("Resolvido", resumo.Resolvidos, Verde)
            });

            var linhas = _filtrados.GroupBy(i => i.AtivoId)
                .Select(g => new
                {
                    Nome = NomeAtivo(g.Key),
                    Total = g.Count(),
                    Abertos = g.Count(i => i.Status == StatusIncidente.Aberto),
                    Criticos = g.Count(i => CalculadoraRisco.Calcular(i) >= 17),
                    Resolvidos = g.Count(i => i.Status == StatusIncidente.Resolvido)
                })
                .OrderByDescending(g => g.Total)
                .ThenBy(g => g.Nome)
                .Take(10)
                .ToList();

            dgvEquipamentos.Rows.Clear();
            foreach (var item in linhas)
                dgvEquipamentos.Rows.Add(item.Nome, item.Total, item.Abertos,
                    item.Criticos, item.Resolvidos);
            dgvEquipamentos.ClearSelection();
            lblTabela.Text = linhas.Count == 0 ? "Nenhum equipamento com incidentes" :
                "TOP " + linhas.Count + " • " + _filtrados.Count + " incidentes no filtro";
            btnExportar.Enabled = _filtrados.Count > 0;
        }

        private void LimparResultados()
        {
            lblTotal.Text = "0";
            lblAbertos.Text = "0";
            lblCriticos.Text = "0";
            lblResolvidos.Text = "0";
            lblTaxa.Text = "Taxa de resolução: 0%";
            lblTabela.Text = "Sem registros";
            dgvEquipamentos.Rows.Clear();
            graficoRiscos.AtualizarItens(new[]
            {
                new ItemGrafico("Crítico", 0, Vermelho),
                new ItemGrafico("Alto", 0, Laranja),
                new ItemGrafico("Moderado", 0, Azul),
                new ItemGrafico("Baixo", 0, Verde)
            });
            graficoStatus.AtualizarItens(new[]
            {
                new ItemGrafico("Aberto", 0, Laranja),
                new ItemGrafico("Em análise", 0, Roxo),
                new ItemGrafico("Resolvido", 0, Verde)
            });
            btnExportar.Enabled = false;
        }

        private string NomeAtivo(int id)
        {
            string nome;
            return _nomesAtivos.TryGetValue(id, out nome) ? nome : "Equipamento #" + id;
        }

        private void ExportarCsv()
        {
            if (!_carregado || _filtrados.Count == 0) return;
            using (var dialogo = new SaveFileDialog
            {
                Title = "Exportar relatório do CyberDesk",
                Filter = "Arquivo CSV (*.csv)|*.csv",
                FileName = "CyberDesk_Relatorios_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv",
                DefaultExt = "csv",
                AddExtension = true,
                OverwritePrompt = true
            })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (var writer = new StreamWriter(dialogo.FileName, false,
                        new UTF8Encoding(true)))
                    {
                        writer.WriteLine("ID;Data de abertura;Equipamento;Título;Descrição;Impacto;Probabilidade;Pontuação;Risco;Status");
                        foreach (Incidente i in _filtrados)
                        {
                            string[] campos =
                            {
                                i.Id.ToString(CultureInfo.InvariantCulture),
                                i.DataAbertura.ToString("dd/MM/yyyy HH:mm"),
                                NomeAtivo(i.AtivoId),
                                i.Titulo,
                                i.Descricao,
                                i.Impacto.ToString(CultureInfo.InvariantCulture),
                                i.Probabilidade.ToString(CultureInfo.InvariantCulture),
                                CalculadoraRisco.Calcular(i).ToString(CultureInfo.InvariantCulture),
                                CalculadoraRisco.Classificar(i),
                                TraduzirStatus(i.Status)
                            };
                            writer.WriteLine(string.Join(";", campos.Select(CsvSeguro)));
                        }
                    }
                    MessageBox.Show("Relatório exportado com sucesso!\n\n" + dialogo.FileName,
                        "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Falha ao salvar o arquivo:\n" + ex.Message,
                        "Erro na exportação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Sem permissão para salvar o arquivo:\n" + ex.Message,
                        "Erro na exportação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Evita que valores controlados pelo usuario sejam interpretados como formula pelo Excel.
        private static string CsvSeguro(string valor)
        {
            string s = valor ?? "";
            string trimmed = s.TrimStart();
            if (trimmed.Length > 0 && "=+-@".IndexOf(trimmed[0]) >= 0)
                s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private static string TraduzirStatus(StatusIncidente status)
        {
            switch (status)
            {
                case StatusIncidente.Aberto: return "Aberto";
                case StatusIncidente.EmAnalise: return "Em análise";
                case StatusIncidente.Resolvido: return "Resolvido";
                default: return "Desconhecido";
            }
        }
    }
}
