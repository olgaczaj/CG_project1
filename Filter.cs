using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CG_project1
{
    internal static class Filter
    {
        private static double gamma { get; set; }
        private static double brightnessCoeff { get; set; }

        static Filter()
        {
            gamma = 1.1;
            brightnessCoeff = 5.0;
        }
        public static void Apply(int function, WriteableBitmap wBit, MainWindow mw)
        {
            int stride = wBit.PixelWidth * wBit.Format.BitsPerPixel / 8;
            byte[] pixels = new byte[wBit.PixelHeight * stride];
            wBit.CopyPixels(pixels, stride, 0);
            switch (function)
            {
                case 0:
                    {
                        InverseFilter(pixels, wBit.BackBufferStride, wBit.PixelHeight);
                        break;
                    }
                case 1:
                    {
                        BrightnessFilter(pixels, wBit.BackBufferStride, wBit.PixelHeight);
                        break;
                    }
                case 2:
                    {
                        GammaFilter(pixels);
                        break;
                    }
                case 3:
                    {
                        ContrastFilter(pixels);
                        break;
                    }
                case 4:
                    {
                        pixels = ConvolutionFilter(pixels, mw);
                        break;
                    }
                case 5:
                    {
                        pixels = MedianFilter(pixels, wBit);
                        break;
                    }
                case 6:
                    {
                        Greyscale(pixels, wBit);
                        break;
                    }
                case 7:
                    {
                        pixels = mw.dither.Apply(pixels);
                        break;
                    }
                case 8:
                    {
                        pixels = mw.cq.Apply(pixels);
                        break;
                    }
            }
            wBit.WritePixels(new Int32Rect(0, 0, wBit.PixelWidth, wBit.PixelHeight), pixels, stride, 0);
            mw.modifiedPicture.Source = wBit;
        }

        private static void InverseFilter(byte[] pixels, int width, int height)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (byte)(255 - pixels[i]);
            }
        }

        private static void BrightnessFilter(byte[] pixels, int width, int height)//ELSE!!!
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] + brightnessCoeff <= 255 && pixels[i] + brightnessCoeff >= 0)
                {
                    pixels[i] += (byte)((int)brightnessCoeff);
                }
            }
        }

        private static void GammaFilter(byte[] pixels)
        {
            double normalisedPixel;
            for (int i = 0; i < pixels.Length; i++)
            {
                normalisedPixel = (double)pixels[i] / 255;
                normalisedPixel = Math.Pow(normalisedPixel, gamma);
                if (normalisedPixel > 1)
                    normalisedPixel = 1;
                pixels[i] = (byte)((int)(normalisedPixel * 255));
            }
        }

        private static void ContrastFilter(byte[] pixels)
        {
            double a = 1.5, help;
            for (int i = 0; i < pixels.Length; i++)
            {
                help = (pixels[i] - 255 / 2) * a + 255 / 2;
                help = Math.Min(help, 255);
                help = Math.Max(help, 0);
                pixels[i] = (byte)((int)help);
            }
        }

        private static byte[] MedianFilter(byte[] pixels, WriteableBitmap wBit) //only for black and white pictures
        {
            byte[] newPixels = new byte[pixels.Length];
            int bytesPP = wBit.Format.BitsPerPixel / 8;
            int[] neighbors = new int[9];
            int nind = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                nind = 0;
                for (int jh = -1; jh <= 1; jh++)
                {
                    for (int jw = -1; jw <= 1; jw++)
                    {
                        int ind = Math.Max(Math.Min(i / (wBit.PixelWidth * bytesPP) + jh, wBit.PixelHeight - 1), 0) *
                        wBit.PixelWidth * bytesPP + Math.Max(Math.Min(i % (wBit.PixelWidth * bytesPP) + jw * bytesPP,
                        (wBit.PixelWidth - 1) * bytesPP), 0);
                        neighbors[nind] = pixels[ind];
                        nind++;
                    }
                }
                Array.Sort(neighbors);
                newPixels[i] = (byte)neighbors[4];
            }
            return newPixels;
        }

        private static byte[] ConvolutionFilter(byte[] pixels, MainWindow mw)
        {
            byte[] newPixels = new byte[pixels.Length];
            double d;
            if (mw.autoComputeD.IsChecked == true)
                d = mw.kernel.data.Sum(x => x);
            else
                d = mw.kernel.divisor;
            if (d == 0)
            {
                d = 1;
            }
            int bytesPP = mw.wBit.Format.BitsPerPixel / 8;
            double tmpP = 0;
            for (int c = 0; c < bytesPP; c++) //leiej by było na wewnętrznej pętli
            {
                for (int i = c; i < pixels.Length; i += bytesPP)
                {
                    tmpP = 0;
                    for (int jh = -mw.kernel.anchor / mw.kernel.width; jh < mw.kernel.height - mw.kernel.anchor / mw.kernel.width; jh++)
                    {
                        for (int jw = -mw.kernel.anchor % mw.kernel.width; jw < mw.kernel.width - mw.kernel.anchor % mw.kernel.width; jw++)
                        {
                            int ind = Math.Max(Math.Min(i / (mw.wBit.PixelWidth * bytesPP) + jh, mw.wBit.PixelHeight - 1), 0) * mw.wBit.PixelWidth * bytesPP + Math.Max(Math.Min(i % (mw.wBit.PixelWidth * bytesPP) + jw * bytesPP, (mw.wBit.PixelWidth - 1) * bytesPP), 0);
                            tmpP += (double)pixels[ind] * mw.kernel.data[mw.kernel.anchor + jh * mw.kernel.width + jw] / d;
                        }
                    }
                    newPixels[i] = (byte)Math.Min(Math.Max(((int)tmpP + mw.kernel.offset), 0), 255);
                }
            }
            return newPixels;
        }

        private static void Greyscale(byte[] pixels, WriteableBitmap wBit)
        {
            int bytesPP = wBit.Format.BitsPerPixel / 8, col;
            for(int i = 0; i < pixels.Length/bytesPP; i++)
            {
                col = 0;
                for(int j = 0; j < bytesPP; j++)
                {
                    col += pixels[i * bytesPP + j];
                }
                col /= Math.Max(Math.Min(bytesPP, 255), 0);
                for (int j = 0; j < bytesPP; j++)
                {
                    pixels[i * bytesPP + j] = (byte)col;
                }
            }
        }
    }
}
