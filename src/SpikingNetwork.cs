using System;

public class SpikingNetwork
{
    public float HormoneLevel => hormoneLevel;
    public int PrunedCount { get; private set; } = 0;

    private int removalCounter = 0; // для отложенного вызова

    private float hormoneLevel;
    private float hormoneTarget;

    private bool[] alive;
    private List<int> pendingRemoval = new List<int>();

    public Random rnd;

    public int TotalNeurons;
    public int TotalSynapses;

    private BrainParams brainParams;
    private int[] silenceCounter;
    private bool[] rescueMode;
    private int[] rescueTimer;

    // Типы нейронов: 0=сенсор, 1=мотор, 2=внутренний
    public int[] NeuronType;

    // Состояния нейронов
    public float[] V;           // мембранный потенциал
    public float[] I;           // входной ток (накапливается для следующего шага)
    public float[] Threshold;   // порог срабатывания
    public float[] Decay;       // скорость утечки (0..1)
    public float[] Bias;        // постоянная добавка

    // Выходные спайки (флаги на текущем шаге)
    public bool[] Spike;

    // Синапсы в разреженном формате (CSR)
    public int[] SynTarget;     // целевой нейрон для каждого синапса
    public Half[] SynWeight;    // вес синапса (Half = 2 байта)
    public int[] NOutStart;     // начало исходящих связей нейрона в SynTarget
    public int[] NOutCount;     // количество исходящих связей у нейрона

    // Параметры, чтобы знать границы
    public int NumSensors;
    public int NumMotors;
    public int NumHidden;

    public SpikingNetwork(int numSensors, int numMotors, int numHidden,
                          float connectionsPerNeuron, BrainParams bp)
    {
        hormoneLevel = bp.HormoneBaseline; hormoneTarget = bp.HormoneBaseline;

        this.brainParams = bp;

        NumSensors = numSensors;
        NumMotors = numMotors;
        NumHidden = numHidden;
        TotalNeurons = numSensors + numMotors + numHidden;
        TotalSynapses = (int)(TotalNeurons * connectionsPerNeuron);

        // Выделяем память
        V = new float[TotalNeurons];
        I = new float[TotalNeurons];
        Threshold = new float[TotalNeurons];
        Decay = new float[TotalNeurons];
        Bias = new float[TotalNeurons];
        NeuronType = new int[TotalNeurons];
        Spike = new bool[TotalNeurons];

        SynTarget = new int[TotalSynapses];
        SynWeight = new Half[TotalSynapses];
        NOutStart = new int[TotalNeurons];
        NOutCount = new int[TotalNeurons];

        silenceCounter = new int[TotalNeurons];
        rescueMode = new bool[TotalNeurons];
        rescueTimer = new int[TotalNeurons];

        alive = new bool[TotalNeurons];

        // Инициализация нейронов
        rnd = new Random(); // фиксированный seed для повторяемости
        for (int i = 0; i < TotalNeurons; i++)
        {
            V[i] = 0f;
            I[i] = 0f;

            Decay[i] = bp.DefaultDecay;
            Bias[i] = (NeuronType[i] == 2) ? bp.DefaultBias : 0f;

            if (i < numSensors)
                NeuronType[i] = 0;
            else if (i < numSensors + numMotors)
                NeuronType[i] = 1;
            else
                NeuronType[i] = 2;

            alive[i] = true;
        }

        // Создаём случайные разреженные связи
        int synIdx = 0;
        for (int src = 0; src < TotalNeurons; src++)
        {
            NOutStart[src] = synIdx;
            // Для каждого нейрона создаём connectionsPerNeuron исходящих связей
            int outCount = (int)connectionsPerNeuron;
            // Чтобы не выйти за границу массива для последних нейронов,
            // можно чуть урезать, но при точном расчёте TotalSynapses должно хватить.
            if (synIdx + outCount > TotalSynapses)
                outCount = TotalSynapses - synIdx;
            NOutCount[src] = outCount;

            for (int j = 0; j < outCount; j++)
            {
                int target = rnd.Next(TotalNeurons);
                SynTarget[synIdx] = target;
                // Вес случайный в диапазоне [-0.5, 0.5]
                float w = (float)(rnd.NextDouble() * 2.0 - 1.0) * 0.5f;
                SynWeight[synIdx] = (Half)w;
                synIdx++;
            }
        }
    }

