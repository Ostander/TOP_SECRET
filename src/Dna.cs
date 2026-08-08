using System;

public class Dna
{
    public byte[] Data;

    // Создать случайную ДНК заданной длины
    public Dna(int length)
    {
        Data = new byte[length];
        Random rnd = new Random();
        for (int i = 0; i < length; i++)
            Data[i] = (byte)rnd.Next(4);
    }

    // Создать ДНК из готового массива
    public Dna(byte[] data)
    {
        Data = (byte[])data.Clone();
    }

    // Мутация с заданной вероятностью на нуклеотид
    public void Mutate(double rate = 0.01)
    {
        Random rnd = new Random();
        for (int i = 0; i < Data.Length; i++)
            if (rnd.NextDouble() < rate)
                Data[i] = (byte)rnd.Next(4);
    }

    // Кроссинговер двух ДНК
    public static Dna Crossover(Dna a, Dna b)
    {
        int len = Math.Min(a.Data.Length, b.Data.Length);
        byte[] child = new byte[len];
        Random rnd = new Random();
        for (int i = 0; i < len; i++)
            child[i] = rnd.Next(2) == 0 ? a.Data[i] : b.Data[i];
        return new Dna(child);
    }

    // Буквы нуклеотидов
    private static readonly char[] NucChars = { 'A', 'T', 'G', 'C' };

    // Таблица кодонов ДНК (T, а не U) → аминокислота (однобуквенный код)
    private static readonly Dictionary<string, char> CodonTable = new Dictionary<string, char>
    {
        // T-старт
        {"TTT", 'F'}, {"TTC", 'F'}, {"TTA", 'L'}, {"TTG", 'L'},
        {"TCT", 'S'}, {"TCC", 'S'}, {"TCA", 'S'}, {"TCG", 'S'},
        {"TAT", 'Y'}, {"TAC", 'Y'}, {"TAA", '*'}, {"TAG", '*'},
        {"TGT", 'C'}, {"TGC", 'C'}, {"TGA", '*'}, {"TGG", 'W'},
        // C-старт
        {"CTT", 'L'}, {"CTC", 'L'}, {"CTA", 'L'}, {"CTG", 'L'},
        {"CCT", 'P'}, {"CCC", 'P'}, {"CCA", 'P'}, {"CCG", 'P'},
        {"CAT", 'H'}, {"CAC", 'H'}, {"CAA", 'Q'}, {"CAG", 'Q'},
        {"CGT", 'R'}, {"CGC", 'R'}, {"CGA", 'R'}, {"CGG", 'R'},
        // A-старт
        {"ATT", 'I'}, {"ATC", 'I'}, {"ATA", 'I'}, {"ATG", 'M'}, // ATG = старт
        {"ACT", 'T'}, {"ACC", 'T'}, {"ACA", 'T'}, {"ACG", 'T'},
        {"AAT", 'N'}, {"AAC", 'N'}, {"AAA", 'K'}, {"AAG", 'K'},
        {"AGT", 'S'}, {"AGC", 'S'}, {"AGA", 'R'}, {"AGG", 'R'},
        // G-старт
        {"GTT", 'V'}, {"GTC", 'V'}, {"GTA", 'V'}, {"GTG", 'V'},
        {"GCT", 'A'}, {"GCC", 'A'}, {"GCA", 'A'}, {"GCG", 'A'},
        {"GAT", 'D'}, {"GAC", 'D'}, {"GAA", 'E'}, {"GAG", 'E'},
        {"GGT", 'G'}, {"GGC", 'G'}, {"GGA", 'G'}, {"GGG", 'G'}
    };

    // Вернуть строку нуклеотидов (все или первые N)
    public string ToNucleotideString(int maxLen = 60)
    {
        int len = Math.Min(Data.Length, maxLen);
        char[] chars = new char[len];
        for (int i = 0; i < len; i++)
            chars[i] = NucChars[Data[i]];
        string result = new string(chars);
        if (Data.Length > maxLen) result += "...";
        return result;
    }

    // Вернуть белковую последовательность (трансляция по 3 нуклеотида)
    public string ToAminoAcidString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i + 2 < Data.Length; i += 3)
        {
            string codon = $"{NucChars[Data[i]]}{NucChars[Data[i + 1]]}{NucChars[Data[i + 2]]}";
            if (CodonTable.TryGetValue(codon, out char aa))
                sb.Append(aa);
            else
                sb.Append('?');
        }
        return sb.ToString();
    }
}