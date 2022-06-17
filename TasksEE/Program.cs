using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

Random r = new Random();
var taskList = new List<PeriodicTask>();
taskList.Clear();
for (int i = 0; i < 4; i++)
{
    taskList.Add(new PeriodicTask($"T{i}", 5+i, 2));
}

Schedule schedule = new Schedule(taskList);

schedule.EDF(24);
schedule.ClearSchedule();

schedule.LLF(24);
schedule.ClearSchedule();

schedule.RMS(24);
schedule.ClearSchedule();

class PeriodicTask
{
    public static PeriodicTask NaN = new PeriodicTask("NaN", 0, 0);
    public int Period => _period;
    public int ExecutionTime => _executionTime;
    public int RemainingTime => _remainingTime;
    public bool isCompleted { get; set; }
    public int Time => _time;
    public int Priority { get; set; }
    public int TimeToExpiry
    {
        get
        {
            int timeToExpiry = 0;
            if(isCompleted == true)
            {
                timeToExpiry = int.MaxValue/2;
                //timeToExpiry = _period - _deadlineOsc;
            }
            else
            {
                timeToExpiry = _period - _deadlineOsc;
            }
            return timeToExpiry;
        }
    }
    public string Name { get; set; }
    public int TimesExecutedTotal { get; set; }

    private int _period;
    private int _executionTime;
    private int _remainingTime;
    private int _internalTime;
    private int _time;
    private int _responseTimeOsc;
    private int _deadlineOsc;

    public PeriodicTask(string Name, int Period, int ExecutionTime)
    {
        _period = Period;
        _executionTime = ExecutionTime;
        _remainingTime = ExecutionTime;
        _time = 0;
        _internalTime = 0;
        isCompleted = false;
        this.Name = Name;
        _responseTimeOsc = 0;
    }

    public void Tick()
    {
        _time++;
        _internalTime++;
        _responseTimeOsc++;
        _deadlineOsc++;

        if (_internalTime == _period)
        {
            if (isCompleted == true)
            {
                _deadlineOsc = 0;
                _remainingTime = _executionTime;
            }

            _internalTime = 0;
            isCompleted = false;
        }
    }

    public void Execute()
    {
        TimesExecutedTotal++;
        if (isCompleted == false)
        {
            _remainingTime--;
            if (_remainingTime == 0)
            {
                isCompleted = true;
                _responseTimeOsc = 0;
                _deadlineOsc = int.MaxValue;
            }
            return;
        }
        throw new InvalidOperationException("The task is already completed.");

    }

    public void ResetTask()
    {
        _executionTime = ExecutionTime;
        _remainingTime = ExecutionTime;
        _time = 0;
        _internalTime = 0;
        isCompleted = false;
        _deadlineOsc = 0;
        _responseTimeOsc = 0;
        TimesExecutedTotal = 0;
    }
}

class TaskSlice
{
    public PeriodicTask Task;
    public string Name;
    public TaskSlice(PeriodicTask task)
    {
        Task = task;
        Name = task.Name;
    }
}

class Schedule
{
    public static long lcm_of_array_elements(int[] element_array)
    {
        long lcm_of_array_elements = 1;
        int divisor = 2;

        while (true)
        {

            int counter = 0;
            bool divisible = false;
            for (int i = 0; i < element_array.Length; i++)
            {

                // lcm_of_array_elements (n1, n2, ... 0) = 0.
                // For negative number we convert into
                // positive and calculate lcm_of_array_elements.
                if (element_array[i] == 0)
                {
                    return 0;
                }
                else if (element_array[i] < 0)
                {
                    element_array[i] = element_array[i] * (-1);
                }
                if (element_array[i] == 1)
                {
                    counter++;
                }

                // Divide element_array by devisor if complete
                // division i.e. without remainder then replace
                // number with quotient; used for find next factor
                if (element_array[i] % divisor == 0)
                {
                    divisible = true;
                    element_array[i] = element_array[i] / divisor;
                }
            }

            // If divisor able to completely divide any number
            // from array multiply with lcm_of_array_elements
            // and store into lcm_of_array_elements and continue
            // to same divisor for next factor finding.
            // else increment divisor
            if (divisible)
            {
                lcm_of_array_elements *= divisor;
            }
            else
            {
                divisor++;
            }

            // Check if all element_array is 1 indicate
            // we found all factors and terminate while loop.
            if (counter == element_array.Length)
            {
                return lcm_of_array_elements;
            }
        }
    }

    public List<TaskSlice> TaskSchedule;
    List<PeriodicTask> _tasks;
    public Schedule(List<PeriodicTask> tasks)
    {
        _tasks = tasks;
        TaskSchedule = new List<TaskSlice>();
    }

