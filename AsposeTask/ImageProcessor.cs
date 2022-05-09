using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsposeTask
{
    /// <summary>
    /// 
    /// </summary>
    public enum ComparisonAlgorithm
    {
        /// <summary>
        /// 
        /// </summary>
        ARGB,
        /// <summary>
        /// Only RGB values are taken
        /// </summary>
        RGB
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImageProcessor : IImageProcessor
    {
        public ComparisonAlgorithm Algorithm { get; set; }
        public int NumberOfDiff { get; set; }
        public int ErrorTolerance { get; set; }
        public int DepthOfDFS { get; set; }
        public int SizeOfDiff { get; set; }
        private readonly string fileName1;
        private readonly string fileName2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName1"></param>
        /// <param name="fileName2"></param>
        /// <param name="algorithm"></param>
        /// <param name="numberOfDiff"></param>
        /// <param name="errorTolerance"></param>
        /// <param name="depthOfDFS"></param>
        /// <param name="sizeOfDiff"></param>
        public ImageProcessor
            (string fileName1,
            string fileName2,
            ComparisonAlgorithm algorithm = ComparisonAlgorithm.RGB,
            int numberOfDiff = 0,
            int errorTolerance = 8,
            int depthOfDFS = 1,
            int sizeOfDiff = 150)
        {
            this.fileName1 = fileName1;
            this.fileName2 = fileName2;
            Algorithm = algorithm;
            NumberOfDiff = numberOfDiff;
            ErrorTolerance = errorTolerance;
            DepthOfDFS = depthOfDFS;
            SizeOfDiff = sizeOfDiff;
        }

        private delegate int CalculateDiff(Color pixel1, Color pixel2);

        private Dictionary<ComparisonAlgorithm, CalculateDiff> diffFormula = new Dictionary<ComparisonAlgorithm, CalculateDiff>
        {
            [ComparisonAlgorithm.ARGB] = (pixel1, pixel2) => 
            {
                return (Math.Abs(pixel1.A - pixel2.A) + Math.Abs(pixel1.R - pixel2.R) + Math.Abs(pixel1.G - pixel2.G) + Math.Abs(pixel1.B - pixel2.B)) / 4;
            },
            [ComparisonAlgorithm.RGB] = (pixel1, pixel2) =>
            {
                return (Math.Abs(pixel1.R - pixel2.R) + Math.Abs(pixel1.G - pixel2.G) + Math.Abs(pixel1.B - pixel2.B)) / 3;
            }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultFileName1"></param>
        /// <param name="resultFileName2"></param>
        /// <returns></returns>
        public async Task StartAsync(string resultFileName1, string resultFileName2)
        {
            Pen pen = new Pen(Color.Red, 5);
            Bitmap img1 = new Bitmap(fileName1);
            Bitmap img2 = new Bitmap(fileName2);

            await foreach (var cluster in CompareTwoImagesAsync(img1, img2))
            {
                Console.WriteLine(cluster.ToString());

                Rectangle rectangle = new Rectangle(cluster.LeftPoint - 5, cluster.UpperPoint - 5, cluster.RightPoint - cluster.LeftPoint + 10, cluster.LowerPoint - cluster.UpperPoint + 10);

                using (Graphics g = Graphics.FromImage(img1))
                {
                    g.DrawRectangle(pen, rectangle);
                }

                using (Graphics g = Graphics.FromImage(img2))
                {
                    g.DrawRectangle(pen, rectangle);
                }
            }

            img1.Save(resultFileName1);
            img2.Save(resultFileName2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultFileName1"></param>
        /// <param name="resultFileName2"></param>
        public void Start(string resultFileName1, string resultFileName2)
        {
            Pen pen = new Pen(Color.Red, 5);
            Bitmap img1 = new Bitmap(fileName1);
            Bitmap img2 = new Bitmap(fileName2);
            var clusters = CompareTwoImages(img1, img2);

            foreach (var cluster in clusters)
            {
                Console.WriteLine(cluster.ToString());

                Rectangle rectangle = new Rectangle(cluster.LeftPoint - 5, cluster.UpperPoint - 5, cluster.RightPoint - cluster.LeftPoint + 10, cluster.LowerPoint - cluster.UpperPoint + 10);

                using (Graphics g = Graphics.FromImage(img1))
                {
                    g.DrawRectangle(pen, rectangle);
                }

                using (Graphics g = Graphics.FromImage(img2))
                {
                    g.DrawRectangle(pen, rectangle);
                }
            }

            img1.Save(resultFileName1);
            img2.Save(resultFileName2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async IAsyncEnumerable<PixelCluster> CompareTwoImagesAsync(Bitmap img1, Bitmap img2)
        {
            if(img1.Width != img2.Width || img1.Height != img2.Height)
            {
                throw new ArgumentException("Images should be of the same size pixel-wise.");
            }

            bool[,] isVisited = new bool[img1.Width, img1.Height];
            bool[,] isDifferent =  await Task.Run(() => DetectDifferences(img1, img2));

            int size = 0;

            for(int i = 5; i < img1.Width - 5; i++)
            {
                for(int j = 5; j < img1.Height - 5; j++)
                {
                    if(NumberOfDiff > 0)
                    {
                        if(size >= NumberOfDiff)
                        {
                            continue;
                        }
                    }

                    if (isDifferent[i, j] && !isVisited[i, j])
                    {
                        PixelCluster cluster = new PixelCluster();
                        await Task.Run(() => DFS(cluster, isDifferent, i, j, isVisited));
                        cluster.SetBorders();
                        if (cluster.Size > SizeOfDiff)
                        {
                            size += 1;
                            yield return cluster;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <returns></returns>
        public IEnumerable<PixelCluster> CompareTwoImages(Bitmap img1, Bitmap img2)
        {
            if (img1.Width != img2.Width || img1.Height != img2.Height)
            {
                throw new ArgumentException("Images should be of the same size pixel-wise.");
            }

            List<PixelCluster> result = new List<PixelCluster>();

            bool[,] isVisited = new bool[img1.Width, img1.Height];
            bool[,] isDifferent = DetectDifferences(img1, img2);

            for (int i = 5; i < img1.Width - 5; i++)
            {
                for (int j = 5; j < img1.Height - 5; j++)
                {
                    if (NumberOfDiff > 0)
                    {
                        if (result.Count >= NumberOfDiff)
                        {
                            continue;
                        }
                    }

                    if (isDifferent[i, j] && !isVisited[i, j])
                    {
                        PixelCluster cluster = new PixelCluster();
                        DFS(cluster, isDifferent, i, j, isVisited);
                        cluster.SetBorders();
                        if (cluster.Size > SizeOfDiff)
                        {
                            result.Add(cluster);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <returns></returns>
        public bool[,] DetectDifferences(Bitmap img1, Bitmap img2)
        {
            bool[,] isDifferent = new bool[img1.Width, img1.Height];

            for (int i = 5; i < img1.Width - 5; i++)
            {
                for (int j = 5; j < img1.Height - 5; j++)
                {
                    if (diffFormula[Algorithm](img1.GetPixel(i, j), img2.GetPixel(i, j)) > ErrorTolerance)
                    {
                        isDifferent[i, j] = true;
                    }
                }
            }

            return isDifferent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isDifferent"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private bool IsSafe(bool[,] isDifferent, int row, int col, bool[,] visited)
        {
            return (row >= 0) && (row < isDifferent.GetLength(0)) && (col >= 0) && (col < isDifferent.GetLength(1)) && (isDifferent[row, col] && !visited[row, col]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="isDifferent"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="isVisited"></param>
        public void DFS(PixelCluster cluster, bool[,] isDifferent, int row, int col, bool[,] isVisited)
        {
            int[] rowNbr = new int[] { -DepthOfDFS, -DepthOfDFS, -DepthOfDFS, 0, 0, DepthOfDFS, DepthOfDFS, DepthOfDFS };
            int[] colNbr = new int[] { -DepthOfDFS, 0, DepthOfDFS, -DepthOfDFS, DepthOfDFS, -DepthOfDFS, 0, DepthOfDFS };

            isVisited[row, col] = true;

            cluster.XCoordinates.Add(row);
            cluster.YCoordinates.Add(col);

            for (int k = 0; k < 8; ++k)
            {
                if (IsSafe(isDifferent, row + rowNbr[k], col + colNbr[k], isVisited))
                {
                    DFS(cluster, isDifferent, row + rowNbr[k], col + colNbr[k], isVisited);
                }
            }
        }
    }
}
