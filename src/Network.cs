class Network
{
    public float[] Sensor;
    public float[] Processing;
    public float[] Motor;
    public float[] Output;

    public float[] Total;
    public float[] Sum;

    public float[] Bias;
    public float[] Decay;

    public float[] Weight;
    public Network(int num_in, int num_hid, int num_out, int weight)
    {
        CreateNetwork(num_in, num_hid, num_out, weight);
    }
    public void CreateNetwork(int num_in, int num_hid, int num_out, int weight)
    {
        Sensor = new float[num_in];
        Processing = new float[num_hid];
        Motor = new float[num_out];

        Output = new float[num_in + num_hid + num_out];

        Total = new float[num_hid + num_out];
        Sum = new float[num_hid + num_out];

        Bias = new float[num_hid];
        Decay = new float[num_hid];

        Weight = new float[weight];

        for (int i = 0; i < weight; i++)
            Weight[i] = 1.0f;
    }
}