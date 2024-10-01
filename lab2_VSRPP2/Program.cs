using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class StarDrawingForm : Form
{
    private PictureBox pictureBox;
    private List<Star> stars = new List<Star>();
    private Stack<Star> deletedStars = new Stack<Star>();
    private Color color1 = Color.Blue;
    private Color color2 = Color.Red;
    private bool isDrawing = false;
    private Point startPoint;
    private Star currentStar;

    public StarDrawingForm()
    {
        this.Text = "Star Drawing Application";
        this.DoubleBuffered = true;
        this.WindowState = FormWindowState.Maximized;

        pictureBox = new PictureBox();
        pictureBox.Dock = DockStyle.Fill;
        pictureBox.BackColor = Color.White;
        pictureBox.MouseDown += new MouseEventHandler(this.OnMouseDown);
        pictureBox.MouseMove += new MouseEventHandler(this.OnMouseMove);
        pictureBox.MouseUp += new MouseEventHandler(this.OnMouseUp);
        pictureBox.Paint += new PaintEventHandler(this.OnPaint);
        this.Controls.Add(pictureBox);

        MenuStrip menuStrip = new MenuStrip();
        ToolStripMenuItem colorMenuItem = new ToolStripMenuItem("Colors");

        ToolStripMenuItem color1MenuItem = new ToolStripMenuItem("Color 1");
        ToolStripMenuItem color2MenuItem = new ToolStripMenuItem("Color 2");
        ToolStripMenuItem deleteAllMenuItem = new ToolStripMenuItem("Delete All");
        ToolStripMenuItem deleteLastMenuItem = new ToolStripMenuItem("Delete Last");
        ToolStripMenuItem redoMenuItem = new ToolStripMenuItem("Redo");

        color1MenuItem.Click += (s, e) => ChooseColor(ref color1);
        color2MenuItem.Click += (s, e) => ChooseColor(ref color2);
        deleteAllMenuItem.Click += DeleteAll;
        deleteLastMenuItem.Click += DeleteLast;
        redoMenuItem.Click += RedoLastDeleted;

        colorMenuItem.DropDownItems.Add(color1MenuItem);
        colorMenuItem.DropDownItems.Add(color2MenuItem);
        colorMenuItem.DropDownItems.Add(deleteAllMenuItem);
        colorMenuItem.DropDownItems.Add(deleteLastMenuItem);
        colorMenuItem.DropDownItems.Add(redoMenuItem);

        menuStrip.Items.Add(colorMenuItem);
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);
    }

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isDrawing = true;
            startPoint = e.Location;
            currentStar = new Star(e.Location, 0, color1, color2);
            stars.Add(currentStar);
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (isDrawing)
        {
            int radius = (int)Math.Sqrt(Math.Pow(e.X - startPoint.X, 2) + Math.Pow(e.Y - startPoint.Y, 2));
            currentStar.UpdateSize(radius);
            pictureBox.Invalidate();
        }
    }

    private void OnMouseUp(object sender, MouseEventArgs e)
    {
        if (isDrawing)
        {
            isDrawing = false;
        }
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        foreach (var star in stars)
        {
            star.Draw(g);
        }
    }

    private void DeleteAll(object sender, EventArgs e)
    {
        if (stars.Count > 0)
        {
            deletedStars.Clear();
            foreach (var star in stars)
            {
                deletedStars.Push(star);
            }

            stars.Clear();
            pictureBox.Invalidate();
        }
    }

    private void DeleteLast(object sender, EventArgs e)
    {
        if (stars.Count > 0)
        {
            var lastStar = stars[stars.Count - 1];
            deletedStars.Push(lastStar);
            stars.RemoveAt(stars.Count - 1);
            pictureBox.Invalidate();
        }
    }

    private void RedoLastDeleted(object sender, EventArgs e)
    {
        if (deletedStars.Count > 0)
        {
            var restoredStar = deletedStars.Pop();
            stars.Add(restoredStar);
            pictureBox.Invalidate();
        }
    }

    private void ChooseColor(ref Color color)
    {
        using (ColorDialog colorDialog = new ColorDialog())
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                color = colorDialog.Color;
            }
        }
    }

    public static void Main()
    {
        Application.Run(new StarDrawingForm());
    }
}

public class Star
{
    private Point center;
    private int size;
    private Color borderColor1;
    private Color borderColor2;
    private Color coreColor = Color.Yellow;

    public Star(Point center, int size, Color borderColor1, Color borderColor2)
    {
        this.center = center;
        this.size = size;
        this.borderColor1 = borderColor1;
        this.borderColor2 = borderColor2;
    }

    public void UpdateSize(int newSize)
    {
        this.size = newSize;
    }

    public void Draw(Graphics g)
    {
        Point[] outerPoints = new Point[8];
        for (int i = 0; i < 4; i++)
        {
            double outerAngle = Math.PI / 2 + i * (Math.PI / 2);
            double innerAngle = outerAngle + Math.PI / 4;

            outerPoints[i * 2] = new Point(
                center.X + (int)(size * Math.Cos(outerAngle)),
                center.Y - (int)(size * Math.Sin(outerAngle)));

            outerPoints[i * 2 + 1] = new Point(
                center.X + (int)((size / 2) * Math.Cos(innerAngle)),
                center.Y - (int)((size / 2) * Math.Sin(innerAngle)));
        }

        using (Brush coreBrush = new SolidBrush(coreColor))
        {
            g.FillPolygon(coreBrush, outerPoints);
        }

        for (int i = 0; i < 4; i++)
        {
            using (Pen borderPen = new Pen(i % 2 == 0 ? borderColor1 : borderColor2, 2))
            {
                g.DrawLine(borderPen, outerPoints[i * 2], outerPoints[(i * 2 + 1) % 8]);
                g.DrawLine(borderPen, outerPoints[(i * 2 + 1) % 8], outerPoints[(i * 2 + 2) % 8]);
            }
        }
    }
}