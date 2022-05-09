using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsposeTask
{
    /// <summary>
    /// summary
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task StartAsync(string resultFileName1, string resultFileName2);
        void Start(string resultFileName1, string resultFileName2);
        IAsyncEnumerable<PixelCluster> CompareTwoImagesAsync(Bitmap img1, Bitmap img2);
        IEnumerable<PixelCluster> CompareTwoImages(Bitmap img1, Bitmap img2);
        bool[,] DetectDifferences(Bitmap img1, Bitmap img2);
        void DFS(PixelCluster cluster, bool[,] isDifferent, int row, int col, bool[,] isVisited);
    }
}