    // Один шаг симуляции: передаём значения сенсоров, получаем спайки моторов
    public bool[] Step(float[] sensorInput)
    {
        hormoneLevel = hormoneLevel * brainParams.HormoneDecay + hormoneTarget * (1 - brainParams.HormoneDecay);
        // Пока hormoneTarget меняться не будет (позже его можно привязать к ошибке предсказания).

        // 1. Загружаем сенсоры и сразу рассылаем их спайки
        for (int i = 0; i < NumSensors; i++)
        {
            V[i] = sensorInput[i];
            if (V[i] >= Threshold[i])
            {
                Spike[i] = true;
                V[i] = 0f;

                // Рассылаем спайк этого сенсора
                int start = NOutStart[i];
                int end = start + NOutCount[i];
                for (int s = start; s < end; s++)
                {
                    int tgt = SynTarget[s];
                    float w = (float)SynWeight[s];
                    I[tgt] += w;
                }
            }
            else
            {
                Spike[i] = false;
            }
        }

        // 2. Обновляем скрытые и моторные нейроны (они уже могут иметь ненулевой I)
        for (int i = NumSensors; i < TotalNeurons; i++)
        {
            I[i] += (float)(rnd.NextDouble() * 0.02 - 0.01);
            float effDecay = Math.Clamp(Decay[i] + brainParams.HormoneEffectOnDecay * hormoneLevel, 0f, 1f);
            float effBias = Bias[i] + brainParams.HormoneEffectOnBias * hormoneLevel;
            V[i] = V[i] * effDecay + effBias + I[i];
            I[i] = 0f; // сброс после использования

            if (V[i] >= Threshold[i])
            {
                Spike[i] = true;
                V[i] = 0f;

                // Рассылаем спайки скрытых/моторных нейронов
                int start = NOutStart[i];
                int end = start + NOutCount[i];
                for (int s = start; s < end; s++)
                {
                    int tgt = SynTarget[s];
                    float w = (float)SynWeight[s];
                    I[tgt] += w;
                }
            }
            else
            {
                Spike[i] = false;
            }
        }

        // 3. Собираем моторные спайки
        bool[] motorSpikes = new bool[NumMotors];
        for (int i = 0; i < NumMotors; i++)
            motorSpikes[i] = Spike[NumSensors + i];

        ApplyPruning();

        return motorSpikes;
    }

    // Добавить один нейрон и вернуть его индекс
    public int AddNeuron(int type, float threshold, float decay, float bias)
    {
        int newIdx = TotalNeurons;
        TotalNeurons++;

        Array.Resize(ref V, TotalNeurons);
        Array.Resize(ref I, TotalNeurons);
        Array.Resize(ref Threshold, TotalNeurons);
        Array.Resize(ref Decay, TotalNeurons);
        Array.Resize(ref Bias, TotalNeurons);
        Array.Resize(ref NeuronType, TotalNeurons);
        Array.Resize(ref Spike, TotalNeurons);
        Array.Resize(ref NOutStart, TotalNeurons);
        Array.Resize(ref NOutCount, TotalNeurons);

        V[newIdx] = 0f;
        I[newIdx] = 0f;
        Threshold[newIdx] = threshold;
        Decay[newIdx] = decay;
        Bias[newIdx] = bias;
        NeuronType[newIdx] = type;
        Spike[newIdx] = false;
        NOutStart[newIdx] = 0; // связи ещё не созданы
        NOutCount[newIdx] = 0;

        return newIdx;
    }

    // Добавить синапс от src к tgt с указанным весом
    public void AddSynapse(int src, int tgt, Half weight)
    {
        // Индекс, куда вставим новую связь — сразу после последней связи src
        int insertPos = NOutStart[src] + NOutCount[src];

        // Увеличиваем размеры массивов синапсов на 1
        TotalSynapses++;
        Array.Resize(ref SynTarget, TotalSynapses);
        Array.Resize(ref SynWeight, TotalSynapses);

        // Сдвигаем всё, что после insertPos, на одну позицию вправо
        for (int i = TotalSynapses - 1; i > insertPos; i--)
        {
            SynTarget[i] = SynTarget[i - 1];
            SynWeight[i] = SynWeight[i - 1];
        }

        // Вставляем новую связь
        SynTarget[insertPos] = tgt;
        SynWeight[insertPos] = weight;

        // Обновляем счётчик исходящих связей src
        NOutCount[src]++;

        // Сдвигаем начала блоков для всех нейронов после src
        for (int i = src + 1; i < TotalNeurons; i++)
        {
            NOutStart[i]++;
        }
    }

    private void ApplyPruning()
    {
        if (!brainParams.PruningEnabled) return;

        for (int i = NumSensors; i < TotalNeurons; i++)
        {
            if (NeuronType[i] != 2) continue;

            if (Spike[i])
            {
                silenceCounter[i] = 0;
                if (rescueMode[i])
                {
                    rescueMode[i] = false;
                    // Возвращаем исходные параметры (можно сохранить и восстановить, пока просто перестаём форсировать)
                }
            }
            else
            {
                silenceCounter[i]++;
            }

            // Проверка на необходимость спасения или удаления
            if (!rescueMode[i] && silenceCounter[i] >= brainParams.PruningSilenceTimeout)
            {
                if (brainParams.PruningRescueWindow > 0)
                {
                    // Включаем режим спасения: временно сильно облегчаем возбуждение
                    rescueMode[i] = true;
                    rescueTimer[i] = brainParams.PruningRescueWindow;
                    Threshold[i] *= 0.5f;
                    Bias[i] += 0.1f;
                }
                else
                {
                    RemoveNeuron(i);
                }
            }
            else if (rescueMode[i])
            {
                rescueTimer[i]--;
                if (rescueTimer[i] <= 0)
                {
                    // Окно спасения истекло, а нейрон так и не застрелял — удаляем
                    RemoveNeuron(i);
                }
            }
        }
    }

