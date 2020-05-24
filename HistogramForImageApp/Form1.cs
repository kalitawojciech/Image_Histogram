using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HistogramForImageApp
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        string lastSavedImageTime;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Users\wojte\Desktop";
                openFileDialog.FilterIndex = 2;
                openFileDialog.Filter = "Image files (*.jpg)|*.jpg|Image files (*.png)|*.png|All Files (*.*)|*.*";
                openFileDialog.RestoreDirectory = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;


                    if (openFileDialog.FileName.EndsWith("jpg") || openFileDialog.FileName.EndsWith("png"))
                    {
                        bitmap = new Bitmap(filePath);
                        int position = filePath.LastIndexOf("\\") + 1;
                        textBox1.Text = filePath.Substring(position, filePath.Length - position);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (bitmap != null)
            {
                ProcessImage();
            }
        }

        private void ProcessImage()
        {
            this.button2.Enabled = false;

            int threadsNumber = (int)numericUpDown1.Value;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            ConcurrentDictionary<Color, int> colors = new ConcurrentDictionary<Color, int>();
            //List<Dictionary<Color, int>> colors = new List<Dictionary<Color, int>>();
            //for(int i = 0; i < threadsNumber; i++)
            //{
            //    colors.Add(new Dictionary<Color, int>());
            //}

            int heightJump = bitmap.Height / threadsNumber;

            int moduloHeight = bitmap.Height % threadsNumber;
            int bitmapWidth = bitmap.Width;

            int[] heightBreakpoints = new int[threadsNumber + 1];
            heightBreakpoints[0] = 0;

            for (int i = 2; i < threadsNumber + 2; i++)
            {
                heightBreakpoints[i - 1] = heightJump * (i - 1);
            }

            if (moduloHeight != 0)
            {
                heightBreakpoints[threadsNumber] = heightBreakpoints[threadsNumber] + moduloHeight;
            }


            Parallel.For(0, threadsNumber,
                index =>
                {

                    int heightStartIndex = heightBreakpoints[index];
                    int widthLimit = bitmapWidth;
                    int heightLimit = heightBreakpoints[index + 1];

                    for (int i = 0; i < widthLimit; i++)
                    {
                        for (int j = heightStartIndex; j < heightLimit; j++)
                        {
                            System.Drawing.Color pixel;
                            lock (bitmap)
                            {
                                pixel = bitmap.GetPixel(i, j);
                            }

                            //if (colors[index].ContainsKey(pixel))
                            //{
                            //    colors[index][pixel]++;
                            //}
                            //else
                            //{
                            //    colors[index][pixel] = 1;
                            //}

                            if (colors.ContainsKey(pixel))
                            {
                                colors[pixel]++;
                            }
                            else
                            {
                                colors[pixel] = 1;
                            }
                        }
                    }
            });


            //Dictionary<Color, int> allColors = new Dictionary<Color, int>();
            //foreach(var colorDictionary in colors)
            //{
            //    AddDictionaryToDictionary(allColors, colorDictionary);
            //}

            Dictionary<Color, int> allColors = colors.ToDictionary(x => x.Key, x => x.Value);

            List<KeyValuePair<Color, int>> colorsList = allColors.ToList();

            var allColorsTemp = (from color in allColors orderby color.Value descending select color).Take(10);



            sw.Stop();

            int sumOfPixels = CountSumOfPixelsInDictionary(allColors);

            allColors = allColorsTemp.ToDictionary(x => x.Key, x => x.Value);

            var countsStrBuilder = new StringBuilder();
            countsStrBuilder.AppendLine("RGB COUNTS");
            countsStrBuilder.AppendLine("R, G, B, Counts");
            PrintColors(allColors, countsStrBuilder);

            countsStrBuilder.AppendLine("Total time, Number of threads");
            countsStrBuilder.AppendLine(String.Format("{0}, {1}", sw.Elapsed + "s", threadsNumber.ToString()));
            countsStrBuilder.AppendLine("Image width, height");
            countsStrBuilder.AppendLine(String.Format("{0}, {1}", bitmap.Width, bitmap.Height));
            countsStrBuilder.AppendLine("Image * height");
            countsStrBuilder.AppendLine(String.Format("{0}", bitmap.Width * bitmap.Height));
            lastSavedImageTime = sw.Elapsed.ToString();

            countsStrBuilder.AppendLine("sum of pixels");
            countsStrBuilder.AppendLine(String.Format("{0}", sumOfPixels));

            this.textBox2.Text = lastSavedImageTime;

            SaveCsv(@"C:\Users\wojte\Desktop\imageRaport\", countsStrBuilder, "rgba.csv");

            this.button2.Enabled = true;
        }

        private void PrintColors(Dictionary<Color, int> colors, StringBuilder stringBuilder)
        {
            foreach (var color in colors)
            {
                stringBuilder.AppendLine(String.Format("{0},{1},{2}, {3}", color.Key.R, color.Key.G, color.Key.B, color.Value));
            }
        }

        private void SaveCsv(string path, StringBuilder strBuilder, string fileName)
        {
            string pathToSave = path + fileName;

            if (File.Exists(pathToSave))
            {
                File.Delete(pathToSave);
            }

            File.AppendAllText(pathToSave, strBuilder.ToString());
        }


        private void AddDictionaryToDictionary(Dictionary<Color, int> mainDictionary, Dictionary<Color, int> dictionaryToAdd)
        {
            foreach (KeyValuePair<Color, int> entry in dictionaryToAdd)
            {
                if (mainDictionary.ContainsKey(entry.Key))
                {
                    mainDictionary[entry.Key] += entry.Value;
                }
                else
                {
                    mainDictionary[entry.Key] = entry.Value;
                }
            }
        }

        private Dictionary<Color, int> AddDictionaryTwoDictionaries(Dictionary<Color, int> mainDictionary, Dictionary<Color, int> dictionaryToAdd)
        {
            Dictionary<Color, int> result = mainDictionary;
            foreach (KeyValuePair<Color, int> entry in dictionaryToAdd)
            {
                if (result.ContainsKey(entry.Key))
                {
                    result[entry.Key] += entry.Value;
                }
                else
                {
                    result[entry.Key] = entry.Value;
                }
            }

            return result;
        }


        private int CountSumOfPixelsInDictionary(Dictionary<Color, int> colors)
        {
            int sumOfPixels = 0;
            foreach (KeyValuePair<Color, int> entry in colors)
            {
                sumOfPixels += entry.Value;
            }

            return sumOfPixels;
        }
    }
}
