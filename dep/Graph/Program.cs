namespace Graph
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //EdgeInfo[] info = new EdgeInfo[]{
            //    new EdgeInfo(0, 1, '~'),
            //    new EdgeInfo(0, 1, '0'),
            //    new EdgeInfo(1, 2, '1'),
            //    new EdgeInfo(1, 3, '~'),
            //    new EdgeInfo(2, 2, '1'),
            //    new EdgeInfo(2, 3, '~')
            //};
            //TempDFA graph = new TempDFA(0, 3, info, new char[] {'0','1'});
            //Console.WriteLine(graph.ToString());

            EdgeInfo[] info = new EdgeInfo[]{
                new EdgeInfo(0, 1, '~'),
                new EdgeInfo(0, 2, '~'),
                new EdgeInfo(1, 1, '1'),
                new EdgeInfo(1, 1, '0'),
                new EdgeInfo(1, 3, '0'),
                new EdgeInfo(2, 2, '1')
            };
            TempDFA graph = new TempDFA(0, 3, info, new char[] { '0', '1' });
            Console.WriteLine(graph.ToString());
        }
    }
}