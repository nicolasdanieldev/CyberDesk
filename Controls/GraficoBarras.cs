using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberDesk.Controls
{
    public sealed class ItemGrafico
    {
        public string Rotulo { get; private set; }
        public int Valor { get; private set; }
        public Color Cor { get; private set; }

        public ItemGrafico(string rotulo, int valor, Color cor)
        {
            if (string.IsNullOrWhiteSpace(rotulo))
            {
                throw new ArgumentException("O rótulo é obrigatório.", "rotulo");
            }
             
            if (valor < 0)
            {
                throw new ArgumentOutOfRangeException("valor");
            }

            Rotulo = rotulo.Trim();
            Valor = valor;
            Cor = cor;
        }
    }

    public sealed class GraficoBarras : Control
    {
        private readonly List<ItemGrafico> _itens = new List<ItemGrafico>();

        public GraficoBarras()
        {
            BackColor = Color.FromArgb(23, 36, 57);
            ForeColor = Color.FromArgb(235, 243, 252);
            Font = new Font("Segoe UI", 9.5F);
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void AtualizarItens(IEnumerable<ItemGrafico> itens)
        {
            if (itens == null)
            {
                throw new ArgumentNullException("itens");
            }
            
            var novos = itens.ToList();
            if (novos.Any(i => i == null))
            {
                throw new ArgumentException("A lista não pode conter valores nulos.", "itens");
            }

            _itens.Clear();
            _itens.AddRange(novos);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (_itens.Count == 0 || Width < 130 || Height < 50)
            {
                return;
            }
                

            int maximo = Math.Max(1, _itens.Max(i => i.Valor));
            int esquerda = 18;
            int direita = 24;
            int largura = Math.Max(10, ClientSize.Width - esquerda - direita);
            int alturaLinha = Math.Min(62, Math.Max(46, (ClientSize.Height - 12) / _itens.Count));
            int topo = 11;
            int alturaBarra = 17;

            using (var textoBrush = new SolidBrush(ForeColor))
            using (var textoValor = new SolidBrush(Color.FromArgb(185, 201, 217)))
            using (var fundoBarra = new SolidBrush(Color.FromArgb(36, 55, 75)))
            using (var fonteValor = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                foreach (ItemGrafico item in _itens)
                {
                    var rectRotulo = new RectangleF(esquerda, topo, largura - 48, 21);
                    e.Graphics.DrawString(item.Rotulo, Font, textoBrush, rectRotulo);

                    string valor = item.Valor.ToString("N0");
                    var formato = new StringFormat { Alignment = StringAlignment.Far };
                    try
                    {
                        e.Graphics.DrawString(valor, fonteValor, textoValor,
                            new RectangleF(esquerda + largura - 67, topo, 67, 21), formato);
                    }
                    finally { formato.Dispose(); }

                    var fundo = new Rectangle(esquerda, topo + 25, largura, alturaBarra);
                    e.Graphics.FillRectangle(fundoBarra, fundo);

                    int barraLargura = (int)Math.Round(largura * (double)item.Valor / maximo);
                    if (barraLargura > 0)
                    {
                        using (var cor = new SolidBrush(item.Cor))
                            e.Graphics.FillRectangle(cor,
                                new Rectangle(esquerda, topo + 25, barraLargura, alturaBarra));
                    }
                    topo += alturaLinha;
                }
            }
        }
    }
}
