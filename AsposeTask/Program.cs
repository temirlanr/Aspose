namespace AsposeTask
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ImageProcessor processor = new ImageProcessor(ComparisonAlgorithm.RGB, 0, 5);
            Pen pen = new Pen(Color.Red, 5);

            string fileName1 = "./Samples/Sample_1_A.jpg";
            string fileName2 = "./Samples/Sample_1_B.jpg";

            Bitmap img1 = new Bitmap(fileName1);
            Bitmap img2 = new Bitmap(fileName2);

            await foreach (var cluster in processor.CompareTwoImagesAsync(fileName1, fileName2))
            {
                Rectangle rectangle = new Rectangle(cluster.LeftPoint - 5, cluster.UpperPoint - 5, cluster.RightPoint - cluster.LeftPoint + 10, cluster.UpperPoint - cluster.LowerPoint + 10);

                using (Graphics g = Graphics.FromImage(img1))
                {
                    g.DrawRectangle(pen, rectangle);
                }

                using (Graphics g = Graphics.FromImage(img2))
                {
                    g.DrawRectangle(pen, rectangle);
                }
            }

            img1.Save("result1.jpg");
            img2.Save("result2.jpg");
        }
    }
}