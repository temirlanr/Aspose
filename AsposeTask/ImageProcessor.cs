using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsposeTask
{
    public class ImageProcessor
    {
        public ComparisonAlgorithm Algorithm { get; private set; }
        public int NumberOfDiff { get; private set; }
        public int ErrorTolerance { get; private set; }
        public int DepthOfDFS { get; private set; }
        public int SizeOfDiff { get; private set; }

        public ImageProcessor
            (ComparisonAlgorithm algorithm = ComparisonAlgorithm.RGB,
            int numberOfDiff = 0,
            int errorTolerance = 5,
            int depthOfDFS = 1,
            int sizeOfDiff = 100)
        {
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

        public async IAsyncEnumerable<PixelCluster> CompareTwoImagesAsync(string fileName1, string fileName2)
        {
            Bitmap img1 = new Bitmap(fileName1);
            Bitmap img2 = new Bitmap(fileName2);

            bool[,] isVisited = new bool[img1.Width, img1.Height];
            bool[,] isDifferent = DetectDifferences(img1, img2);

            int size = 0;

            for(int i = 5; i < img1.Width - 5; i++)
            {
                for(int j = 5; j < img1.Height - 5; j++)
                {
                    if(isDifferent[i, j] && !isVisited[i, j] && NumberOfDiff > 0 && size < NumberOfDiff)
                    {
                        PixelCluster cluster = new PixelCluster();
                        await Task.Run(() => DFS(cluster, isDifferent, i, j, isVisited));
                        cluster.SetBorders();
                        if (cluster.Size > SizeOfDiff)
                        {
                            yield return cluster;
                            size += 1;
                        }
                    }
                }
            }
        }

        public IEnumerable<PixelCluster> CompareTwoImages(string fileName1, string fileName2)
        {
            Bitmap img1 = new Bitmap(fileName1);
            Bitmap img2 = new Bitmap(fileName2);

            List<PixelCluster> result = new List<PixelCluster>();

            bool[,] isVisited = new bool[img1.Width, img1.Height];
            bool[,] isDifferent = DetectDifferences(img1, img2);

            for (int i = 5; i < img1.Width - 5; i++)
            {
                for (int j = 5; j < img1.Height - 5; j++)
                {
                    if (isDifferent[i, j] && !isVisited[i, j] && NumberOfDiff > 0 && result.Count < NumberOfDiff)
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

        private bool[,] DetectDifferences(Bitmap img1, Bitmap img2)
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

        private bool IsSafe(bool[,] isDifferent, int row, int col, bool[,] visited)
        {
            return (row >= 0) && (row < isDifferent.GetLength(0)) && (col >= 0) && (col < isDifferent.GetLength(1)) && (isDifferent[row, col] && !visited[row, col]);
        }

        private void DFS(PixelCluster cluster, bool[,] isDifferent, int row, int col, bool[,] isVisited)
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

    public enum ComparisonAlgorithm
    {
        ARGB,
        RGB
    }
}
