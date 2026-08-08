using System.Threading;
using System.Linq;
class Program
{
    static void Main()
    {
        // Создаём карту 30x30
        Map map = new Map(30, 30);
        World world = new World(map);

        // Рамка из стен
        for (int x = 0; x < map.Width; x++)
        {
            map.Tiles[0, x] = 1;
            map.Tiles[map.Height - 1, x] = 1;
        }
        for (int y = 0; y < map.Height; y++)
        {
            map.Tiles[y, 0] = 1;
            map.Tiles[y, map.Width - 1] = 1;
        }

        // Добавляем одного агента в центр
        Agent agent = new Agent(15, 15);

        Console.WriteLine($"ДНК агента (первые 60 нуклеотидов): {agent.Genome.ToNucleotideString()}");
        Console.WriteLine($"Трансляция в белок: {agent.Genome.ToAminoAcidString()}");

        Console.WriteLine($"Геном: {agent.Genome.ToNucleotideString()}");
        Console.WriteLine($"Белок: {agent.Genome.ToAminoAcidString()}");
        Console.WriteLine($"Фенотип: {agent.BrainParams.Describe()}");

        world.Agents.Add(agent);

        // Простейший консольный вывод каждые 100 шагов
        for (int step = 0; step < 1000; step++)
        {
            world.Update();

            // Вывод каждые 10 шагов, чтобы не мельтешило
            if (step % 100 == 0)
            {
                int sensorSpikes = agent.Brain.Spike.Take(agent.Brain.NumSensors).Count(s => s);
                int motorSpikes = agent.Brain.Spike.Skip(agent.Brain.NumSensors).Take(agent.Brain.NumMotors).Count(s => s);
                Console.WriteLine($"Шаг {step,4}: агент ({agent.X,2},{agent.Y,2}) Спайков S={sensorSpikes,3} M={motorSpikes,3} " +
                  $"Гормон={agent.Brain.HormoneLevel:F2} Удалено нейронов={agent.Brain.PrunedCount} " +
                  $"V[104]={agent.Brain.V[104]:F2} V[105]={agent.Brain.V[105]:F2}");
            }
        }

        Console.WriteLine("Симуляция завершена.");
    }
}