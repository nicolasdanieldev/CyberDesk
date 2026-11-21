using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CyberDesk.Data;
using CyberDesk.Exceptions;
using CyberDesk.Models;
using CyberDesk.Repositories;
using MySqlConnector;

namespace CyberDesk
{
    public partial class frmAtivos : Form
    {
        // Mesma paleta de cores usada no frmPrincipal.
        private static readonly Color Fundo = Color.FromArgb(10, 17, 30);
        private static readonly Color Menu = Color.FromArgb(16, 27, 45);
        private static readonly Color Card = Color.FromArgb(23, 36, 57);
        private static readonly Color Borda = Color.FromArgb(44, 63, 86);
        private static readonly Color Texto = Color.FromArgb(235, 243, 252);
        private static readonly Color TextoSecundario = Color.FromArgb(153, 174, 194);
        private static readonly Color Verde = Color.FromArgb(64, 212, 165);
        private static readonly Color Azul = Color.FromArgb(75, 164, 255);
        private static readonly Color Vermelho = Color.FromArgb(248, 108, 113);
        private static readonly Color Laranja = Color.FromArgb(255, 181, 83);

        private TextBox txtNome;
        private TextBox txtIp;
        private TextBox txtSetor;
        private TextBox txtResponsavel;
        private TextBox txtPesquisa;
        private DataGridView dgvAtivos;
        private Button btnCadastrar;
        private Button btnAtualizar;
        private Button btnExcluir;
        private Button btnNovo;
        private Button btnRecarregar;
        private Label lblTotal;
        private Label lblResultado;
        private Label lblConexao;
        private Label lblSelecao;
        private Label lblSemDados;

        private readonly List<Ativo> _ativos = new List<Ativo>();
        private AtivoRepository _repositorio;
        private int _idSelecionado;
        private bool _montandoLinhas;

        public frmAtivos()
        {
            InitializeComponent();
            ConfigurarFormulario();
            MontarInterface();
            Shown += (s, e) => CarregarAtivos();
        }

