using CyberDesk.Data;
using CyberDesk.Enums;
using CyberDesk.Exceptions;
using CyberDesk.Models;
using CyberDesk.Repositories;
using CyberDesk.Services;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberDesk
{
    public partial class frmIncidentes : Form
    {
        // Mesma paleta das telas frmPrincipal e frmAtivos.
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

        private ComboBox cmbAtivo;
        private TextBox txtTitulo;
        private TextBox txtDescricao;
        private NumericUpDown nudImpacto;
        private NumericUpDown nudProbabilidade;
        private Label lblRisco;
        private Label lblSelecao;
        private Label lblStatusAtual;
        private ComboBox cmbNovoStatus;
        private TextBox txtMotivo;
        private TextBox txtBusca;
        private ComboBox cmbFiltro;
        private DataGridView dgvIncidentes;
        private Label lblConexao;
        private Label lblQuantidade;
        private Label lblSemDados;
        private Button btnCadastrar;
        private Button btnEditar;
        private Button btnExcluir;
        private Button btnNovo;
        private Button btnAlterarStatus;
        private Button btnHistorico;
        private Button btnRecarregar;

        private readonly List<Ativo> _ativos = new List<Ativo>();
        private readonly List<Incidente> _incidentes = new List<Incidente>();
        private IncidenteRepository _incidenteRepository;
        private HistoricoRepository _historicoRepository;
        private Incidente _selecionado;
        private bool _carregado;
        private bool _atualizandoGrade;

        public frmIncidentes()
        {
            InitializeComponent();
            ConfigurarJanela();
            CriarInterface();
            Shown += (s, e) => CarregarDados();
        }

        private void ConfigurarJanela()
        {
            Text = "CyberDesk | Gestão de Incidentes";
            BackColor = Fundo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 10F);
            Size = new Size(1380, 850);
            MinimumSize = new Size(1140, 720);
            StartPosition = FormStartPosition.CenterParent;
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
                else if (e.Control && e.KeyCode == Keys.N)
                {
                    LimparFormulario();
                    txtTitulo.Focus();
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
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(25, 15, 25, 12),
                Margin = Padding.Empty
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            raiz.Controls.Add(CriarCabecalho(), 0, 0);

            var conteudo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 435F));
            conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            conteudo.Controls.Add(CriarAreaFormulario(), 0, 0);
            conteudo.Controls.Add(CriarAreaTabela(), 1, 0);
            raiz.Controls.Add(conteudo, 0, 1);
            raiz.Controls.Add(CriarRodape(), 0, 2);
            Controls.Add(raiz);
        }

        private Control CriarCabecalho()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titulo = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            titulo.Controls.Add(new Label
            {
                Text = "Gestão de incidentes",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Texto,
                AutoSize = true,
                Location = new Point(0, 4)
            });
            titulo.Controls.Add(new Label
            {
                Text = "Registre ocorrências, classifique riscos e acompanhe as mudanças de status",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(2, 58),
                Font = new Font("Segoe UI", 10F)
            });
            painel.Controls.Add(titulo, 0, 0);

            var voltar = CriarBotao("←  Voltar ao painel", Menu, Texto);
            voltar.Size = new Size(185, 43);
            voltar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            voltar.Location = new Point(7, 20);
            voltar.Click += (s, e) => Close();
            var acoes = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            acoes.Controls.Add(voltar);
            painel.Controls.Add(acoes, 1, 0);
            return painel;
        }

        private Control CriarAreaFormulario()
        {
            var moldura = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                AutoScroll = true,
                Padding = new Padding(18),
                Margin = new Padding(0, 0, 15, 0)
            };
            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 1030,
                BackColor = Card,
                ColumnCount = 1,
                RowCount = 15,
                Margin = Padding.Empty
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            int[] alturas = { 58, 69, 71, 125, 75, 60, 105, 25, 50, 66, 105, 53, 52, 66, 0 };
            foreach (int h in alturas)
                campos.RowStyles.Add(h == 0 ? new RowStyle(SizeType.Percent, 100F)
                    : new RowStyle(SizeType.Absolute, h));

            var cabecalho = new Panel { BackColor = Card, Dock = DockStyle.Fill };
            cabecalho.Controls.Add(new Label
            {
                Text = "Dados do incidente",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Texto,
                Location = new Point(0, 0)
            });
            lblSelecao = new Label
            {
                Text = "Novo registro",
                AutoSize = true,
                ForeColor = Verde,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(1, 31)
            };
            cabecalho.Controls.Add(lblSelecao);
            campos.Controls.Add(cabecalho, 0, 0);

            cmbAtivo = CriarCombo();
            campos.Controls.Add(CriarCampo("EQUIPAMENTO AFETADO *", cmbAtivo), 0, 1);
            txtTitulo = CriarTexto(150);
            campos.Controls.Add(CriarCampo("TÍTULO DA OCORRÊNCIA *", txtTitulo), 0, 2);
            txtDescricao = CriarTexto(4000);
            txtDescricao.Multiline = true;
            txtDescricao.ScrollBars = ScrollBars.Vertical;
            campos.Controls.Add(CriarCampo("DESCRIÇÃO *", txtDescricao), 0, 3);

            var notas = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,
                BackColor = Card,
                Margin = Padding.Empty
            };
            notas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            notas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            notas.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            nudImpacto = CriarNota();
            nudProbabilidade = CriarNota();
            notas.Controls.Add(CriarCampo("IMPACTO (1 A 5) *", nudImpacto), 0, 0);
            notas.Controls.Add(CriarCampo("PROBABILIDADE (1 A 5) *", nudProbabilidade), 1, 0);
            nudImpacto.ValueChanged += (s, e) => AtualizarPontuacao();
            nudProbabilidade.ValueChanged += (s, e) => AtualizarPontuacao();
            campos.Controls.Add(notas, 0, 4);

            lblRisco = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Risco: 1/25  •  Baixo",
                ForeColor = Verde,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            campos.Controls.Add(lblRisco, 0, 5);

            var botoes = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 2,
                RowCount = 2,
                Margin = Padding.Empty
            };
            botoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            botoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            botoes.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            botoes.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            btnCadastrar = CriarBotao("+  Cadastrar", Verde, Fundo);
            btnEditar = CriarBotao("✎  Atualizar", Azul, Fundo);
            btnExcluir = CriarBotao("×  Excluir", Vermelho, Fundo);
            btnNovo = CriarBotao("↺  Limpar", Menu, Texto);
            var arr = new[] { btnCadastrar, btnEditar, btnExcluir, btnNovo };
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].Dock = DockStyle.Fill;
                arr[i].Margin = new Padding(0, 3, 8, 5);
                botoes.Controls.Add(arr[i], i % 2, i / 2);
            }
            btnCadastrar.Click += (s, e) => Cadastrar();
            btnEditar.Click += (s, e) => Atualizar();
            btnExcluir.Click += (s, e) => Excluir();
            btnNovo.Click += (s, e) => LimparFormulario();
            campos.Controls.Add(botoes, 0, 6);

            campos.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Card }, 0, 7);
            var cabStatus = new Panel { Dock = DockStyle.Fill, BackColor = Card };
            cabStatus.Controls.Add(new Label
            {
                Text = "Alteração de status",
                ForeColor = Texto,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 4)
            });
            lblStatusAtual = new Label
            {
                Text = "Status atual: —",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(2, 31)
            };
            cabStatus.Controls.Add(lblStatusAtual);
            campos.Controls.Add(cabStatus, 0, 8);

            cmbNovoStatus = CriarCombo();
            cmbNovoStatus.Items.AddRange(new object[] { "Aberto", "Em análise", "Resolvido" });
            cmbNovoStatus.SelectedIndex = 0;
            campos.Controls.Add(CriarCampo("NOVO STATUS *", cmbNovoStatus), 0, 9);
            txtMotivo = CriarTexto(500);
            txtMotivo.Multiline = true;
            txtMotivo.ScrollBars = ScrollBars.Vertical;
            campos.Controls.Add(CriarCampo("MOTIVO DA ALTERAÇÃO *", txtMotivo), 0, 10);

            btnAlterarStatus = CriarBotao("✓  Salvar status e histórico", Verde, Fundo);
            btnAlterarStatus.Dock = DockStyle.Fill;
            btnAlterarStatus.Margin = new Padding(0, 4, 8, 5);
            btnAlterarStatus.Click += (s, e) => SalvarNovoStatus();
            campos.Controls.Add(btnAlterarStatus, 0, 11);

            btnHistorico = CriarBotao("◷  Consultar histórico do selecionado", Menu, Texto);
            btnHistorico.Dock = DockStyle.Fill;
            btnHistorico.Margin = new Padding(0, 3, 8, 5);
            btnHistorico.Click += (s, e) => MostrarHistorico();
            campos.Controls.Add(btnHistorico, 0, 12);

            var aviso = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Text = "Selecione um incidente na tabela para editar, excluir ou mudar o status.\nF5: recarregar • Ctrl+N: novo registro",
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            campos.Controls.Add(aviso, 0, 13);
            moldura.Controls.Add(campos);
            return moldura;
        }

        private Control CriarAreaTabela()
        {
            var moldura = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Margin = Padding.Empty,
                Padding = new Padding(15, 13, 15, 12)
            };
            var grade = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty
            };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grade.RowStyles.Add(new RowStyle(SizeType.Absolute, 102F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grade.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            var busca = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 2,
                RowCount = 2,
                Margin = Padding.Empty
            };
            busca.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            busca.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195F));
            busca.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            busca.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            busca.Controls.Add(new Label
            {
                Text = "Incidentes registrados",
                ForeColor = Texto,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            busca.SetColumnSpan(busca.GetControlFromPosition(0, 0), 2);
            txtBusca = CriarTexto(150);
            txtBusca.Margin = new Padding(0, 5, 14, 12);
            txtBusca.TextChanged += (s, e) => AtualizarGrade();
            busca.Controls.Add(CriarCampoCompacto("BUSCAR POR TÍTULO OU EQUIPAMENTO", txtBusca), 0, 1);
            cmbFiltro = CriarCombo();
            cmbFiltro.Items.AddRange(new object[] { "Todos", "Abertos", "Em análise", "Resolvidos" });
            cmbFiltro.SelectedIndex = 0;
            cmbFiltro.SelectedIndexChanged += (s, e) => AtualizarGrade();
            busca.Controls.Add(CriarCampoCompacto("FILTRAR POR STATUS", cmbFiltro), 1, 1);
            grade.Controls.Add(busca, 0, 0);

            var contorno = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                BorderStyle = BorderStyle.FixedSingle,
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
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                Font = new Font("Segoe UI", 9F),
                RowTemplate = { Height = 42 }
            };
            dgvIncidentes.ColumnHeadersDefaultCellStyle.BackColor = Campo;
            dgvIncidentes.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            dgvIncidentes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.7F, FontStyle.Bold);
            dgvIncidentes.DefaultCellStyle.BackColor = Card;
            dgvIncidentes.DefaultCellStyle.ForeColor = Texto;
            dgvIncidentes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 68, 92);
            dgvIncidentes.DefaultCellStyle.SelectionForeColor = Texto;
            dgvIncidentes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(27, 42, 62);
            AdicionarColuna("colId", "ID", 43);
            AdicionarColuna("colTitulo", "INCIDENTE", 210);
            AdicionarColuna("colAtivo", "ATIVO", 155);
            AdicionarColuna("colPontos", "PONTOS", 66);
            AdicionarColuna("colNivel", "RISCO", 104);
            AdicionarColuna("colStatus", "STATUS", 101);
            AdicionarColuna("colData", "ABERTURA", 140);
            dgvIncidentes.SelectionChanged += (s, e) => SelecionarLinha();
            contorno.Controls.Add(dgvIncidentes);
            lblSemDados = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Nenhum incidente encontrado.\nCadastre um incidente ou altere a pesquisa.",
                Visible = false
            };
            contorno.Controls.Add(lblSemDados);
            grade.Controls.Add(contorno, 0, 1);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lblQuantidade = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Nenhum incidente carregado"
            };
            footer.Controls.Add(lblQuantidade, 0, 0);
            btnRecarregar = CriarBotao("↻  Recarregar", Menu, Texto);
            btnRecarregar.Dock = DockStyle.Fill;
            btnRecarregar.Margin = new Padding(6, 5, 0, 5);
            btnRecarregar.Click += (s, e) => CarregarDados();
            footer.Controls.Add(btnRecarregar, 1, 0);
            grade.Controls.Add(footer, 0, 2);
            moldura.Controls.Add(grade);
            return moldura;
        }

        private Control CriarRodape()
        {
            var grade = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 335F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lblConexao = new Label
            {
                Text = "●  Aguardando conexão MySQL",
                Dock = DockStyle.Fill,
                ForeColor = Laranja,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            grade.Controls.Add(lblConexao, 0, 0);
            grade.Controls.Add(new Label
            {
                Text = "CyberDesk  •  C# Windows Forms + MySQL",
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleRight
            }, 1, 0);
            return grade;
        }

        private static TextBox CriarTexto(int limite)
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                MaxLength = limite,
                BackColor = Campo,
                ForeColor = Texto,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10.5F),
                Margin = new Padding(0, 0, 7, 5)
            };
        }

        private static ComboBox CriarCombo()
        {
            return new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Campo,
                ForeColor = Texto,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 0, 7, 5)
            };
        }

        private static NumericUpDown CriarNota()
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 1,
                Maximum = 5,
                Value = 1,
                BackColor = Campo,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 11F),
                Margin = new Padding(0, 0, 9, 5),
                TextAlign = HorizontalAlignment.Center
            };
        }

        private static Control CriarCampo(string titulo, Control entrada)
        {
            var grade = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Card,
                Margin = new Padding(0, 0, 0, 4)
            };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grade.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grade.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            grade.Controls.Add(entrada, 0, 1);
            return grade;
        }

        private static Control CriarCampoCompacto(string titulo, Control entrada)
        {
            var grade = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Card,
                Margin = Padding.Empty
            };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grade.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grade.Controls.Add(new Label
            {
                Text = titulo,
                ForeColor = Muted,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            }, 0, 0);
            grade.Controls.Add(entrada, 0, 1);
            return grade;
        }

        private static Button CriarBotao(string titulo, Color fundo, Color corTexto)
        {
            var botao = new Button
            {
                Text = titulo,
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

        private void AdicionarColuna(string nome, string titulo, float largura)
        {
            dgvIncidentes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nome,
                HeaderText = titulo,
                FillWeight = largura,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private void AtualizarPontuacao()
        {
            if (lblRisco == null) return;
            int pontos = (int)nudImpacto.Value * (int)nudProbabilidade.Value;
            string classificacao = pontos <= 4 ? "Baixo" : pontos <= 9 ? "Moderado" :
            pontos <= 16 ? "Alto" : "Crítico";
            lblRisco.Text = "Risco: " + pontos + "/25  •  " + classificacao;
            lblRisco.ForeColor = CorRisco(pontos);
        }

        private void CarregarDados()
        {
            Cursor = Cursors.WaitCursor;
            btnRecarregar.Enabled = false;
            try
            {
                var banco = ConexaoMySql.DoAmbiente();
                var repoAtivos = new AtivoRepository(banco);
                var repoIncidentes = new IncidenteRepository(banco);
                var repoHistorico = new HistoricoRepository(banco);

                // Carrega em listas temporárias: só publica os dados após todas as consultas.
                List<Ativo> novosAtivos = repoAtivos.ListarTodos();
                List<Incidente> novosIncidentes = repoIncidentes.ListarTodos();
                _incidenteRepository = repoIncidentes;
                _historicoRepository = repoHistorico;
                _ativos.Clear();
                _ativos.AddRange(novosAtivos);
                _incidentes.Clear();
                _incidentes.AddRange(novosIncidentes);

                cmbAtivo.BeginUpdate();
                try
                {
                    cmbAtivo.Items.Clear();
                    foreach (Ativo ativo in _ativos) cmbAtivo.Items.Add(ativo);
                }
                finally { cmbAtivo.EndUpdate(); }

                _carregado = true;
                lblConexao.Text = "●  MySQL conectado";
                lblConexao.ForeColor = Verde;
                LimparFormulario();
                AtualizarGrade();
            }
            catch (Exception ex)
            {
                _carregado = false;
                _incidenteRepository = null;
                _historicoRepository = null;
                _ativos.Clear();
                _incidentes.Clear();
                cmbAtivo.Items.Clear();
                _atualizandoGrade = true;
                try { dgvIncidentes.Rows.Clear(); }
                finally { _atualizandoGrade = false; }
                lblSemDados.Visible = true;
                lblSemDados.BringToFront();
                lblConexao.Text = "●  Falha na conexão/consulta";
                lblConexao.ForeColor = Vermelho;
                lblQuantidade.Text = "Não foi possível carregar os registros";
                LimparFormulario();
                MostrarErro(ex, "Falha ao consultar o MySQL");
            }
            finally
            {
                btnRecarregar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void AtualizarGrade()
        {
            if (!_carregado || dgvIncidentes == null) return;
            string texto = txtBusca.Text.Trim();
            int filtro = cmbFiltro.SelectedIndex;
            var nomes = _ativos.ToDictionary(a => a.Id, a => a.Nome);

            var exibidos = _incidentes.Where(i =>
            {
                if (filtro == 1 && i.Status != StatusIncidente.Aberto) return false;
                if (filtro == 2 && i.Status != StatusIncidente.EmAnalise) return false;
                if (filtro == 3 && i.Status != StatusIncidente.Resolvido) return false;
                string nomeAtivo = nomes.ContainsKey(i.AtivoId) ? nomes[i.AtivoId] : "Ativo #" + i.AtivoId;
                return texto.Length == 0 ||
                    i.Titulo.IndexOf(texto, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                    nomeAtivo.IndexOf(texto, StringComparison.CurrentCultureIgnoreCase) >= 0;
            }).OrderByDescending(i => i.DataAbertura).ThenByDescending(i => i.Id).ToList();

            _atualizandoGrade = true;
            try
            {
                dgvIncidentes.Rows.Clear();
                foreach (Incidente item in exibidos)
                {
                    string ativo = nomes.ContainsKey(item.AtivoId) ? nomes[item.AtivoId] : "Ativo #" + item.AtivoId;
                    int pontos = CalculadoraRisco.Calcular(item);
                    int n = dgvIncidentes.Rows.Add(item.Id, item.Titulo, ativo, pontos,
                        CalculadoraRisco.Classificar(item), TextoStatus(item.Status),
                        item.DataAbertura.ToString("dd/MM/yyyy HH:mm"));
                    dgvIncidentes.Rows[n].Tag = item;
                    dgvIncidentes.Rows[n].Cells["colNivel"].Style.ForeColor = CorRisco(pontos);
                    dgvIncidentes.Rows[n].Cells["colStatus"].Style.ForeColor = item.Status == StatusIncidente.Resolvido
                        ? Verde : item.Status == StatusIncidente.EmAnalise ? Azul : Laranja;
                }
                dgvIncidentes.ClearSelection();
            }
            finally { _atualizandoGrade = false; }

            lblQuantidade.Text = "Exibindo " + exibidos.Count + " de " + _incidentes.Count + " incidente(s)";
            lblSemDados.Visible = exibidos.Count == 0;
            if (lblSemDados.Visible) lblSemDados.BringToFront();
            LimparFormulario();
        }

        private void SelecionarLinha()
        {
            if (_atualizandoGrade || dgvIncidentes.SelectedRows.Count != 1) return;
            var incidente = dgvIncidentes.SelectedRows[0].Tag as Incidente;
            if (incidente == null) return;
            _selecionado = incidente;
            lblSelecao.Text = "Editando incidente #" + incidente.Id;
            lblSelecao.ForeColor = Azul;
            txtTitulo.Text = incidente.Titulo;
            txtDescricao.Text = incidente.Descricao;
            nudImpacto.Value = incidente.Impacto;
            nudProbabilidade.Value = incidente.Probabilidade;
            cmbAtivo.SelectedItem = cmbAtivo.Items.Cast<Ativo>().FirstOrDefault(a => a.Id == incidente.AtivoId);
            lblStatusAtual.Text = "Status atual: " + TextoStatus(incidente.Status);
            cmbNovoStatus.SelectedIndex = (int)incidente.Status;
            txtMotivo.Clear();
            AtualizarBotoes();
        }

        private void LimparFormulario()
        {
            _selecionado = null;
            txtTitulo.Clear();
            txtDescricao.Clear();
            txtMotivo.Clear();
            nudImpacto.Value = 1;
            nudProbabilidade.Value = 1;
            cmbAtivo.SelectedIndex = cmbAtivo.Items.Count > 0 ? 0 : -1;
            cmbNovoStatus.SelectedIndex = 0;
            lblSelecao.Text = "Novo registro";
            lblSelecao.ForeColor = Verde;
            lblStatusAtual.Text = "Status atual: —";
            _atualizandoGrade = true;
            try { dgvIncidentes.ClearSelection(); }
            finally { _atualizandoGrade = false; }
            AtualizarPontuacao();
            AtualizarBotoes();
        }

        private void AtualizarBotoes()
        {
            bool existente = _carregado && _selecionado != null;
            btnCadastrar.Enabled = _carregado && _ativos.Count > 0 && !existente;
            btnEditar.Enabled = existente;
            btnExcluir.Enabled = existente;
            btnAlterarStatus.Enabled = existente;
            btnHistorico.Enabled = existente;
        }

        private Incidente LerIncidenteDaTela(bool edicao)
        {
            var ativo = cmbAtivo.SelectedItem as Ativo;
            if (ativo == null)
                throw new ValidacaoCyberDeskException("Cadastre e selecione um equipamento na tela de Ativos.");
            int impacto = (int)nudImpacto.Value;
            int probabilidade = (int)nudProbabilidade.Value;
            if (!edicao)
                return new Incidente(ativo.Id, txtTitulo.Text, txtDescricao.Text, impacto, probabilidade);

            if (_selecionado == null)
                throw new ValidacaoCyberDeskException("Selecione um incidente na tabela.");
            // Reaproveita Status e DataAbertura para não alterar os campos de auditoria.
            return new Incidente(_selecionado.Id, ativo.Id, txtTitulo.Text, txtDescricao.Text,
                impacto, probabilidade, _selecionado.Status, _selecionado.DataAbertura);
        }

        private void Cadastrar()
        {
            if (!_carregado || _incidenteRepository == null) return;
            try
            {
                Incidente novo = LerIncidenteDaTela(false);
                _incidenteRepository.Adicionar(novo);
                MessageBox.Show(this, "Incidente #" + novo.Id + " cadastrado com sucesso!",
                    "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarDados();
            }
            catch (Exception ex) { MostrarErro(ex, "Não foi possível cadastrar"); }
        }

        private void Atualizar()
        {
            if (_selecionado == null || _incidenteRepository == null) return;
            try
            {
                Incidente alterado = LerIncidenteDaTela(true);
                if (!_incidenteRepository.Atualizar(alterado))
                {
                    MessageBox.Show(this, "Registro não encontrado. Atualize a listagem.",
                        "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show(this, "Dados do incidente atualizados. Para mudar o status, use a seção própria.",
                    "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarDados();
            }
            catch (Exception ex) { MostrarErro(ex, "Não foi possível atualizar"); }
        }

        private void Excluir()
        {
            if (_selecionado == null || _incidenteRepository == null) return;
            int id = _selecionado.Id;
            var confirmacao = MessageBox.Show(this,
                "Excluir permanentemente o incidente #" + id + "?\n\n" +
                "ATENÇÃO: o MySQL também apagará o histórico deste incidente (ON DELETE CASCADE).\n" +
                "Essa ação não pode ser desfeita.", "Confirmar exclusão",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirmacao != DialogResult.Yes) return;
            try
            {
                if (!_incidenteRepository.Excluir(id))
                {
                    MessageBox.Show(this, "O incidente não existe mais. Atualize a listagem.",
                        "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show(this, "Incidente excluído.", "CyberDesk",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarDados();
            }
            catch (Exception ex) { MostrarErro(ex, "Não foi possível excluir"); }
        }

        private void SalvarNovoStatus()
        {
            if (_selecionado == null || _incidenteRepository == null) return;
            try
            {
                var novoStatus = (StatusIncidente)cmbNovoStatus.SelectedIndex;
                if (novoStatus == _selecionado.Status)
                    throw new ValidacaoCyberDeskException("Escolha um status diferente do atual.");
                string motivo = txtMotivo.Text.Trim();
                if (motivo.Length == 0)
                    throw new ValidacaoCyberDeskException("Informe o motivo da alteração.");
                if (!_incidenteRepository.AlterarStatusComHistorico(_selecionado.Id, novoStatus, motivo))
                {
                    MessageBox.Show(this, "Status não alterado. O registro pode ter sido modificado por outro usuário. Recarregue a tela.",
                        "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CarregarDados();
                    return;
                }
                MessageBox.Show(this, "Status atualizado e histórico registrado no MySQL!",
                    "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarDados();
            }
            catch (Exception ex) { MostrarErro(ex, "Não foi possível alterar o status"); }
        }

        private void MostrarHistorico()
        {
            if (_selecionado == null || _historicoRepository == null) return;
            try
            {
                var registros = _historicoRepository.ListarPorIncidente(_selecionado.Id);
                using (var janela = new Form())
                {
                    janela.Text = "CyberDesk | Histórico do incidente #" + _selecionado.Id;
                    janela.Size = new Size(950, 480);
                    janela.MinimumSize = new Size(750, 380);
                    janela.StartPosition = FormStartPosition.CenterParent;
                    janela.BackColor = Fundo;
                    janela.ForeColor = Texto;
                    janela.Font = new Font("Segoe UI", 10F);
                    var grade = new DataGridView
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
                        GridColor = Borda,
                        BorderStyle = BorderStyle.None,
                        EnableHeadersVisualStyles = false
                    };
                    grade.ColumnHeadersDefaultCellStyle.BackColor = Campo;
                    grade.ColumnHeadersDefaultCellStyle.ForeColor = Texto;
                    grade.DefaultCellStyle.BackColor = Card;
                    grade.DefaultCellStyle.ForeColor = Texto;
                    grade.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 68, 92);
                    grade.DefaultCellStyle.SelectionForeColor = Texto;
                    grade.Columns.Add("data", "DATA/HORA");
                    grade.Columns.Add("anterior", "ANTERIOR");
                    grade.Columns.Add("novo", "NOVO");
                    grade.Columns.Add("motivo", "MOTIVO");
                    grade.Columns["data"].FillWeight = 130;
                    grade.Columns["anterior"].FillWeight = 95;
                    grade.Columns["novo"].FillWeight = 95;
                    grade.Columns["motivo"].FillWeight = 340;
                    foreach (HistoricoIncidente h in registros)
                        grade.Rows.Add(h.DataAlteracao.ToString("dd/MM/yyyy HH:mm:ss"),
                            TextoStatus(h.StatusAnterior), TextoStatus(h.StatusNovo), h.Observacao);
                    var topo = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 52,
                        BackColor = Fundo,
                        ForeColor = Texto,
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(15, 0, 0, 0),
                        Text = "Histórico de alterações: " + registros.Count + " registro(s)"
                    };
                    janela.Controls.Add(grade);
                    janela.Controls.Add(topo);
                    janela.ShowDialog(this);
                }
            }
            catch (Exception ex) { MostrarErro(ex, "Não foi possível consultar o histórico"); }
        }

        private static string TextoStatus(StatusIncidente status)
        {
            switch (status)
            {
                case StatusIncidente.Aberto: return "Aberto";
                case StatusIncidente.EmAnalise: return "Em análise";
                case StatusIncidente.Resolvido: return "Resolvido";
                default: return "Desconhecido";
            }
        }

        private static Color CorRisco(int pontos)
        {
            if (pontos >= 17) return Vermelho;
            if (pontos >= 10) return Laranja;
            if (pontos >= 5) return Azul;
            return Verde;
        }

        private void MostrarErro(Exception ex, string titulo)
        {
            string mensagem;
            if (ex is ValidacaoCyberDeskException)
                mensagem = ex.Message;
            else if (ex is MySqlException)
                mensagem = "Operação recusada pelo MySQL.\n\n" + ex.Message;
            else
                mensagem = ex.Message;

            MessageBox.Show(this, mensagem, titulo,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
