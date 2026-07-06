using System.Diagnostics;
class Neuron
{
    public double Out;
    public double SIn;
    public Neuron(double SIN = 0.0, double OUT = 0.0)
    {
        SIn = SIN;
        Out = OUT;
    }
    public virtual void CollectInputs()
    {
        foreach (var synapse in MySynapseList)
            SIn += synapse.Out.Out * synapse.Weight;
    }
    public virtual void Activate()
    {
        Out = Math.Max(0.0, SIn);
        SIn = 0;
    }
    public List<Synapse> MySynapseList = new List<Synapse>();
}
class SensorNeuron : Neuron
{
    public SensorNeuron(double SIN = 0.0, double OUT = 0.0)
    {

    }
    public override void CollectInputs() { }
}
class ProcessingNeuron : Neuron
{
    public double Total;
    public double Bias;
    public double Decay;
    public ProcessingNeuron(double BIAS = 0.0, double DECAY = 0.0, double TOTAL = 0.0, double SIN = 0.0, double OUT = 0.0)
    {
        Total = TOTAL;
        Bias = BIAS;
        Decay = DECAY;
    }
    public override void Activate()
    {
        Total = Total * Decay + Bias + SIn;
        SIn = 0.0;
        Out = Math.Max(0.0, Total);
    }
}

class MotorNeuron : Neuron
{
    public double Total;
    public MotorNeuron(double TOTAL = 0.0, double SIN = 0.0, double OUT = 0.0)
    {
        Total = TOTAL;
    }
}

class Synapse
{
    public Neuron Out;
    public Neuron In;
    public double Weight = 1.0;
    public Synapse(Neuron OUT, Neuron IN)
    {
        In = IN;
        Out = OUT;
    }
}

class Network0
{
    public List<SensorNeuron> Sensors = new List<SensorNeuron>();
    public List<ProcessingNeuron> Hidden = new List<ProcessingNeuron>();
    public List<MotorNeuron> Motors = new List<MotorNeuron>();

    public List<Synapse> SynapseList = new List<Synapse>();

    public Network0(int num_in, int num_hid, int num_out)
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();
        CreateNetwork(num_in, num_hid, num_out);
        sw.Stop();

        ForwardNetwork();
        Console.WriteLine($"Время создания сети: {sw.Elapsed.TotalMilliseconds} мс ({sw.ElapsedTicks} тиков таймера)");
    }
    public void CreateNetwork(int num_in, int num_hid, int num_out)
    {
        for (int i = 0; i < num_in; i++)
            Sensors.Add(new SensorNeuron());

        for (int i = 0; i < num_hid; i++)
            Hidden.Add(new ProcessingNeuron(BIAS: 0.0, DECAY: 0.99));

        for (int i = 0; i < num_out; i++)
            Motors.Add(new MotorNeuron());

        foreach (var hid in Hidden)
            foreach (var sens in Sensors)
            {
                Synapse synapse = new Synapse(sens, hid);
                SynapseList.Add(synapse);
                hid.MySynapseList.Add(synapse);
            }

        foreach (var mot in Motors)
            foreach (var hid in Hidden)
            {
                Synapse synapse = new Synapse(hid, mot);
                SynapseList.Add(synapse);
                mot.MySynapseList.Add(synapse);
            }
    }

    public void ForwardNetwork()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        for (int tick = 0; tick < 10; tick++)
        {
            if (tick == 1)
            {
                Sensors[0].SIn += 1.0;
                Sensors[1].SIn += 3.0;
            }

            foreach (var neuron in Sensors)
                neuron.Activate();

            foreach (var neuron in Hidden)
            {
                neuron.CollectInputs();
                neuron.Activate();
            }

            foreach (var neuron in Motors)
            {
                neuron.CollectInputs();
                neuron.Activate();
            }

            Console.WriteLine($"================[tick: {tick}]================");
        }

        sw.Stop();
        double averageMsPerTick = sw.Elapsed.TotalMilliseconds / 10;

        Console.WriteLine("\n================[АНАЛИЗ ВРЕМЕНИ]================");
        Console.WriteLine($"Всего времени на {10} тиков: {sw.Elapsed.TotalMilliseconds} мс");
        Console.WriteLine($"В среднем на один тик: {averageMsPerTick} мс");
        Console.WriteLine("================================================\n");
    }
}