        private void ConfigurarFormulario()
        {
            Text = "CyberDesk | Gerenciamento de Ativos";
            BackColor = Fundo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 10F);
            Size = new Size(1260, 790);
            MinimumSize = new Size(1060, 680);
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    CarregarAtivos();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.N)
                {
                    LimparFormulario();
                    txtNome.Focus();
                    e.Handled = true;
                }
            };
        }

        private void MontarInterface()
        {
            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(26, 16, 26, 12),
                Margin = Padding.Empty
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 106F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            raiz.Controls.Add(CriarCabecalho(), 0, 0);
            raiz.Controls.Add(CriarAreaPrincipal(), 0, 1);
            raiz.Controls.Add(CriarRodape(), 0, 2);
            Controls.Add(raiz);
        }

        private Control CriarCabecalho()
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
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 192F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titulo = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            titulo.Controls.Add(new Label
            {
                Text = "Gerenciamento de ativos",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Texto,
                AutoSize = true,
                Location = new Point(0, 5)
            });
            titulo.Controls.Add(new Label
            {
                Text = "Cadastre, consulte e organize os equipamentos monitorados pelo CyberDesk",
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextoSecundario,
                AutoSize = true,
                Location = new Point(2, 60)
            });
            grade.Controls.Add(titulo, 0, 0);

            var acoes = new Panel { Dock = DockStyle.Fill, BackColor = Fundo };
            var btnVoltar = CriarBotao("←  Voltar ao painel", Menu, Texto, 178, 42);
            btnVoltar.Location = new Point(8, 18);
            btnVoltar.Click += (s, e) => Close();
            acoes.Controls.Add(btnVoltar);
            grade.Controls.Add(acoes, 1, 0);
            return grade;
        }

        private Control CriarAreaPrincipal()
        {
            var grade = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fundo,
                Margin = Padding.Empty
            };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 362F));
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grade.Controls.Add(CriarFormulario(), 0, 0);
            grade.Controls.Add(CriarListagem(), 1, 0);
            return grade;
        }

        private Control CriarFormulario()
        {
            var moldura = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Padding = new Padding(20),
                Margin = new Padding(0, 0, 16, 0),
                AutoScroll = true
            };
            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 576,
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Card,
                Margin = Padding.Empty
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 73F));
            for (int i = 0; i < 4; i++)
                campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            campos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titulo = new Panel { Dock = DockStyle.Fill, BackColor = Card };
            titulo.Controls.Add(new Label
            {
                Text = "Dados do equipamento",
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Texto,
                Location = new Point(0, 3)
            });
            lblSelecao = new Label
            {
                Text = "Novo cadastro",
                AutoSize = true,
                ForeColor = Verde,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(2, 39)
            };
            titulo.Controls.Add(lblSelecao);
            campos.Controls.Add(titulo, 0, 0);

            campos.Controls.Add(CriarCampo("NOME DO EQUIPAMENTO *", 120, out txtNome), 0, 1);
            campos.Controls.Add(CriarCampo("ENDEREÇO IP *", 45, out txtIp), 0, 2);
            campos.Controls.Add(CriarCampo("SETOR *", 100, out txtSetor), 0, 3);
            campos.Controls.Add(CriarCampo("RESPONSÁVEL *", 120, out txtResponsavel), 0, 4);

            var botoes = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 2,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = new Padding(0, 6, 0, 0)
            };
            botoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            botoes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            botoes.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            botoes.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            btnCadastrar = CriarBotao("+  Cadastrar", Verde, Fundo, 140, 42);
            btnAtualizar = CriarBotao("✎  Atualizar", Azul, Fundo, 140, 42);
            btnExcluir = CriarBotao("×  Excluir", Vermelho, Fundo, 140, 42);
            btnNovo = CriarBotao("↺  Limpar", Menu, Texto, 140, 42);
            foreach (Button b in new[] { btnCadastrar, btnAtualizar, btnExcluir, btnNovo })
            {
                b.Dock = DockStyle.Fill;
                b.Margin = new Padding(0, 2, 7, 5);
            }
            btnAtualizar.Enabled = false;
            btnExcluir.Enabled = false;
            btnCadastrar.Click += (s, e) => CadastrarAtivo();
            btnAtualizar.Click += (s, e) => AtualizarAtivo();
            btnExcluir.Click += (s, e) => ExcluirAtivo();
            btnNovo.Click += (s, e) => { LimparFormulario(); txtNome.Focus(); };
            botoes.Controls.Add(btnCadastrar, 0, 0);
            botoes.Controls.Add(btnAtualizar, 1, 0);
            botoes.Controls.Add(btnExcluir, 0, 1);
            botoes.Controls.Add(btnNovo, 1, 1);
            campos.Controls.Add(botoes, 0, 5);

            campos.Controls.Add(new Label
            {
                Text = "* Campos obrigatórios  •  IP único no MySQL",
                ForeColor = TextoSecundario,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5F)
            }, 0, 6);

            var instrucao = new Label
            {
                Text = "Dica: selecione uma linha da tabela para editar ou excluir.\n\nO CyberDesk impede a exclusão de equipamentos que tenham incidentes vinculados.",
                ForeColor = TextoSecundario,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };
            campos.Controls.Add(instrucao, 0, 7);
            moldura.Controls.Add(campos);
            return moldura;
        }


        private Control CriarCampo(string titulo, int maximo, out TextBox caixa)
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 8)
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            painel.Controls.Add(new Label
            {
                Text = titulo,
                Dock = DockStyle.Fill,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            caixa = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(31, 48, 69),
                ForeColor = Texto,
                Font = new Font("Segoe UI", 11F),
                MaxLength = maximo,
                Margin = new Padding(0, 1, 0, 4)
            };
            painel.Controls.Add(caixa, 0, 1);
            return painel;
        }

        private Control CriarListagem()
        {
            var moldura = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Margin = Padding.Empty,
                Padding = new Padding(15, 14, 15, 10)
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
            grade.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            grade.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            var cabecalho = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 242F));
            cabecalho.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var titulo = new Panel { Dock = DockStyle.Fill, BackColor = Card };
            titulo.Controls.Add(new Label
            {
                Text = "Inventário de ativos",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(1, 5),
                ForeColor = Texto
            });
            lblTotal = new Label
            {
                Text = "0 equipamento(s) cadastrado(s)",
                AutoSize = true,
                Location = new Point(2, 44),
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 9F)
            };
            titulo.Controls.Add(lblTotal);
            cabecalho.Controls.Add(titulo, 0, 0);

            var areaBusca = new Panel { Dock = DockStyle.Fill, BackColor = Card };
            areaBusca.Controls.Add(new Label
            {
                Text = "BUSCAR ATIVO",
                ForeColor = TextoSecundario,
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Location = new Point(0, 5)
            });
            txtPesquisa = new TextBox
            {
                Location = new Point(0, 33),
                Width = 230,
                MaxLength = 120,
                BackColor = Color.FromArgb(31, 48, 69),
                ForeColor = Texto,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F)
            };
            txtPesquisa.TextChanged += (s, e) => AtualizarTabela();
            areaBusca.Controls.Add(txtPesquisa);
            cabecalho.Controls.Add(areaBusca, 1, 0);
            grade.Controls.Add(cabecalho, 0, 0);
            var areaTabela = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = Padding.Empty
            };
            dgvAtivos = new DataGridView
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
                Font = new Font("Segoe UI", 9.5F),
                RowTemplate = { Height = 42 }
            };
            dgvAtivos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 45, 66);
            dgvAtivos.ColumnHeadersDefaultCellStyle.ForeColor = TextoSecundario;
            dgvAtivos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvAtivos.ColumnHeadersDefaultCellStyle.Padding = new Padding(7, 0, 4, 0);
            dgvAtivos.DefaultCellStyle.BackColor = Card;
            dgvAtivos.DefaultCellStyle.ForeColor = Texto;
            dgvAtivos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 68, 92);
            dgvAtivos.DefaultCellStyle.SelectionForeColor = Texto;
            dgvAtivos.DefaultCellStyle.Padding = new Padding(7, 0, 4, 0);
            dgvAtivos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(27, 42, 62);
            AdicionarColuna("colId", "ID", 45);
            AdicionarColuna("colNome", "EQUIPAMENTO", 180);
            AdicionarColuna("colIp", "IP", 125);
            AdicionarColuna("colSetor", "SETOR", 125);
            AdicionarColuna("colResponsavel", "RESPONSÁVEL", 150);
            dgvAtivos.SelectionChanged += (s, e) => PreencherFormularioSelecionado();
            areaTabela.Controls.Add(dgvAtivos);
            lblSemDados = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Nenhum equipamento encontrado.\nCadastre um ativo ou ajuste sua pesquisa.",
                Visible = false
            };
            areaTabela.Controls.Add(lblSemDados);
            grade.Controls.Add(areaTabela, 0, 1);

            var rodape = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 2,
                RowCount = 1
            };
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            rodape.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lblResultado = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Exibindo 0 equipamentos"
            };
            rodape.Controls.Add(lblResultado, 0, 0);
            btnRecarregar = CriarBotao("↻  Recarregar", Menu, Texto, 146, 38);
            btnRecarregar.Dock = DockStyle.Fill;
            btnRecarregar.Margin = new Padding(5, 6, 0, 5);
            btnRecarregar.Click += (s, e) => CarregarAtivos();
            rodape.Controls.Add(btnRecarregar, 1, 0);
            grade.Controls.Add(rodape, 0, 2);
            moldura.Controls.Add(grade);
            return moldura;
        }

        private Control CriarRodape()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                ColumnCount = 2,
                RowCount = 1
            };
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lblConexao = new Label
            {
                Text = "●  Aguardando conexão com MySQL",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Laranja,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            painel.Controls.Add(lblConexao, 0, 0);
            painel.Controls.Add(new Label
            {
                Text = "CyberDesk  •  LP2  •  C# Windows Forms + MySQL",
                ForeColor = TextoSecundario,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleRight
            }, 1, 0);
            return painel;
        }

        private static Button CriarBotao(string texto, Color fundo, Color corTexto, int largura, int altura)
        {
            var botao = new Button
            {
                Text = texto,
                Width = largura,
                Height = altura,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = fundo,
                ForeColor = corTexto,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderSize = 0;
            return botao;
        }

        private void AdicionarColuna(string nome, string titulo, float peso)
        {
            dgvAtivos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nome,
                HeaderText = titulo,
                FillWeight = peso,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private void CarregarAtivos()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnRecarregar.Enabled = false;
                if (_repositorio == null)
                    _repositorio = new AtivoRepository(ConexaoMySql.DoAmbiente());

                // Consulta primeiro. Em caso de falha, a lista em memoria nao e apagada.
                List<Ativo> encontrados = _repositorio.ListarTodos();
                _ativos.Clear();
                _ativos.AddRange(encontrados);
                lblTotal.Text = _ativos.Count + " equipamento(s) cadastrado(s)";
                lblConexao.Text = "●  MySQL conectado  •  Atualizado: " + DateTime.Now.ToString("HH:mm:ss");
                lblConexao.ForeColor = Verde;
                AtualizarTabela();
            }
            catch (Exception ex)
            {
                lblConexao.Text = "●  Não foi possível carregar os ativos";
                lblConexao.ForeColor = Vermelho;
                MostrarErro("Falha ao consultar o MySQL", ex);
            }
            finally
            {
                btnRecarregar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void AtualizarTabela()
        {
            if (dgvAtivos == null || txtPesquisa == null) return;
            string termo = txtPesquisa.Text.Trim();
            List<Ativo> filtrados = _ativos.Where(a =>
                termo.Length == 0 ||
                a.Id.ToString().IndexOf(termo, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                a.Nome.IndexOf(termo, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                a.EnderecoIp.IndexOf(termo, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                a.Setor.IndexOf(termo, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                a.Responsavel.IndexOf(termo, StringComparison.CurrentCultureIgnoreCase) >= 0
            ).ToList();

            _montandoLinhas = true;
            try
            {
                dgvAtivos.Rows.Clear();
                foreach (Ativo ativo in filtrados)
                {
                    int linha = dgvAtivos.Rows.Add(ativo.Id, ativo.Nome, ativo.EnderecoIp,
                        ativo.Setor, ativo.Responsavel);
                    dgvAtivos.Rows[linha].Tag = ativo;
                }
                dgvAtivos.ClearSelection();
            }
            finally
            {
                _montandoLinhas = false;
            }
            LimparFormulario();
            lblResultado.Text = "Exibindo " + filtrados.Count + " de " + _ativos.Count + " equipamento(s)";
            lblSemDados.Visible = filtrados.Count == 0;
            if (lblSemDados.Visible) lblSemDados.BringToFront();
        }

        private void PreencherFormularioSelecionado()
        {
            if (_montandoLinhas || dgvAtivos.SelectedRows.Count == 0) return;
            Ativo ativo = dgvAtivos.SelectedRows[0].Tag as Ativo;
            if (ativo == null) return;
            _idSelecionado = ativo.Id;
            txtNome.Text = ativo.Nome;
            txtIp.Text = ativo.EnderecoIp;
            txtSetor.Text = ativo.Setor;
            txtResponsavel.Text = ativo.Responsavel;
            lblSelecao.Text = "Editando equipamento #" + ativo.Id;
            lblSelecao.ForeColor = Azul;
            btnCadastrar.Enabled = false;
            btnAtualizar.Enabled = true;
            btnExcluir.Enabled = true;
        }

        private void LimparFormulario()
        {
            _montandoLinhas = true;
            try
            {
                _idSelecionado = 0;
                txtNome.Clear();
                txtIp.Clear();
                txtSetor.Clear();
                txtResponsavel.Clear();
                dgvAtivos.ClearSelection();
                lblSelecao.Text = "Novo cadastro";
                lblSelecao.ForeColor = Verde;
                btnCadastrar.Enabled = true;
                btnAtualizar.Enabled = false;
                btnExcluir.Enabled = false;
            }
            finally
            {
                _montandoLinhas = false;
            }
        }

        private Ativo ObterAtivoDoFormulario(int id = 0)
        {
            // Construtores e propriedades de Ativo validam os quatro campos.
            return new Ativo(id, txtNome.Text, txtIp.Text, txtSetor.Text, txtResponsavel.Text);
        }

        private void CadastrarAtivo()
        {
            try
            {
                if (_idSelecionado != 0) return;
                Ativo ativo = ObterAtivoDoFormulario();
                GarantirRepositorio();
                _repositorio.Adicionar(ativo);
                MessageBox.Show(this, "Equipamento cadastrado com sucesso! ID: " + ativo.Id,
                    "CyberDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarAtivos();
            }
            catch (Exception ex)
            {
                MostrarErro("Não foi possível cadastrar o equipamento", ex);
            }
        }

        private void AtualizarAtivo()
        {
            if (_idSelecionado <= 0) return;
            try
            {
                Ativo atualizado = ObterAtivoDoFormulario(_idSelecionado);
                GarantirRepositorio();
                if (!_repositorio.Atualizar(atualizado))
                {
                    MessageBox.Show(this, "Este equipamento não existe mais no banco. Recarregue a lista.",
                        "Registro não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show(this, "Equipamento atualizado com sucesso!", "CyberDesk",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarAtivos();
            }
            catch (Exception ex)
            {
                MostrarErro("Não foi possível atualizar o equipamento", ex);
            }
        }

        private void ExcluirAtivo()
        {
            if (_idSelecionado <= 0) return;
            string nome = txtNome.Text;
            DialogResult resposta = MessageBox.Show(this,
                "Deseja excluir o equipamento \"" + nome + "\"?\n\nEssa ação não pode ser desfeita.",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (resposta != DialogResult.Yes) return;

            try
            {
                GarantirRepositorio();
                if (!_repositorio.Excluir(_idSelecionado))
                {
                    MessageBox.Show(this, "Equipamento não encontrado. Recarregue a lista.",
                        "Registro não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show(this, "Equipamento excluído com sucesso.", "CyberDesk",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CarregarAtivos();
            }
            catch (Exception ex)
            {
                MostrarErro("Não foi possível excluir o equipamento", ex);
            }
        }

        private void GarantirRepositorio()
        {
            if (_repositorio == null)_repositorio = new AtivoRepository(ConexaoMySql.DoAmbiente());
        }

        private void MostrarErro(string titulo, Exception ex)
        {
            string detalhe;
            if (ex is ValidacaoCyberDeskException)
            {
                detalhe = ex.Message;
            }
            else if (ex is MySqlException)
            {
                var erroMySql = (MySqlException)ex;
                if (erroMySql.Number == 1062)
                    detalhe = "Este endereço IP já está cadastrado. Informe outro endereço.";
                else if (erroMySql.Number == 1451 || erroMySql.Number == 1217)
                    detalhe = "Não é possível excluir este equipamento porque ele possui incidentes vinculados.";
                else if (erroMySql.Number == 1045)
                    detalhe = "Acesso negado ao MySQL. Verifique usuário, senha e permissões.";
                else if (erroMySql.Number == 1049)
                    detalhe = "O banco 'cyberdesk' não foi encontrado. Execute o script de criação.";
                else
                    detalhe = "Falha no MySQL (código " + erroMySql.Number + "): " + erroMySql.Message;
            }
            else
            {
                detalhe = ex.Message;
            }
            MessageBox.Show(this, titulo + ".\n\n" + detalhe, "CyberDesk",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


    }
}
