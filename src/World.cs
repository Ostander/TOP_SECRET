public class Map
{
    public int Width, Height;
    public int[,] Tiles; // 0 — пусто, 1 — стена

    public Map(int w, int h)
    {
        Width = w; Height = h;
        Tiles = new int[h, w];
        // Пока оставим карту пустой, потом добавим стены по краям или лабиринт
    }
}

public class World
{
    public Map Map;
    public List<Agent> Agents;

    public World(Map map)
    {
        Map = map;
        Agents = new List<Agent>();
    }

    public void Update()
    {
        foreach (var agent in Agents)
        {
            float[] sensors = agent.GetSensorData(this);
            bool[] motorSpikes = agent.Brain.Step(sensors);
            agent.Act(motorSpikes, this);
            agent.Develop();
        }
    }
}