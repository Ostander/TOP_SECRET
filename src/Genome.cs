using System;
using System.Collections.Generic;

// Ген — одно правило развития
public class Gene
{
    public enum ConditionType { Time, HighError } // можно расширять
    public enum ActionType { AddNeurons }

    public ConditionType Condition;
    public int TriggerTick;    // для Time
    public float ErrorThreshold; // для HighError
    public int ErrorDuration;  // сколько тактов держится ошибка

    public float DefaultBias;
    public float DefaultDecay;
    public float DefaultThreshold;

    public ActionType Action;
    public int NeuronCount;
    public int SynapsesPerNeuron;
    public float Decay;
    public float Bias;
    public float Threshold;

    // Простой конструктор для первого гена
    public static Gene TimeBased(int tick, int neuronCount, int synPerNeuron = 50)
    {
        return new Gene
        {
            Condition = ConditionType.Time,
            TriggerTick = tick,
            Action = ActionType.AddNeurons,
            NeuronCount = neuronCount,
            SynapsesPerNeuron = synPerNeuron,
            DefaultDecay = 0.99f,
            DefaultBias = 0.02f,    // небольшой толчок, чтобы сеть не засыпала
            DefaultThreshold = 1.0f
        };
    }
}

// Геном — список генов
public class Genome
{
    public List<Gene> Genes = new List<Gene>();

    public Genome() { }

    // Создаём стартовый геном с одним геном: через 200 тактов добавить 20 скрытых нейронов
    public static Genome CreateDefault()
    {
        var g = new Genome();
        g.Genes.Add(Gene.TimeBased(200, 20, 50));
        return g;
    }
}