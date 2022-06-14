PeriodicTask t1 = new PeriodicTask("t1", 2, 2);
PeriodicTask t2 = new PeriodicTask("t2", 5, 1);

Schedule schedule = new Schedule(new PeriodicTask[] { t1, t2 }.ToList());
schedule.LLF();

class PeriodicTask
{
    public static PeriodicTask NaN = new PeriodicTask("NaN", 0, 0);
    public int Period => _period;
    public int ExecutionTime => _executionTime;
    public int RemainingTime => _remainingTime;
    public bool isCompleted { get; set; }
    public int Time => _time;
    public int Cost => _cost;
    public int Priority { get; set; }
    public int TimeToExpiry => _period - _internalTime;
    public string Name { get; set; }

    private int _period;
    private int _executionTime;
    private int _remainingTime;
    private int _internalTime;
    private int _time;
    private int _cost;

    public PeriodicTask(string Name, int Period, int ExecutionTime)
    {
        _period = Period;
        _executionTime = ExecutionTime;
        _remainingTime = ExecutionTime;
        _time = 0;
        _internalTime = 0;
        isCompleted = false;
        this.Name = Name;
        _cost = 0;
    }

    public void Tick()
    {
        _time++;
        _internalTime++;
        if(_internalTime == _period)
        {
            _internalTime = 0;
            _cost += RemainingTime;
            _remainingTime = _executionTime;
            isCompleted = false;
        }
    }

    public void Execute()
    {

        if (isCompleted == false)
        {
            _remainingTime--;
            if (_remainingTime == 0)
            {
                isCompleted = true;
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
        _cost = 0;
    }
}

class TaskSlice
{
    public PeriodicTask Task;
    public string Name;
    public int Cost;
    public TaskSlice(PeriodicTask task)
    {
        Task = task;
        Name = task.Name;
        Cost = task.Cost; 
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
                lcm_of_array_elements = lcm_of_array_elements * divisor;
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

    public void EDF()
    {
        long length = lcm_of_array_elements(_tasks.Select(x => x.Period).ToArray());
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
            Console.WriteLine(i + " " + task.Name);
            TaskSchedule.Add(new TaskSlice(task));
            _tasks.ForEach(x => x.Tick());
        }
        _tasks.ForEach(x => Console.WriteLine($"Task: {x.Name}, Cost: {x.Cost}"));
    }

    public void LLF()
    {
        long length = lcm_of_array_elements(_tasks.Select(x => x.Period).ToArray());
        for (long i = 0; i < length; i++)
        {
            _tasks.ForEach(x => x.Priority = x.TimeToExpiry - x.RemainingTime);
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
            Console.WriteLine(i + " " + task.Name);
            TaskSchedule.Add(new TaskSlice(task));
            _tasks.ForEach(x => x.Tick());
        }
        _tasks.ForEach(x => Console.WriteLine($"Task: {x.Name}, Cost: {x.Cost}"));
    }

    public void RMS()
    {
        long length = lcm_of_array_elements(_tasks.Select(x => x.Period).ToArray());
        _tasks.ForEach(x => x.Priority = x.ExecutionTime);
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
            Console.WriteLine(i + " " + task.Name);
            TaskSchedule.Add(new TaskSlice(task));
            _tasks.ForEach(x => x.Tick());
        }

        _tasks.ForEach(x => Console.WriteLine($"Task: {x.Name}, Cost: {x.Cost}"));
    }

    public void ClearSchedule()
    {
        TaskSchedule.Clear();
        _tasks.ForEach(x => x.ResetTask());
    }
}
