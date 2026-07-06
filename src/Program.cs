class Project
{
    static void Main()
    {
        Network Network = new Network(1000, 8990, 10, 100000);
        Network0 Network0 = new Network0(2, 1, 1);
        Console.WriteLine(Network.Weights[0]);
        string? name = Console.ReadLine();
    }
}