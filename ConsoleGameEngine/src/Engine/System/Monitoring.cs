using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleGameEngine.Engine.System;

public class Monitoring(int samplingRate)
{
    private DateTime _startTime;
    private TimeSpan _startCpu;
    private DateTime _endTime;
    private TimeSpan _endCpu;
    private readonly Queue<double> _cpuSamples = new();
    private readonly int _samplingRate = samplingRate;
    private readonly Process _process = Process.GetCurrentProcess();

    public void StartTimer()
    {
        _startTime = DateTime.UtcNow;
        _startCpu = _process.TotalProcessorTime;
    }

    public void StopTimer()
    {
        _endTime = DateTime.UtcNow;
        _endCpu = _process.TotalProcessorTime;
    }

    // percent
    public double GetCpuUsage()
    {
        double elapsedTime = (_endTime - _startTime).TotalMilliseconds;
        double elapsedUsage = (_endCpu - _startCpu).TotalMilliseconds;
        return elapsedUsage / (Environment.ProcessorCount * elapsedTime) * 100;
    }

    // in MB
    public double GetPrivateMemoryUsage()
    {
        _process.Refresh();
        return (double)_process.PrivateMemorySize64 / 1_048_576;
        ;
    }

    // in MB
    public double GetWorkingSet()
    {
        _process.Refresh();
        return (double)_process.WorkingSet64 / 1_048_576; // MB (1024*1024);
    }

    public double GetAverageCpuUsage()
    {
        double current = GetCpuUsage();
        _cpuSamples.Enqueue(current);

        if (_cpuSamples.Count > samplingRate)
        {
            _cpuSamples.Dequeue();
        }

        return _cpuSamples.Average();
    }
}