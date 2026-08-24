using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyCapture
{
    public partial class OverlayForm : Form
    {
        private bool isSelecting = false;
        private Point startPoint;
        private Point currentPoint;
        private Rectangle selectionRectangle;

        public event Action<Rectangle> AreaSelected;

        public OverlayForm()
        {
            InitializeComponent();
        }

        private void OverlayForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isSelecting = true;
                startPoint = e.Location;
                currentPoint = e.Location;
            }
        }

        private void OverlayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                currentPoint = e.Location;
                this.Invalidate();
            }
        }

        private void OverlayForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isSelecting)
            {
                isSelecting = false;

                int x = Math.Min(startPoint.X, currentPoint.X);
                int y = Math.Min(startPoint.Y, currentPoint.Y);
                int width = Math.Abs(currentPoint.X - startPoint.X);
                int height = Math.Abs(currentPoint.Y - startPoint.Y);

                if (width > 10 && height > 10)
                {
                    selectionRectangle = new Rectangle(x, y, width, height);
                    AreaSelected?.Invoke(selectionRectangle);
                    this.Close();
                }
            }
        }

        private void OverlayForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            if (isSelecting)
            {
                int x = Math.Min(startPoint.X, currentPoint.X);
                int y = Math.Min(startPoint.Y, currentPoint.Y);
                int width = Math.Abs(currentPoint.X - startPoint.X);
                int height = Math.Abs(currentPoint.Y - startPoint.Y);

                Rectangle rect = new Rectangle(x, y, width, height);

                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.White)))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
        }
    }
}
