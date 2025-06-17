using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// Records run-time statistics for the various path-finding algorithms and writes them both to the console
/// (UnityEngine.Debug.Log) and to a persistent CSV file so the data can later be analysed (e.g. in a spreadsheet).
/// </summary>
public static class AlgorithmAnalytics
{
    private const string CsvHeader = "Timestamp,Algorithm,Path Length,Computation Time (ms),Nodes Explored,Iterations/Generations,Reached Target";
    private static readonly string CsvPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "algorithm_analytics.csv");

    /// <summary>
    /// Writes the <paramref name="result"/> to the Unity console as well as to a CSV file located in
    /// <see cref="UnityEngine.Application.persistentDataPath"/>. The CSV file is created on first use with a header row.
    /// </summary>
    public static void Log(ResultData result)
    {
        // 1) Console output (nice formatted string)
        UnityEngine.Debug.Log($"[Analytics] {result.Algorithm} | Path Length: {result.PathLength} | Time: {result.TimeMs} ms | Nodes Explored: {result.NodesExplored} | Iterations: {result.Iterations} | Reached: {result.ReachedTarget}");

        // 2) CSV output – ensure file exists with header
        try
        {
            if (!File.Exists(CsvPath))
            {
                File.WriteAllText(CsvPath, CsvHeader + Environment.NewLine);
            }

            string csvLine = $"{result.Algorithm},{result.PathLength},{result.TimeMs},{result.NodesExplored},{result.Iterations}";
            File.AppendAllText(CsvPath, csvLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[Analytics] Failed to write analytics CSV: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Convenience factory for creating the immutable analytics payload.
    /// </summary>
    public readonly struct ResultData
    {
        public readonly string Algorithm;
        public readonly int PathLength;
        public readonly long TimeMs;
        public readonly int NodesExplored;
        public readonly int Iterations;
        public readonly bool ReachedTarget;

        public ResultData(string algorithm, int pathLength, long timeMs, int nodesExplored, int iterations, bool reachedTarget)
        {
            Algorithm = algorithm;
            PathLength = pathLength;
            TimeMs = timeMs;
            NodesExplored = nodesExplored;
            Iterations = iterations;
            ReachedTarget = reachedTarget;
        }
    }
} 