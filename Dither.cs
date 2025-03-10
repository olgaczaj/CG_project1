using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CG_project1
{
    public class Dither
    {
        public int colors {  get; set; }

        public Dither()
        {
            colors = 8;
        }

        public byte[] Apply(byte[] pixels)
        {
            pixels = ConvertToYCC(pixels);
            Random random = new Random();
            byte[] result = new byte[pixels.Length];
            for(int i = 0; i < pixels.Length; i+=4)
            {
                //losowanie (0, colorWidth)
                int colorWidth = 255 / (colors - 1);
                int thresholds = random.Next(0, colorWidth);
                for (int j = 0; j < 3; j++)
                {
                    int color = pixels[i + j] * (colors - 1) / 255;
                    byte col;
                    if (pixels[i + j] == 128)
                        col = (byte)128;
                    else if (pixels[i + j] > thresholds + (color * colorWidth))
                        col = (byte)((color + 1) * colorWidth);
                    else
                        col = (byte)(color * colorWidth);
                    result[i+j] = col;
                }
            }
            return ConvertFromYCC(result);
        }

        private byte[] ConvertToYCC(byte[] pixels)
        {
            byte[] res = new byte[pixels.Length];
            pixels.CopyTo(res, 0);
            for(int i = 0; i < res.Length; i+=4)
            {
                res[i] = (byte)(Math.Max(Math.Min(0.299 * pixels[i] + 0.587 * pixels[i + 1] + 0.114 * pixels[i + 2], 255), 0));
                res[i+1] = (byte)(Math.Max(Math.Min(128 - 0.168736 * pixels[i] - 0.331264 * pixels[i+1] + 0.5 * pixels[i+2], 255), 0));
                res[i+2] = (byte)(Math.Max(Math.Min(128 + 0.5 * pixels[i] - 0.418688 * pixels[i+1] - 0.081312 * pixels[i+2], 255), 0));
            }
            return res;
        }

        private byte[] ConvertFromYCC(byte[] pixels)
        {
            byte[] res = new byte[pixels.Length];
            pixels.CopyTo(res, 0);
            for (int i = 0; i < res.Length; i += 4)
            {
                res[i] = (byte)(Math.Max(Math.Min(pixels[i] + 1.402 * (pixels[i + 2]-128), 255), 0));
                res[i + 1] = (byte)(Math.Max(Math.Min(pixels[i] - 0.344136 * (pixels[i + 1] - 128) - 0.714136 * (pixels[i + 2] - 128), 255), 0));
                res[i + 2] = (byte)(Math.Max(Math.Min(pixels[i] + 1.772 * (pixels[i + 1] - 128), 255), 0));
            }
            return res;
        }
    }


}
