using System;
using System.Drawing;
using System.Windows.Forms;

namespace HistogramForImageApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = string.Empty;

            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    textBox1.Text = filePath;

                    if (openFileDialog.FileName.EndsWith("jpg") || openFileDialog.FileName.EndsWith("png"))
                    {
                        var bitMap = new Bitmap(filePath);
                        Color color = bitMap.GetPixel(3119, 0);
                        textBox1.Text = (color.R).ToString() + " " + (color.G).ToString() + " " + (color.B).ToString();
                    }
                }
            }
        }
    }
}
