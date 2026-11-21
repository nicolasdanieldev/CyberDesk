using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Models;
using CyberDesk.Repositories;
using CyberDesk.Services;
using MySqlConnector;

namespace CyberDesk
{

    public partial class frmPrincipal : Form
    {
        // Identidade visual - cores fixas, reutilizadas em toda a tela.
        private static readonly Color Fundo = Color.FromArgb(10, 17, 30);
        private static readonly Color Menu = Color.FromArgb(16, 27, 45);
        private static readonly Color Card = Color.FromArgb(23, 36, 57);
        private static readonly Color Borda = Color.FromArgb(44, 63, 86);
        private static readonly Color Texto = Color.FromArgb(235, 243, 252);
        private static readonly Color TextoSecundario = Color.FromArgb(153, 174, 194);
        private static readonly Color Azul = Color.FromArgb(75, 164, 255);
        private static readonly Color Verde = Color.FromArgb(64, 212, 165);
        private static readonly Color Laranja = Color.FromArgb(255, 181, 83);
        private static readonly Color Vermelho = Color.FromArgb(248, 108, 113);

        private Label lblTotal;
        private Label lblAbertos;
        private Label lblCriticos;
        private Label lblAtivos;
        private Label lblConexao;
        private Label lblAtualizacao;
        private Label lblResultadoBusca;
        private Label lblSemDados;
        private DataGridView dgvIncidentes;
        private ComboBox cmbStatus;
        private TextBox txtBusca;
        private Button btnAtualizar;

        // Dados consultados no MySQL e mantidos temporariamente para os filtros.
        private List<Incidente> _incidentes = new List<Incidente>();
        private List<Ativo> _ativos = new List<Ativo>();
        private bool _carregado;

        public frmPrincipal()
        {
            InitializeComponent();
            ConfigurarFormulario();
            MontarInterface();

            Shown += frmPrincipal_Shown;
        }

        private void ConfigurarFormulario()
        {
            Text = "CyberDesk | Central de Segurança";
            BackColor = Fundo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 680);
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
        }

