using System;
using System.CodeDom;
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
            return KMeans(pixels);
        }

        public byte[] KMeans(byte[] pixels)
        {
            Random random = new Random();

            Pixel[] centroids = new Pixel[colors];
            for(int i = 0; i < colors; i++)
            {
                centroids[i] = new Pixel((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
            }

            List<(Pixel, Pixel)> map = new List<(Pixel, Pixel)>();
            for (int i = 0; i < pixels.Length; i+=4)
            {
                var p1 = new Pixel(pixels[i], pixels[i + 1], pixels[i + 2]);
                var (p2, dist) = p1.FindNearest(centroids);

                map.Add((p1, p2));
            }

            map = FindCentroids(map, centroids);

            byte[] result = new byte[pixels.Length];
            for (int i = 0;i < map.Count; i++)
            {
                result[4 * i] = map[i].Item2.r;
                result[4*i+1] = map[i].Item2.g;
                result[4*i+2] = map[i].Item2.b;
                result[4 * i + 3] = 0;
            }

            return result;
        }

        private List<(Pixel, Pixel)> FindCentroids(List<(Pixel, Pixel)> map, Pixel[] centroids)
        {
            Pixel newValue;
            bool isChanged = true;
            while (isChanged)
            {
                isChanged = false;
                for (int i = 0; i < colors; i++)
                {
                    newValue = AvgPixel(map.FindAll(p => p.Item2 == centroids[i]));
                    if (newValue != centroids[i])
                    {
                        isChanged = true;
                        centroids[i] = newValue;
                    }
                }

                //reassignment
                for (int i = 0; i < map.Count; i++)
                {
                    var (nearest, dist) = map[i].Item1.FindNearest(centroids);
                    if (nearest != map[i].Item2)
                    {
                        isChanged = true;
                        map[i] = (map[i].Item1, nearest);
                    }
                }
            }
            return map;
        }

        private Pixel AvgPixel(List<(Pixel, Pixel)> map)
        {
            int sumr = 0, sumg = 0, sumb = 0;
            foreach (var m in map) {
                sumr += m.Item1.r;
                sumg += m.Item1.g;
                sumb += m.Item1.b;
            }
            byte r, g, b;
            if(map.Count == 0)
            {
                r = 0;
                g = 0;
                b = 0;
            }
            else
            {
                r = (byte)(sumr / map.Count);
                g = (byte)(sumg / map.Count);
                b = (byte)(sumb / map.Count);
            }

            return new Pixel(r, g, b);
        }


        public byte[] OldCQ(byte[] pixels)
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

    internal class Pixel
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }

        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public double distance(Pixel x)
        {
            double res = Math.Pow((r - x.r), 2) + Math.Pow((g - x.g), 2) + Math.Pow((b - x.b), 2);
            return Math.Sqrt(res);
        }

        public (Pixel, double) FindNearest(Pixel[] arr)
        {
            double minDist = this.distance(arr[0]);
            int minInd = 0;
            double dist;
            for(int i =  1; i < arr.Length; i++)
            {
                dist = this.distance(arr[i]);
                if(dist < minDist)
                {
                    minInd = i;
                    minDist = dist;
                }
            }
            return (arr[minInd], minDist);
        }

        public static bool operator==(Pixel left, Pixel right)
        {
            return left.r==right.r && left.g==right.g && left.b==right.b;
        }

        public static bool operator!=(Pixel left, Pixel right)
        {
            return !(left==right);
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
