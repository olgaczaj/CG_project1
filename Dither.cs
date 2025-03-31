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

        private int count {  get; set; }

        public Dither()
        {
            colors = 8;
        }

        public byte[] Apply(byte[] pixels)
        {
            return AverageDithering(pixels);
        }

        private List<int[]> ComputeThreshold(List<byte> r, List<byte> g, List<byte> b, int depth)
        {
            //int sumr = 0, sumg = 0, sumb = 0;
            int[] result = new int[3];
            List<int[]> res = new List<int[]>();
            
            //for (int i = 0; i < r.Count; i++)
            //{
            //    sumr += r[i];
            //}

            //for (int i = 0; i < g.Count; i++)
            //{
            //    sumg += g[i];
            //}

            //for (int i = 0; i < b.Count; i++)
            //{
            //    sumg += b[i];
            //}

            result[0] = (int)(r.Average(x => x) + 0.5);
            result[1] = (int)(g.Average(x => x) + 0.5);
            result[2] = (int)(b.Average(x => x) + 0.5);

            int indr = r.FindLastIndex(x => x <= result[0]),
                        indg = g.FindLastIndex(x => x <= result[1]),
                        indb = b.FindLastIndex(x => x <= result[2]);

            int rest = (int)((Math.Pow(2, depth)) - colors + 1);

            if (rest <= 0 || (count < rest && rest < colors - 1))
            {
                var left = ComputeThreshold(r[..indr], g[..indg], b[..indb], depth + 1);
                var right = ComputeThreshold(r[indr..], g[indg..], b[indb..], depth + 1);

                foreach ( var l in left )
                {
                    res.Add(l);
                }
                res.Add(result);
                foreach (var l in right )
                { 
                    res.Add(l); 
                }
            }
            else
            {
                res.Add(result);
                count++;
            }

            return res;
        }

        //private int[][] ComputeAllThresholds(byte[] pixels)
        //{
        //    List<byte> r = new List<byte>(), g = new List<byte>(), b = new List<byte>();

        //    int[][] result = new int[colors-1][];

        //    for (int i = 0; i < pixels.Length; i += 3)
        //    {
        //        r.Add(pixels[i]);
        //        g.Add(pixels[i + 1]);
        //        b.Add(pixels[i + 2]);
        //    }

        //    r.Sort();
        //    g.Sort();
        //    b.Sort();

        //    int r2 = 255, g2 = 255, b2 = 255;

        //    for (int i = 1; i < colors ; i *=2)
        //    {
        //        int part = colors / (i * 2);
        //        int indr1 = 0, indg1 = 0, indb1 = 0;
        //        for (int j = part - 1; j < colors - 1; j+= part)
        //        {
        //            int indr = r.FindLastIndex(x => x <= r2),
        //                indg = g.FindLastIndex(x => x <= g2),
        //                indb = b.FindLastIndex(x => x <= b2);

        //            result[j] = new int[3];
        //            result[j][0] = (int)r[indr1..indr].Average(x => (int)x);
        //            result[j][1] = (int)g[indg1..indg].Average(x => (int)x);
        //            result[j][2] = (int)b[indb1..indb].Average(x => (int)x);

        //            indr1 = indr;
        //            indg1 = indg;
        //            indb1 = indb;

        //            if(i != 1)
        //            {
                        
        //            }
        //        }

        //    }
        //}

        private byte[] AverageDithering(byte[] pixels)
        {
            List<byte> r = new List<byte>(), g = new List<byte>(), b = new List<byte>();
            for (int i = 0; i < pixels.Length; i += 4)
            {
                r.Add(pixels[i]);
                g.Add(pixels[i + 1]);
                b.Add(pixels[i + 2]);
            }

            r.Sort();
            g.Sort();
            b.Sort();

            count = 0;

            List<int[]> thresholds = ComputeThreshold(r, g, b, 1);

            byte[] result = new byte[pixels.Length];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                for (int j = 0; j < 3; j++)
                {
                    int c;
                    if (thresholds[0][j] > pixels[i + j])
                        c = 0;
                    else
                        c = thresholds.FindLast(t => t[j] <= pixels[i + j])[j];
                    result[i + j] = (byte)c;
                }
            }

            return result;
        }

        public byte[] RandomDithering(byte[] pixels)
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
