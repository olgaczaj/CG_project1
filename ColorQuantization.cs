using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_project1
{
    public class ColorQuantization
    {
        public int colors { get; set; } = 64;

        public byte[] Apply(byte[] pixels)
        {
            (byte red, byte green, byte blue, int ind)[] pix = new (byte, byte, byte, int)[pixels.Length / 4];
            for (int i = 0; i < pix.Length; i++) 
            {
                pix[i] = (pixels[4*i], pixels[4*i+1], pixels[4*i+2], i);
            }
            List<Bucket> buckets = [new Bucket(pix, 255, 255, 255)];
            for(int c = 1; c < colors; c *=2)
            {
                List<Bucket> newBuckets = new List<Bucket>();
                for(int i = 0; i < Math.Min(c, colors - c); i++)
                {
                    var (b1, b2) = buckets[i].Divide();
                    newBuckets.Add(b1);
                    newBuckets.Add(b2);
                }
                for(int i = Math.Min(c, colors - c); i < buckets.Count; i++)
                {
                    newBuckets.Add(buckets[i]);
                }
                buckets = newBuckets;
            }
            int ind = 0;
            foreach(Bucket bucket in buckets)
            {
                bucket.setColor();
                for(int i = 0; i < bucket.pixels.Length; i++)
                {
                    pix[ind+i] = bucket.pixels[i];
                }
                ind += bucket.pixels.Length;
            }
            foreach(var p in pix)
            {
                pixels[p.Item4 * 4] = p.Item1;
                pixels[p.ind * 4 + 1] = p.green;
                pixels[p.ind*4 + 2] = p.blue;
            }
            return pixels;
        }
    }

    internal class Bucket
    {
        public int redWidth { get; set; }
        public int greenWidth { get; set; }
        public int blueWidth { get; set; }
        public (byte, byte, byte, int)[] pixels { get; set; }

        public Bucket((byte, byte, byte, int)[] pixels, int redW, int greenW, int blueW)
        {
            this.redWidth = redW;
            this.greenWidth = greenW;
            this.blueWidth = blueW;
            this.pixels = new (byte, byte, byte, int)[pixels.Length];
            pixels.CopyTo(this.pixels, 0);
        }

        public (Bucket, Bucket) Divide()
        {
            if(redWidth >= greenWidth && redWidth >= blueWidth)
            {
                Array.Sort(pixels, delegate ((byte, byte, byte, int) x, (byte, byte, byte, int) y)
                {
                    return x.Item1.CompareTo(y.Item1);
                });
                return (new Bucket(pixels[0..(pixels.Length / 2)], redWidth / 2, greenWidth, blueWidth), 
                    new Bucket(pixels[(pixels.Length / 2 + 1)..(pixels.Length)], redWidth / 2, greenWidth, blueWidth));
            }
            if (greenWidth >= redWidth && greenWidth >= blueWidth)
            {
                Array.Sort(pixels, delegate ((byte, byte, byte, int) x, (byte, byte, byte, int) y)
                {
                    return x.Item2.CompareTo(y.Item2);
                });
                return (new Bucket(pixels[0..(pixels.Length / 2)], redWidth, greenWidth/2, blueWidth),
                    new Bucket(pixels[(pixels.Length / 2 + 1)..(pixels.Length)], redWidth, greenWidth/2, blueWidth));
            }
            Array.Sort(pixels, delegate ((byte, byte, byte, int) x, (byte, byte, byte, int) y)
            {
                return x.Item3.CompareTo(y.Item3);
            });
            return (new Bucket(pixels[0..(pixels.Length / 2)], redWidth, greenWidth, blueWidth/2),
                new Bucket(pixels[(pixels.Length / 2 + 1)..(pixels.Length)], redWidth, greenWidth, blueWidth/2));
        }

        public void setColor()
        {
            int red = 0, green = 0, blue = 0;
            foreach(var (r, g, b, i) in pixels)
            {
                red += r;
                green += g;
                blue += b;
            }
            red /= pixels.Length; green /= pixels.Length; blue /= pixels.Length;
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i].Item1 = (byte)red;
                pixels[i].Item2 = (byte)green;
                pixels[i].Item3 = (byte)blue;
            }
        }
    }
}
