using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsposeTask
{
    public class GUI
    {
        PictureBox pictureBox1 = new PictureBox();
        PictureBox pictureBox2 = new PictureBox();
        Pen pen = new Pen(Color.Red, 5);
        ImageProcessor processor = new ImageProcessor();
        string fileName1 = "./Samples/Sample_1_A.jpg";
        string fileName2 = "./Samples/Sample_1_B.jpg";

        public async Task Initialize()
        {
            Bitmap img1 = new Bitmap(fileName1);
            Bitmap img2 = new Bitmap(fileName2);

            pictureBox1.Image = img1;
            pictureBox2.Image = img2;

            await foreach (var cluster in processor.CompareTwoImagesAsync(fileName1, fileName2))
            {
                Rectangle rectangle = new Rectangle(cluster.LeftPoint - 5, cluster.UpperPoint - 5, cluster.RightPoint - cluster.LeftPoint + 10, cluster.UpperPoint - cluster.LowerPoint + 10);

                using(Graphics g = Graphics.FromImage(img1))
                {
                    g.DrawRectangle(pen, rectangle);
                }

                using(Graphics g = Graphics.FromImage(img2))
                {
                    g.DrawRectangle(pen, rectangle);
                }
            }
        }
    }
}
