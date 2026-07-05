public abstract class Entity
{
    public int X;
    public int Y;
    public Controller Controller;

    protected Entity(int y, int x, Controller controller)
    {
        Y = y;
        X = x;
        Controller = controller;
    }

    public virtual void Update(World world)
    {
        var action = Controller.GetAction(this, world);
        X += action.DeltaX;
        Y += action.DeltaY;
    }
}
public abstract class Controller
{
    public abstract Action GetAction(Entity entity, World world);
}
public class Action
{
    public int DeltaX;
    public int DeltaY;
}
class Agent : Entity
{
    public Agent(int y, int x, Controller controller) : base(y, x, controller)
    {

    }
}
class Enemy : Entity
{
    public Enemy(int y, int x, Controller controller) : base(y, x, controller)
    {

    }
}
class God : Entity
{
    public God(int y, int x, Controller controller) : base(y, x, controller)
    {

    }
}
class Test : Entity
{
    public Test(int y, int x, Controller controller) : base(y, x, controller)
    {

    }
}
class AgentController : Controller
{
    public override Action GetAction(Entity entity, World world)
    {
        return new Action { DeltaX = 0, DeltaY = 0 };
    }
}
class EnemyController : Controller
{
    public override Action GetAction(Entity entity, World world)
    {
        return new Action { DeltaX = 0, DeltaY = 0 };
    }
}
class GodController : Controller
{
    public override Action GetAction(Entity entity, World world)
    {
        return new Action { DeltaX = 0, DeltaY = 0 };
    }
}
class TestController : Controller
{
    public override Action GetAction(Entity entity, World world)
    {
        return new Action { DeltaX = 0, DeltaY = 0 };
    }
}
class NullController : Controller
{
    public override Action GetAction(Entity entity, World world)
    {
        return new Action { DeltaX = 0, DeltaY = 0 };
    }
}
public class Map
{
    public int Width;
    public int Height;
    public Map(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
public class World
{
    public Map Map;
    public List<Entity> Entities;
    public World(Map map)
    {
        Map = map;
        Entities = new List<Entity>();
    }
    public void Update()
    {

    }
}
class Renderer
{
    static Renderer()
    {

    }
    public void Draw(World world)
    {

    }
}
class InputHandler
{

}
class GameState
{
    public GameState()
    {

    }
}
class Project1
{
    static void Main1()
    {

    }
}