namespace AsposeTask
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IImageProcessor processor = new ImageProcessor(ComparisonAlgorithm.RGB, 0, 8, 1, 150);
            await processor.StartAsync("./Samples/Sample_1_A.jpg", "./Samples/Sample_3_B.jpg", "result3A.jpg", "result3B.jpg");
        }
    }
}