class Project
{
    static void Main()
    {
        // --- Параметры сети ---
        int N = 3;                       // всего нейронов
        float[] V = new float[N];        // потенциалы
        float[] I_ext = new float[N];    // накопленный ток от спайков
        float[] threshold = { 1f, 1f, 1f };
        float[] decay = { 1f, 0.9f, 0.9f };
        bool[] spike = new bool[N];

        // --- Синапсы (всего 2 связи) ---
        int[] synTarget = new int[2];    // цель каждой связи
        float[] synWeight = new float[2]; // её вес
                                          // Для каждого нейрона: начало и количество исходящих связей
        int[] nOutStart = new int[N];
        int[] nOutCount = new int[N];

        // Инициализация связей:
        // Нейрон 0 → Нейрон 1 (вес 0.7)
        synTarget[0] = 1; synWeight[0] = 0.7f;
        nOutStart[0] = 0; nOutCount[0] = 1;
        // Нейрон 1 → Нейрон 2 (вес 1.2)
        synTarget[1] = 2; synWeight[1] = 1.2f;
        nOutStart[1] = 1; nOutCount[1] = 1;
        // Нейрон 2 не имеет исходящих связей
        nOutStart[2] = 2; nOutCount[2] = 0; // начало за границей массива, количество 0

        // Моделируем 10 шагов
        for (int step = 0; step < 10; step++)
        {
            // 1. Вручную задаём спайк нейрону 0 на шаге 2
            if (step == 2)
            {
                spike[0] = true;   // мы как будто "вкололи" спайк сенсору
                V[0] = 0f;         // сброс не важен, т.к. мы не считаем его потенциал
            }
            else
                spike[0] = false;

            // 2. Обновляем нейроны 1 и 2 (нейрон 0 мы не обновляем — он управляется вручную)
            for (int i = 1; i < N; i++)
            {
                // Сначала копим ток, который пришёл от спайков на предыдущем шаге (он уже лежит в I_ext[i])
                V[i] = V[i] * decay[i] + I_ext[i];
                I_ext[i] = 0f; // сбрасываем накопленный ток

                if (V[i] >= threshold[i])
                {
                    spike[i] = true;
                    V[i] = 0f;
                }
                else
                    spike[i] = false;
            }

            // 3. Рассылаем спайки по связям
            for (int src = 0; src < N; src++)
            {
                if (spike[src])
                {
                    int start = nOutStart[src];
                    int end = start + nOutCount[src];
                    for (int s = start; s < end; s++)
                    {
                        int tgt = synTarget[s];
                        float w = synWeight[s];
                        I_ext[tgt] += w; // добавляем вес к входному току цели
                    }
                }
            }

            // 4. Выводим состояние
            Console.WriteLine($"Шаг {step}: спайки: {spike[0]},{spike[1]},{spike[2]}, V1={V[1]:F2}, V2={V[2]:F2}");
        }
    }
}