    private void ProcessRemovals()
    {
        if (pendingRemoval.Count == 0) return;

        // Множество нейронов, которые выжили
        HashSet<int> removedSet = new HashSet<int>(pendingRemoval);
        List<int> survivors = new List<int>();
        for (int i = 0; i < TotalNeurons; i++)
            if (!removedSet.Contains(i))
                survivors.Add(i);

        int newTotal = survivors.Count;
        if (newTotal == 0) return; // не должно случиться, но защита

        // Пересоздаём нейронные массивы
        float[] newV = new float[newTotal];
        float[] newI = new float[newTotal];
        float[] newThreshold = new float[newTotal];
        float[] newDecay = new float[newTotal];
        float[] newBias = new float[newTotal];
        int[] newType = new int[newTotal];
        bool[] newSpike = new bool[newTotal];
        bool[] newAlive = new bool[newTotal];
        int[] newSilenceCounter = new int[newTotal];
        bool[] newRescueMode = new bool[newTotal];
        int[] newRescueTimer = new int[newTotal];
        float[] newAvgSpike = new float[newTotal];
        // ... и другие массивы, которые есть

        for (int newIdx = 0; newIdx < newTotal; newIdx++)
        {
            int oldIdx = survivors[newIdx];
            newV[newIdx] = V[oldIdx];
            newI[newIdx] = I[oldIdx];
            newThreshold[newIdx] = Threshold[oldIdx];
            newDecay[newIdx] = Decay[oldIdx];
            newBias[newIdx] = Bias[oldIdx];
            newType[newIdx] = NeuronType[oldIdx];
            newSpike[newIdx] = Spike[oldIdx];
            newAlive[newIdx] = alive[oldIdx];
            newSilenceCounter[newIdx] = silenceCounter[oldIdx];
            newRescueMode[newIdx] = rescueMode[oldIdx];
            newRescueTimer[newIdx] = rescueTimer[oldIdx];
            //newAvgSpike[newIdx] = avgSpike[oldIdx];
        }

        // Пересоздаём синапсы
        List<int> newSynTarget = new List<int>();
        List<Half> newSynWeight = new List<Half>();
        int[] newNOutStart = new int[newTotal];
        int[] newNOutCount = new int[newTotal];

        // Строим маппинг старых индексов в новые
        int[] oldToNew = new int[TotalNeurons];
        for (int i = 0; i < TotalNeurons; i++) oldToNew[i] = -1;
        for (int newIdx = 0; newIdx < newTotal; newIdx++)
            oldToNew[survivors[newIdx]] = newIdx;

        for (int newIdx = 0; newIdx < newTotal; newIdx++)
        {
            int oldSrc = survivors[newIdx];
            int start = NOutStart[oldSrc];
            int end = start + NOutCount[oldSrc];
            newNOutStart[newIdx] = newSynTarget.Count;
            for (int s = start; s < end; s++)
            {
                int oldTgt = SynTarget[s];
                if (removedSet.Contains(oldTgt)) continue; // связь в удалённый нейрон — игнорируем
                int newTgt = oldToNew[oldTgt];
                if (newTgt == -1) continue;
                newSynTarget.Add(newTgt);
                newSynWeight.Add(SynWeight[s]);
            }
            newNOutCount[newIdx] = newSynTarget.Count - newNOutStart[newIdx];
        }

        // Заменяем массивы
        V = newV; I = newI; Threshold = newThreshold; Decay = newDecay; Bias = newBias;
        NeuronType = newType; Spike = newSpike; alive = newAlive;
        silenceCounter = newSilenceCounter; rescueMode = newRescueMode; rescueTimer = newRescueTimer;
        //avgSpike = newAvgSpike;
        SynTarget = newSynTarget.ToArray(); SynWeight = newSynWeight.ToArray();
        NOutStart = newNOutStart; NOutCount = newNOutCount;
        TotalNeurons = newTotal;
        TotalSynapses = newSynTarget.Count;

        // Корректируем NumSensors, NumMotors, NumHidden (если удалённый нейрон был сенсором или мотором — это маловероятно, но можно проверить)
        // Пока предполагаем, что удаляются только скрытые нейроны.
        pendingRemoval.Clear();
    }

    public void RemoveNeuron(int index)
    {
        if (alive[index])
        {
            alive[index] = false;
            pendingRemoval.Add(index);
        }
    }
}