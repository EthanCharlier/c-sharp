using System.Diagnostics;

int iterations = 50_000_000;

// ============================================
// VERSION SÉQUENTIELLE
// ============================================
var swSeq = Stopwatch.StartNew();
double sumSeq = 0;

for (int i = 0; i < iterations; i++)
{
    // Cosinus et sinus
    sumSeq += Math.Sin(i) + Math.Cos(i);
    // Racine carrée
    sumSeq += Math.Sqrt(i);
    // Exponentielle et logarithme
    sumSeq += Math.Exp(i) + Math.Log(i + 1);
    // Puissances
    sumSeq += Math.Pow(i % 100, 3);
    // Multiplication
    sumSeq *= 1.000001;
}

swSeq.Stop();

Console.WriteLine("========================================");
Console.WriteLine("VERSION SÉQUENTIELLE");
Console.WriteLine("========================================");
Console.WriteLine($"Temps de calcul : {swSeq.ElapsedMilliseconds} ms");
Console.WriteLine();

// ============================================
// VERSION PARALLÈLE
// ============================================
var swPar = Stopwatch.StartNew();
double sumPar = 0;
object lockObj = new object();

Parallel.For(0, iterations,
    () => 0.0, // Initialisation thread-local
    (i, state, localSum) =>
    {
        // Cosinus et sinus
        localSum += Math.Sin(i) + Math.Cos(i);
        // Racine carrée
        localSum += Math.Sqrt(i);
        // Exponentielle et logarithme
        localSum += Math.Exp(i) + Math.Log(i + 1);
        // Puissances
        localSum += Math.Pow(i % 100, 3);
        // Multiplication
        localSum *= 1.000001;

        return localSum;
    },
    localSum => // Agrégation finale
    {
        lock (lockObj)
        {
            sumPar += localSum;
        }
    }
);

swPar.Stop();

Console.WriteLine("========================================");
Console.WriteLine("VERSION PARALLÈLE");
Console.WriteLine("========================================");
Console.WriteLine($"Temps de calcul : {swPar.ElapsedMilliseconds} ms");
Console.WriteLine();

// ============================================
// COMPARAISON
// ============================================
Console.WriteLine("========================================");
Console.WriteLine("COMPARAISON");
Console.WriteLine("========================================");
Console.WriteLine($"Accélération (speedup) : x{(double)swSeq.ElapsedMilliseconds / swPar.ElapsedMilliseconds:F2}");
Console.WriteLine($"Gain de temps          : {swSeq.ElapsedMilliseconds - swPar.ElapsedMilliseconds} ms");
Console.WriteLine($"Nombre de processeurs  : {Environment.ProcessorCount}");
