using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_project1
{
    public class Kernel
    {
        public int anchor { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public int[] data { get; set; }

        public double divisor { get; set; }
        public double offset { get; set; }

        public Kernel(int anchor = 4, int width = 3, int height = 3, double d = 1, double of = 0, string mode = "")
        {
            this.anchor = anchor;
            this.width = width;
            this.height = height;
            divisor = d;
            offset = of;
            data = new int[height * width];
            switch (mode)
            {
                case "":
                    {
                        for (int i = 0; i < height * width; i++)
                        {
                            data[i] = 0;
                        }
                        data[anchor] = 1;
                        break;
                    }
                case "blur":
                    {
                        for (int i = 0; i < height * width; i++)
                        {
                            data[i] = 1;
                        }
                        break;
                    }
                case "gauss":
                    {
                        data = [0, 1, 0, 1, 4, 1, 0, 1, 0];
                        height = 3;
                        width = 3;
                        anchor = 4;
                        break;
                    }
                case "sharpen":
                    {
                        for (int i = 0; i < height * width; i++)
                        {
                            data[i] = -1;
                        }
                        height = 3;
                        width = 3;
                        anchor = 4;
                        data[anchor] = 9;
                        break;
                    }
                case "edge":
                    {
                        for (int i = 0; i < height * width; i++)
                        {
                            data[i] = 0;
                        }
                        data[anchor] = 1;
                        data[Math.Max(anchor - width, 0)] = -1;
                        offset = 100;
                        break;
                    }
                case "emboss":
                    {
                        data = [-1, 0, 1, -1, 1, 1, -1, 0, 1];
                        height = 3;
                        width = 3;
                        anchor = 4;
                        break;
                    }
            }
        }

        public Kernel(int[] oldData, int dataWidth, int dataHeight, int width, int height, int anchor, double d = 1, double o = 0)
        {
            this.anchor = anchor;
            this.width = width;
            this.height = height;
            divisor = d;
            offset = o;
            this.data = new int[width * height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (j < dataWidth)
                        data[i * width + j] = oldData[i * dataWidth + j];
                    else
                        data[i * width + j] = 0;
                }
            }
        }
    }
}
