using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using Microsoft.Win32;


namespace Cia_Decrypt {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static string Path = "";

        public static int _width = 0;
        public static int _height = 0;
        public static string _pixelePalette = "";
        public static BitmapMaker _Bmp;
        public static string _message = "";
        public static Color[] _aryColors;

        public MainWindow() {
            InitializeComponent();
        }

        private void btndecode_Click(object sender, RoutedEventArgs e) {

            if (_Bmp == null) {
                lblError.Content = ("Please load an PPm image before you decode");
            } else {
                DecodeMessage(_Bmp);
                txtBoxMessage.Text = _message;
            }//end else
        }
        private void muOpen_Click(object sender, RoutedEventArgs e) {
            //creat open file dialog
            OpenFileDialog OpenFileDialog = new OpenFileDialog();

            //setup parameters for our open fiel dialog
            OpenFileDialog.DefaultExt = ".ppm";
            OpenFileDialog.Filter = "PPM Files (.ppm)|*.ppm";

            //SHOW FILE DIALOG
            bool? result = OpenFileDialog.ShowDialog();


            //PROCESS DIALOG RESULTS
            if (result == true) {
                Path = OpenFileDialog.FileName; //FileName sotres the filepath to this property. 

                //CALL LOADIMAGE IF A FILE HAS BEEN SELECTED

                LoadFromPpm(Path);
            }//end if


        }//end event

        private void LoadFromPpm(string path) {
            StreamReader infile = new StreamReader(path);
            string type = infile.ReadLine();
            if (type == "P3") {
                //grabs the first 3 lines
                string words = infile.ReadLine();
                string dimensions = infile.ReadLine();
                string MaxColor = infile.ReadLine();

                //grabs colors
                string palette = infile.ReadToEnd();

                //closes file
                infile.Close();

                //splits width and height
                string[] aryDimensions = dimensions.Split();
                int width = int.Parse(aryDimensions[0]);
                int height = int.Parse(aryDimensions[1]);
                _width = width;
                _height = height;
                //splits into an array
                string[] aryPalette = palette.Split("\n");

                //array of colors
                Color[] aryColors = new Color[aryPalette.Length / 3];
                _aryColors = aryColors;
                //loop through the array length grab 3 numbers and getb 
                for (int paletteIndex = 0; paletteIndex * 3 < aryPalette.Length - 1; paletteIndex++) {
                    int newIndex = paletteIndex * 3;

                    aryColors[paletteIndex].A = 255;
                    aryColors[paletteIndex].R = byte.Parse(aryPalette[newIndex]);
                    aryColors[paletteIndex].G = byte.Parse(aryPalette[++newIndex]);
                    aryColors[paletteIndex].B = byte.Parse(aryPalette[++newIndex]);

                }//end for

                BitmapMaker bmpMaker = new BitmapMaker(width, height);
                int plotX = 0;
                int plotY = 0;
                int colorIndex = 0;

                for (int index = 0; index < aryPalette.Length; index++) {
                    Color plotColor = aryColors[colorIndex];

                    bmpMaker.SetPixel(plotX, plotY, plotColor);

                    plotX++;

                    if (plotX == width) {
                        plotX = 0;
                        plotY++;
                    }//end if
                    if (plotY == height) {
                        break;
                    }
                    colorIndex++;
                }//end for

                //Creates new bitmap
                WriteableBitmap wbmImage = bmpMaker.MakeBitmap();

                //Display Bitmap
                imgMain.Source = wbmImage;
                _Bmp = bmpMaker;
            }//end if


        }//end method
        
        public static void DecodeMessage(BitmapMaker bmpMaker) {
            StringBuilder binaryMessage = new StringBuilder();

            for (int y = 0; y < bmpMaker.Height; y++) {
                for (int x = 0; x < bmpMaker.Width; x++) {
                    // Get pixel color
                    Color pixelColor = bmpMaker.GetPixelColor(x, y);

                    // Decode message bits from color channels
                    byte r = (byte)(pixelColor.R & 1);
                    byte g = (byte)(pixelColor.G & 1);
                    byte b = (byte)(pixelColor.B & 1);

                    // Append message bits to binary string
                    binaryMessage.Append(r);
                    binaryMessage.Append(g);
                    binaryMessage.Append(b);

                    // Check for end of text signal
                    if (binaryMessage.Length >= 8 && binaryMessage.ToString(binaryMessage.Length - 8, 8) == "00001000") {
                        // Remove padding zeros at end of binary message
                        int paddingCount = 0;
                        for (int i = binaryMessage.Length - 9; i >= 0; i--) {
                            if (binaryMessage[i] == '0') {
                                paddingCount++;
                            } else {
                                break;
                            }
                        }

                        // Convert binary string to text
                        StringBuilder message = new StringBuilder();
                        for (int i = 0; i < binaryMessage.Length - paddingCount - 8; i += 8) {
                            string binaryChar = binaryMessage.ToString(i, 8);
                            char c = (char)Convert.ToByte(binaryChar, 2);
                            message.Append(c);
                        }

                        // Populate global message string and return
                        _message = message.ToString();
                        return;
                    }
                }
            }
        }

   
    }//end class
}//end namespace

