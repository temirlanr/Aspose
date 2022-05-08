using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsposeTask
{
    public interface IImageProcessor
    {
        IAsyncEnumerable<PixelCluster> CompareTwoImagesAsync(string fileName1, string fileName2);
        IEnumerable<PixelCluster> CompareTwoImages(string fileName1, string fileName2);
        bool[,] DetectDifferences(Bitmap img1, Bitmap img2);
        void DFS(PixelCluster cluster, bool[,] isDifferent, int row, int col, bool[,] isVisited);
    }
}