        private void MontarInterface()
        {
            // A interface e criada via codigo para ficar identica em qualquer Designer.
            var estrutura = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 226F));
            estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            estrutura.Controls.Add(CriarMenuLateral(), 0, 0);
            estrutura.Controls.Add(CriarConteudo(), 1, 0);
            Controls.Add(estrutura);
        }

        private Control CriarMenuLateral()
        {
            var menuTabela = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Menu,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty
            };
            menuTabela.RowStyles.Add(new RowStyle(SizeType.Absolute, 125F));
            menuTabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            menuTabela.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));

            var marca = new Panel { Dock = DockStyle.Fill, BackColor = Menu };
            marca.Controls.Add(new Label
            {
                Text = "◆  CYBERDESK",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Verde,
                AutoSize = true,
                Location = new Point(20, 30)
            });
            marca.Controls.Add(new Label
            {
                Text = "SECURITY OPERATIONS",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextoSecundario,
                AutoSize = true,
                Location = new Point(24, 66)
            });
            menuTabela.Controls.Add(marca, 0, 0);

            var links = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Menu,
                FlowDirection = FlowDirection.TopDown, 
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 9, 10, 0),
                Margin = Padding.Empty
            };
            links.Controls.Add(CriarBotaoMenu("VISÃO GERAL", true, null));
            links.Controls.Add(CriarBotaoMenu("Equipamentos", false,
                () => AbrirTelaAtivos()));
            links.Controls.Add(CriarBotaoMenu("Incidentes",false,
                () => AbrirTelaIncidentes()));
            links.Controls.Add(CriarBotaoMenu("Histórico",false,
                () => AbrirTelaHistorico()));
            links.Controls.Add(CriarBotaoMenu("Relatórios", false,
                () => AbrirTelaRelatorios()));
            links.Controls.Add(CriarBotaoMenu("Sair", false, () => Close()));
            menuTabela.Controls.Add(links, 0, 1);

            var versao = new Label
            {
                Dock = DockStyle.Fill,
                Text = "CyberDesk  •  Projeto LP2\nC# Windows Forms + MySQL",
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(9)
            };
            menuTabela.Controls.Add(versao, 0, 2);
            return menuTabela;
        }

        private Button CriarBotaoMenu(string titulo, bool selecionado, Action aoClicar)
        {
            var botao = new Button
            {
                Text = (selecionado ? "▸   " : "      ") + titulo,
                Size = new Size(195, 49),
                Margin = new Padding(0, 0, 0, 7),
                FlatStyle = FlatStyle.Flat,
                BackColor = selecionado ? Card : Menu,
                ForeColor = selecionado ? Verde : TextoSecundario,
                Font = new Font("Segoe UI", 10F, selecionado ? FontStyle.Bold : FontStyle.Regular),
                Cursor = selecionado ? Cursors.Default : Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                UseVisualStyleBackColor = false,
                TabStop = !selecionado
            };
            botao.FlatAppearance.BorderSize = 0;
            if (aoClicar != null)
                botao.Click += (s, e) => aoClicar();
            return botao;
        }

        private Control CriarConteudo()
        {
            var conteudo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                Padding = new Padding(28, 14, 28, 12),
                ColumnCount = 1,
                RowCount = 5,
                Margin = Padding.Empty
            };
            conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 98F));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 151F));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

            conteudo.Controls.Add(CriarCabecalho(), 0, 0);
            conteudo.Controls.Add(CriarIndicadores(), 0, 1);
            conteudo.Controls.Add(CriarBarraBusca(), 0, 2);
            conteudo.Controls.Add(CriarTabela(), 0, 3);
            conteudo.Controls.Add(CriarRodape(), 0, 4);
            return conteudo;
        }

        private Control CriarCabecalho()
        {
            var cabecalho = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            cabecalho.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titulo = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            titulo.Controls.Add(new Label
            {
                Text = "Painel de segurança",
                Font = new Font("Segoe UI", 23F, FontStyle.Bold),
                ForeColor = Texto,
                AutoSize = true,
                Location = new Point(0, 10)
            });
            titulo.Controls.Add(new Label
            {
                Text = "Visão geral dos ativos e incidentes cadastrados no MySQL",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextoSecundario,
                AutoSize = true,
                Location = new Point(2, 59)
            });
            cabecalho.Controls.Add(titulo, 0, 0);

            var areaAcao = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            btnAtualizar = new Button
            {
                Text = "↻  Atualizar dados",
                Size = new Size(152, 40),
                Location = new Point(2, 23),
                BackColor = Verde,
                ForeColor = Fundo,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAtualizar.FlatAppearance.BorderSize = 0;
            btnAtualizar.Click += (s, e) => CarregarDashboard();
            areaAcao.Controls.Add(btnAtualizar);
            cabecalho.Controls.Add(areaAcao, 1, 0);
            return cabecalho;
        }

        private Control CriarIndicadores()
        {
            var indicadores = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Fundo,
                Margin = Padding.Empty,
                AutoScroll = true,
                Padding = new Padding(0, 5, 0, 6)
            };

            indicadores.Controls.Add(CriarCard("TOTAL DE INCIDENTES", "Ocorrências cadastradas", Azul, out lblTotal));
            indicadores.Controls.Add(CriarCard("EM ABERTO", "Aguardando atendimento", Laranja, out lblAbertos));
            indicadores.Controls.Add(CriarCard("RISCO CRÍTICO", "Prioridade máxima", Vermelho, out lblCriticos));
            indicadores.Controls.Add(CriarCard("ATIVOS", "Equipamentos cadastrados", Verde, out lblAtivos));

            indicadores.SizeChanged += (s, e) => AjustarCards(indicadores);
            return indicadores;
        }

        private Panel CriarCard(string titulo, string detalhe, Color destaque, out Label numero)
        {
            var painel = new Panel
            {
                BackColor = Card,
                Size = new Size(205, 128),
                Margin = new Padding(0, 0, 15, 0)
            };
            painel.Controls.Add(new Panel
            {
                BackColor = destaque,
                Dock = DockStyle.Left,
                Width = 4
            });
            painel.Controls.Add(new Label
            {
                Text = titulo,
                AutoSize = true,
                Location = new Point(17, 17),
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            });
            numero = new Label
            {
                Text = "--",
                AutoSize = true,
                Location = new Point(17, 39),
                ForeColor = destaque,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold)
            };
            painel.Controls.Add(numero);
            painel.Controls.Add(new Label
            {
                Text = detalhe,
                AutoSize = true,
                Location = new Point(18, 97),
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8.7F)
            });
            return painel;
        }

        private static void AjustarCards(FlowLayoutPanel painel)
        {
            if (painel.ClientSize.Width < 100) return;
            var cards = painel.Controls.OfType<Panel>().ToArray();
            const int espacoEntreCards = 15;
            int largura = Math.Max(150, (painel.ClientSize.Width - 3 * espacoEntreCards - 12) / 4);
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Width = largura;
                cards[i].Margin = new Padding(0, 0, i == cards.Length - 1 ? 0 : espacoEntreCards, 0);
            }
        }

        private Control CriarBarraBusca()
        {
            var area = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 177F));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235F));
            area.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var textos = new Panel { Dock = DockStyle.Fill };
            textos.Controls.Add(new Label
            {
                Text = "Incidentes recentes",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Texto,
                AutoSize = true,
                Location = new Point(0, 18)
            });
            textos.Controls.Add(new Label
            {
                Text = "Últimos 12 registros • filtre por status ou pesquise",
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(1, 54)
            });
            area.Controls.Add(textos, 0, 0);

            var boxStatus = new Panel { Dock = DockStyle.Fill };
            boxStatus.Controls.Add(CriarLabelPequena("FILTRAR STATUS"));
            cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Card,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(0, 41),
                Width = 164
            };
            cmbStatus.Items.AddRange(new object[] { "Todos", "Aberto", "Em análise", "Resolvido" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => AtualizarTabela();
            boxStatus.Controls.Add(cmbStatus);
            area.Controls.Add(boxStatus, 1, 0);

            var boxBusca = new Panel { Dock = DockStyle.Fill };
            boxBusca.Controls.Add(CriarLabelPequena("BUSCAR POR TÍTULO OU ATIVO"));
            txtBusca = new TextBox
            {
                BackColor = Card,
                ForeColor = Texto,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(0, 40),
                Width = 224
            };
            txtBusca.TextChanged += (s, e) => AtualizarTabela();
            boxBusca.Controls.Add(txtBusca);
            area.Controls.Add(boxBusca, 2, 0);
            return area;
        }

        private Label CriarLabelPequena(string texto)
        {
            return new Label
            {
                Text = texto,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 19)
            };
        }

        private Control CriarTabela()
        {
            var contorno = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(1),
                Margin = Padding.Empty
            };
            dgvIncidentes = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Card,
                GridColor = Borda,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeight = 44,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                Font = new Font("Segoe UI", 9.5F),
                RowTemplate = { Height = 43 }
            };
            dgvIncidentes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 45, 66);
            dgvIncidentes.ColumnHeadersDefaultCellStyle.ForeColor = TextoSecundario;
            dgvIncidentes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvIncidentes.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvIncidentes.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 5, 0);
            dgvIncidentes.DefaultCellStyle.BackColor = Card;
            dgvIncidentes.DefaultCellStyle.ForeColor = Texto;
            dgvIncidentes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 68, 92);
            dgvIncidentes.DefaultCellStyle.SelectionForeColor = Texto;
            dgvIncidentes.DefaultCellStyle.Padding = new Padding(8, 0, 5, 0);
            dgvIncidentes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(27, 42, 62);
            dgvIncidentes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            AdicionarColuna("colId", "ID", 50);
            AdicionarColuna("colTitulo", "INCIDENTE", 240);
            AdicionarColuna("colAtivo", "EQUIPAMENTO", 185);
            AdicionarColuna("colRisco", "PONTOS", 80);
            AdicionarColuna("colClassificacao", "RISCO", 112);
            AdicionarColuna("colStatus", "STATUS", 110);
            AdicionarColuna("colData", "ABERTURA", 148);

            contorno.Controls.Add(dgvIncidentes);
            lblSemDados = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ForeColor = TextoSecundario,
                Text = "Nenhum incidente encontrado.\nCadastre o primeiro incidente para preencher este painel.",
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            contorno.Controls.Add(lblSemDados);
            return contorno;
        }

        private void AdicionarColuna(string nome, string cabecalho, float peso)
        {
            var coluna = new DataGridViewTextBoxColumn
            {
                Name = nome,
                HeaderText = cabecalho,
                ReadOnly = true,
                FillWeight = peso,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgvIncidentes.Columns.Add(coluna);
        }

        private Control CriarRodape()
        {
            var barra = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215F));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            barra.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            lblConexao = new Label
            {
                Dock = DockStyle.Fill,
                Text = "●  Verificando MySQL...",
                ForeColor = Laranja,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            lblResultadoBusca = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Nenhum dado carregado",
                ForeColor = TextoSecundario,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F)
            };
            lblAtualizacao = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Sem atualização",
                ForeColor = TextoSecundario,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            };
            barra.Controls.Add(lblConexao, 0, 0);
            barra.Controls.Add(lblResultadoBusca, 1, 0);
            barra.Controls.Add(lblAtualizacao, 2, 0);
            return barra;
        }

        private void frmPrincipal_Shown(object sender, EventArgs e)
        {
            CarregarDashboard();
        }

        // Busca os dados uma vez; a busca e o filtro usam a lista em memoria.
        private void CarregarDashboard()
        {
            Cursor = Cursors.WaitCursor;
            btnAtualizar.Enabled = false;
            try
            {
                var banco = ConexaoMySql.DoAmbiente();
                var repositorioAtivos = new AtivoRepository(banco);
                var repositorioIncidentes = new IncidenteRepository(banco);

                // Os dois ListarTodos ja foram testados na etapa anterior.
                _ativos = repositorioAtivos.ListarTodos();
                _incidentes = repositorioIncidentes.ListarTodos();
                ResumoIncidentes resumo = new RelatorioService().GerarResumo(_incidentes);

                lblTotal.Text = resumo.Total.ToString();
                lblAbertos.Text = resumo.Abertos.ToString();
                lblCriticos.Text = resumo.Criticos.ToString();
                lblAtivos.Text = _ativos.Count.ToString();
                lblConexao.Text = "●  MySQL conectado";
                lblConexao.ForeColor = Verde;
                lblAtualizacao.Text = "Atualizado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                _carregado = true;
                AtualizarTabela();
            }
            catch (MySqlException ex)
            {
                ExibirFalha("Não foi possível consultar o MySQL.\n\n" + ex.Message);
            }
            catch (Exception ex)
            {
                ExibirFalha("Não foi possível carregar o dashboard.\n\n" + ex.Message);
            }
            finally
            {
                btnAtualizar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void ExibirFalha(string mensagem)
        {
            _carregado = false;
            _ativos.Clear();
            _incidentes.Clear();
            lblTotal.Text = "--";
            lblAbertos.Text = "--";
            lblCriticos.Text = "--";
            lblAtivos.Text = "--";
            dgvIncidentes.Rows.Clear();
            lblSemDados.Visible = true;
            lblSemDados.BringToFront();
            lblConexao.Text = "●  MySQL indisponível";
            lblConexao.ForeColor = Vermelho;
            lblResultadoBusca.Text = "Falha ao carregar os dados";
            lblAtualizacao.Text = "Sem atualização";

            MessageBox.Show(this, mensagem, "Erro no CyberDesk",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void AtualizarTabela()
        {
            if (!_carregado || dgvIncidentes == null) return;

            int filtro = cmbStatus.SelectedIndex;
            string busca = txtBusca.Text.Trim();
            var nomesAtivos = _ativos.ToDictionary(a => a.Id, a => a.Nome);

            var filtrados = new List<Incidente>();
            foreach (Incidente incidente in _incidentes)
            {
                if (filtro == 1 && incidente.Status != StatusIncidente.Aberto) continue;
                if (filtro == 2 && incidente.Status != StatusIncidente.EmAnalise) continue;
                if (filtro == 3 && incidente.Status != StatusIncidente.Resolvido) continue;

                string nomeAtivo;
                if (!nomesAtivos.TryGetValue(incidente.AtivoId, out nomeAtivo))
                    nomeAtivo = "Ativo #" + incidente.AtivoId;

                if (busca.Length > 0 &&
                    incidente.Titulo.IndexOf(busca, StringComparison.CurrentCultureIgnoreCase) < 0 &&
                    nomeAtivo.IndexOf(busca, StringComparison.CurrentCultureIgnoreCase) < 0)
                    continue;

                filtrados.Add(incidente);
            }

            dgvIncidentes.Rows.Clear();
            foreach (Incidente incidente in filtrados
                .OrderByDescending(i => i.DataAbertura).ThenByDescending(i => i.Id).Take(12))
            {
                string nomeAtivo;
                if (!nomesAtivos.TryGetValue(incidente.AtivoId, out nomeAtivo))
                    nomeAtivo = "Ativo #" + incidente.AtivoId;

                int risco = CalculadoraRisco.Calcular(incidente);
                string nivel = CalculadoraRisco.Classificar(incidente);
                string status = TraduzirStatus(incidente.Status);
                int linha = dgvIncidentes.Rows.Add(incidente.Id, incidente.Titulo,
                    nomeAtivo, risco, nivel, status,
                    incidente.DataAbertura.ToString("dd/MM/yyyy HH:mm"));
                dgvIncidentes.Rows[linha].Cells["colClassificacao"].Style.ForeColor = CorDeRisco(risco);
                dgvIncidentes.Rows[linha].Cells["colStatus"].Style.ForeColor =
                    incidente.Status == StatusIncidente.Resolvido ? Verde :
                    incidente.Status == StatusIncidente.EmAnalise ? Azul : Laranja;
            }

            lblSemDados.Visible = dgvIncidentes.Rows.Count == 0;
            if (lblSemDados.Visible) lblSemDados.BringToFront();
            lblResultadoBusca.Text = "Exibindo " + dgvIncidentes.Rows.Count +
                " de " + filtrados.Count + " incidente(s) filtrado(s)";
            dgvIncidentes.ClearSelection();
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

        private static Color CorDeRisco(int pontos)
        {
            if (pontos >= 17) return Vermelho;
            if (pontos >= 10) return Laranja;
            if (pontos >= 5) return Azul;
            return Verde;
        }

        private void ExibirModuloFuturo(string nome)
        {
            MessageBox.Show(this,
                nome + " será implementado nas próximas etapas do CyberDesk.",
                "Módulo em desenvolvimento", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void AbrirTelaAtivos()
        {
            using (var tela = new frmAtivos())
            {
                tela.ShowDialog(this);
            }

            // Atualiza o Dashboard quando a tela de ativos fechar.
            CarregarDashboard();
        }
        private void AbrirTelaIncidentes()
        {
            using (var tela = new frmIncidentes())
            {
                tela.ShowDialog(this);
            }

            CarregarDashboard();
        }

        private void AbrirTelaHistorico()
        {
            using (var tela = new frmHistorico())
            {
                tela.ShowDialog(this);
            }

            CarregarDashboard();
        }


        private void AbrirTelaRelatorios()
        {
            using (var tela = new frmRelatorios())
            {
                tela.ShowDialog(this);
            }

            CarregarDashboard();
        }


        private void frmPrincipal_Load(object sender, EventArgs e)
        {

        }
    }
}

