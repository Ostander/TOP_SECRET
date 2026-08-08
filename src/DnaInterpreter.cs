using System;

public static class DnaInterpreter
{
    public static BrainParams Decode(Dna dna)
    {
        var p = new BrainParams();
        byte[] d = dna.Data;
        int pos = 0;

        // Вспомогательная функция: читает float из 4 байт, возвращает значение 0..1
        float ReadFloat01()
        {
            if (pos + 3 >= d.Length) return 0.5f;
            int val = d[pos] | (d[pos + 1] << 2) | (d[pos + 2] << 4) | (d[pos + 3] << 6);
            pos += 4;
            return val / 4096f; // диапазон 0..4095 -> 0..1
        }

        // Читает целое число из одного байта
        int ReadByte()
        {
            if (pos >= d.Length) return 0;
            return d[pos++];
        }

        // --- Секция 1: базовые свойства нейронов (12 байт) ---
        p.DefaultBias = ReadFloat01() * 0.2f - 0.1f;           // -0.1 .. 0.1
        p.DefaultDecay = 0.9f + ReadFloat01() * 0.099f;        // 0.9 .. 0.999
        p.DefaultThreshold = 0.5f + ReadFloat01() * 1.5f;      // 0.5 .. 2.0

        // --- Секция 2: рост (4 байта) ---
        p.GrowthTick = ReadByte() * 10;                        // 0..2550 тактов
        p.GrowthNeuronCount = ReadByte() * 2;                  // 0..510 нейронов
        p.GrowthSynapsesPerNeuron = ReadByte();                // 0..255 связей на нейрон

        // --- Секция 3: прунинг (3 байта) ---
        p.PruningEnabled = (ReadByte() > 127);                 // 50% шанс включиться
        p.PruningSilenceTimeout = ReadByte() * 10;             // 0..2550 тактов молчания
        p.PruningRescueWindow = ReadByte();                    // 0..255 тактов на спасение

        p.HormoneBaseline = ReadFloat01();
        p.HormoneDecay = 0.9f + ReadFloat01() * 0.099f;
        p.HormoneReleaseOnError = ReadFloat01() * 0.5f;
        p.HormoneEffectOnBias = ReadFloat01() * 0.2f - 0.1f; // -0.1..0.1
        p.HormoneEffectOnDecay = ReadFloat01() * 0.2f - 0.1f;

        return p;
    }
}

public struct BrainParams
{
    public string Describe()
    {
        return $"Bias={DefaultBias:F3} Decay={DefaultDecay:F3} Thresh={DefaultThreshold:F3} " +
               $"GrowTick={GrowthTick} GrowCount={GrowthNeuronCount} GrowSyn={GrowthSynapsesPerNeuron} " +
               $"Pruning={PruningEnabled} SilenceTimeout={PruningSilenceTimeout} RescueWindow={PruningRescueWindow} " +
               $"HormBase={HormoneBaseline:F2} HormDecay={HormoneDecay:F2} H_ErrRel={HormoneReleaseOnError:F2} " +
               $"H→Bias={HormoneEffectOnBias:F2} H→Decay={HormoneEffectOnDecay:F2}";
    }

    public float DefaultBias;
    public float DefaultDecay;
    public float DefaultThreshold;
    public int GrowthTick;
    public int GrowthNeuronCount;
    public int GrowthSynapsesPerNeuron;
    public bool PruningEnabled;
    public int PruningSilenceTimeout;
    public int PruningRescueWindow;

    public float HormoneBaseline;       // базовый уровень гормона
    public float HormoneDecay;          // скорость распада
    public float HormoneReleaseOnError; // выброс при ошибке предсказания (потом)
    public float HormoneEffectOnBias;   // как сильно гормон меняет Bias
    public float HormoneEffectOnDecay;  // как сильно гормон меняет Decay
}