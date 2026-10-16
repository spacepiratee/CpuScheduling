using System;
using System.Collections.Generic;
using System.Linq;

public class Process
{
    public string Id { get; }
    public int Arrival { get; }
    public int Burst { get; }
    public int Remaining { get; set; }
    public int Completion { get; set; }
    public int TurnaroundTime => Completion - Arrival; // = Total duration --- //
    public int WaitingTime => TurnaroundTime - Burst;  // = Delay duration --- //
    
    public Process(string id, int arrival, int burst)
    {
        Id = id; 
        Arrival = arrival;
        Burst = burst; 
        Remaining = burst;
    }
}

class Program
{
    // --- Burst = don vi thoi gian (ms) --- //
    // --- Arrival = thu tu den --- //
    static List<Process> Sample() => new()
    {
        new("P1", 0, 5), 
        new("P2", 1, 3), 
        new("P3", 2, 8),
        new("P4", 3, 6),
        
    };

    // ---------- SJF (non-preemptive) ----------
    static void SJF(List<Process> ps)
    {
        var gantt = new List<(string id, int s, int e)>();
        int time = 0, done = 0;
        var finished = new HashSet<Process>();

        while (done < ps.Count)
        {
            var ready = ps.Where(p => !finished.Contains(p) && p.Arrival <= time).OrderBy(p => p.Burst).ThenBy(p => p.Arrival).ToList();
            if (ready.Count == 0)
            {
                time = ps.Where(p => !finished.Contains(p)).Min(p => p.Arrival);
                continue;
            }
            var cur = ready[0];
            gantt.Add((cur.Id, time, time + cur.Burst));
            time += cur.Burst;
            cur.Completion = time;
            finished.Add(cur);
            done++;
        }
        PrintResult("SJF", ps, gantt);
    }

    static void RoundRobin(List<Process> processes, int quantum)
    {
        var gantt = new List<(string id, int s, int e)>();
        var sorted = processes.OrderBy(p => p.Arrival).ToList();
        var queue = new Queue<Process>();
        int time = 0;
        int index = 0;
        int finishedProcessCount = 0;

        void Enqueue()
        {
            // kiem tra xem con tien trinh nao chua duoc dua vao hang k
            // kiem tra xem tien trinh ke tiep da den chua
            // thoa man => dua tien trinh do vao hang => tang index len 
            while (index < sorted.Count && sorted[index].Arrival <= time)
            {
                // queue.Enqueue(sorted[index++]);
                queue.Enqueue(sorted[index]);
                index += 1;
            }
        }

        Enqueue();
        while (finishedProcessCount < processes.Count) // 1. lap den khi so tien trinh da xong
        {
            if (queue.Count == 0) // kiem tra xem hang doi co rong k, neu rong ma tien trinh moi chua den => bao loi, can day tien trinh chua den vao hang doi luon
            {
                time = sorted[index].Arrival;
                Enqueue();
                continue;
            }
            
            var currentProcess = queue.Dequeue(); // --- 2. Lay tien trinh dau tien trong hang doi
            int runDuration = Math.Min(quantum, currentProcess.Remaining); // 3. tinh thoi gian chay
             
            gantt.Add((currentProcess.Id, time, time + runDuration));

            time += runDuration;
            currentProcess.Remaining -= runDuration;
            
            Enqueue(); // tiến trình mới đến xếp trước
            
            if (currentProcess.Remaining > 0) // --- neu con tien trinh co Remaining > 0 tuc chua xong
            {
                queue.Enqueue(currentProcess);
            }
            else
            {
                currentProcess.Completion = time; 
                finishedProcessCount++;
            }
            PrintEach(currentProcess, currentProcess.Id, runDuration, time, quantum);
        }
        PrintResult($"Round Robin (quantum = {quantum})", processes, gantt);
    }
    
    static void PrintResult(string name, List<Process> ps, List<(string id, int s, int e)> g)
    {
        Console.WriteLine($"\n===== {name} =====");
        Console.WriteLine("Gantt: " + string.Join(" → ", g.Select(x => $"{x.id}({x.s}-{x.e})")));
        Console.WriteLine("\nPID\tArr\tBurst\tComp\tTAT\tWT");
        foreach (var p in ps.OrderBy(p => p.Id))
        {
            Console.WriteLine($"{p.Id}\t{p.Arrival}\t{p.Burst}\t{p.Completion}\t{p.TurnaroundTime}\t{p.WaitingTime}");
        }
        Console.WriteLine($"\nTrung binh Turnaround Time = {ps.Average(p => p.TurnaroundTime):F2}");
        Console.WriteLine($"Trung binh Wait Time  = {ps.Average(p => p.WaitingTime):F2}");
    }

    static void PrintEach(Process process, string id, int runDuration, int time, int quantum)
    {
        bool notDone = process.Remaining > 0;
        var notDoneString = notDone ? "Chua xong," : "Da xong,";
        var notDoneTimeString = notDone ? $"Con: {process.Remaining}," : "";
        var totalTimeString = $"Tong thoi gian da chay {runDuration} + {time - runDuration} = {time}";
        
        Console.WriteLine($"{id}: Burst: {process.Burst} => {process.Burst} - {quantum} = {process.Burst - quantum} => {notDoneString} {notDoneTimeString} {totalTimeString}");
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        RoundRobin(Sample(), 4);
    }
}