    public void EDF(int scheduleLength = 0)
    {
        long length = scheduleLength == 0?lcm_of_array_elements(_tasks.Select(x => x.Period).ToArray()):scheduleLength;
        
        File.Delete("./EDF.txt");
        File.AppendAllText("./EDF.txt", $"Tasks:\n");
        
        _tasks.ForEach(x => File.AppendAllText("./EDF.txt", $"Task: {x.Name}\nPeriod: {x.Period}\nExecutionTime: {x.ExecutionTime}\n-------------------------\n"));
        
        File.AppendAllText("./EDF.txt", $"Schedule:\n");
        
        for (long i = 0; i < length; i++)
        {
            _tasks.ForEach(x => x.Priority = x.TimeToExpiry);
            _tasks = _tasks.OrderBy(x => x.Priority).ToList();
            
            PeriodicTask task;
            try
            {
                task = _tasks.First(x => !x.isCompleted);
                task.Execute();
            }
            catch (InvalidOperationException ex)
            {
                task = PeriodicTask.NaN;
            }
            
            File.AppendAllText("./EDF.txt", $"{i} {task.Name} {task.RemainingTime}\n");
            
            TaskSchedule.Add(new TaskSlice(task));
            
            _tasks.ForEach(x => x.Tick());
        }

        File.AppendAllText("./EDF.txt", $"Results:\n");

        _tasks.ForEach(x => File.AppendAllText("./EDF.txt", $"Task: {x.Name}\n"));
    }

    public void LLF(int scheduleLength = 0)
    {
        long length = scheduleLength == 0 ? lcm_of_array_elements(_tasks.Select(x => x.Period).ToArray()) : scheduleLength;
        
        File.Delete("./LLF.txt");
        File.AppendAllText("./LLF.txt", $"Tasks:\n");
        
        _tasks.ForEach(x => File.AppendAllText("./LLF.txt", $"Task: {x.Name}\nPeriod: {x.Period}\nExecutionTime: {x.ExecutionTime}\n-------------------------\n"));
        
        File.AppendAllText("./LLF.txt", $"Schedule:\n");
        
        for (long i = 0; i < length; i++)
        {
            _tasks.ForEach(x => x.Priority = x.TimeToExpiry - x.RemainingTime);
            _tasks = _tasks.OrderBy(x => x.Priority).ToList();
            
            PeriodicTask task;
            try
            {
                task = _tasks.First(x => !x.isCompleted && x.Priority >= 0);
                task.Execute();
            }
            catch (InvalidOperationException ex)
            {
                task = PeriodicTask.NaN;
            }
            
            File.AppendAllText("./LLF.txt", $"{i} {task.Name} {task.RemainingTime}\n");
            
            TaskSchedule.Add(new TaskSlice(task));
            
            _tasks.ForEach(x => 
            { 
                if (x.Priority < 0) x.isCompleted = true; 
                x.Tick(); 
            });
        }
        
        File.AppendAllText("./LLF.txt", $"Results:\n");

        _tasks.ForEach(x => File.AppendAllText("./LLF.txt", $"Task: {x.Name}\n"));
    }

    public void RMS(int scheduleLength = 0)
    {
        long length = scheduleLength == 0 ? lcm_of_array_elements(_tasks.Select(x => x.Period).ToArray()) : scheduleLength;
        
        File.Delete("./RMS.txt");
        File.AppendAllText("./RMS.txt", $"Tasks:\n");
        
        _tasks.ForEach(x => File.AppendAllText("./RMS.txt", $"Task: {x.Name}\nPeriod: {x.Period}\nExecutionTime: {x.ExecutionTime}\n-------------------------\n"));
        
        File.AppendAllText("./RMS.txt", $"Schedule:\n");
        
        _tasks.ForEach(x => x.Priority = x.Period);
        _tasks = _tasks.OrderBy(x => x.Priority).ToList();
        
        for (long i = 0; i < length; i++)
        {
            PeriodicTask task;
            try
            {
                task = _tasks.First(x => !x.isCompleted);
                task.Execute();
            }
            catch (InvalidOperationException ex)
            {
                task = PeriodicTask.NaN;
            }
            
            File.AppendAllText("./RMS.txt", $"{i} {task.Name} {task.RemainingTime}\n");
            
            TaskSchedule.Add(new TaskSlice(task));
            
            _tasks.ForEach(x => x.Tick());
        }

        File.AppendAllText("./RMS.txt", $"Results:\n");
        
        _tasks.ForEach(x => File.AppendAllText("./RMS.txt", $"Task: {x.Name}\n"));
    }

    public void ClearSchedule()
    {
        TaskSchedule.Clear();
        _tasks.ForEach(x => x.ResetTask());
    }
}
