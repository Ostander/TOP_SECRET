public class Agent
{ 
    public int X, Y;
    public SpikingNetwork Brain;
    public Dna Genome;
    public BrainParams BrainParams;
    private int age = 0;
    public int SensorSize = 10;
    public static int NumSensors => 10 * 10;
    public static int NumMotors = 4;
    public static int NumHidden = 1000;

    public Agent(int startX, int startY, Dna? dna = null)
    {
        X = startX;
        Y = startY;
        Genome = dna ?? new Dna(50); // длина ДНК 40 байт, хватит на текущие секции
        BrainParams = DnaInterpreter.Decode(Genome);
        Brain = new SpikingNetwork(NumSensors, NumMotors, NumHidden, 50, BrainParams);
    }

    // Собирает сенсорный вектор из мира
    public float[] GetSensorData(World world)
    {
        float[] data = new float[NumSensors];
        int half = SensorSize / 2;
        for (int dy = 0; dy < SensorSize; dy++)
        {
            for (int dx = 0; dx < SensorSize; dx++)
            {
                int worldX = X + dx - half;
                int worldY = Y + dy - half;
                float val = 0f;
                if (worldX < 0 || worldX >= world.Map.Width ||
                    worldY < 0 || worldY >= world.Map.Height)
                {
                    val = 1.0f; // стена за границей
                }
                else if (worldX == X && worldY == Y)
                {
                    val = 1.0f; // сам агент (не 1.0, чтобы отличаться от стены)
                }
                else
                {
                    // Проверим, есть ли в этой клетке другой агент (пока просто стены)
                    if (world.Map.Tiles[worldY, worldX] == 1) // 1 = стена
                        val = 1.0f;
                    else
                        val = 0.0f;
                }
                data[dy * SensorSize + dx] = val;
            }
        }
        return data;
    }

    // Выполняет действия на основе моторных спайков
    public void Act(bool[] motorSpikes, World world)
    {
        if (motorSpikes[0]) Y -= 1; // вверх
        if (motorSpikes[1]) Y += 1; // вниз
        if (motorSpikes[2]) X -= 1; // влево
        if (motorSpikes[3]) X += 1; // вправо

        // Ограничиваем стенами (простейшая коллизия)
        if (X < 0) X = 0;
        if (X >= world.Map.Width) X = world.Map.Width - 1;
        if (Y < 0) Y = 0;
        if (Y >= world.Map.Height) Y = world.Map.Height - 1;
    }

    public void Develop()
    {
        age++;
        // Рост по времени
        if (age == BrainParams.GrowthTick && BrainParams.GrowthNeuronCount > 0)
        {
            for (int i = 0; i < BrainParams.GrowthNeuronCount; i++)
            {
                int idx = Brain.AddNeuron(2, BrainParams.DefaultThreshold, BrainParams.DefaultDecay, BrainParams.DefaultBias);
                // Добавляем случайные связи (как раньше)
                // ...
            }
            Console.WriteLine($"Agent at ({X},{Y}) grew {BrainParams.GrowthNeuronCount} neurons at tick {age}");
        }
        // Прунинг вызывается внутри Brain.Step автоматически
    }
}
