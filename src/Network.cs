
class Network {
    public float[] Sensors;
    public float[] Hidden;
    public float[] Motors;
    public float[] Output;

    public float[] Sum;
    public float[] Total;

    public float[] Bias;
    public float[] Decay;

    public float[] Weights;

    public Network(int num_in, int num_hid, int num_out, int synaps)
    {
        CreateNetwork(num_in, num_hid, num_out, synaps);
    }
    public void Activate(int Neuron)
    {
        Total[Neuron] = Total[Neuron] * Decay[Neuron] + Bias[Neuron] + Sum[Neuron];
        Sum[Neuron] = 0.0f;
        Output[Neuron] = Math.Max(0.0f, Total[Neuron]);
    }
    public void CreateNetwork(int num_in, int num_hid, int num_out, int synaps)
    {

        Sensors = new float[num_in];
        Hidden = new float[num_hid];
        Motors = new float[num_out];
        Output = new float[num_out];

        Sum = new float[num_hid + num_out];
        Total = new float[num_hid + num_out];
        Bias = new float[num_hid + num_out];
        Decay = new float[num_hid + num_out];

        Weights = new float[synaps];

        for (int i = 0; i < Weights.Length; i++)
            Weights[i] = 1.0f;
    }
}