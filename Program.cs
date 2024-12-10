using System;
using System.Collections.Generic;
using System.Linq;

public class TaskScheduler<TTask, TPriority> where TPriority : IComparable<TPriority>
{
    private readonly SortedDictionary<TPriority, Queue<TTask>> _taskQueue;
    private readonly Func<TTask, TPriority> _prioritySelector;

    public TaskScheduler(Func<TTask, TPriority> prioritySelector)
    {
        _taskQueue = new SortedDictionary<TPriority, Queue<TTask>>();
        _prioritySelector = prioritySelector ?? throw new ArgumentNullException(nameof(prioritySelector));
    }

    public delegate void TaskExecution<TTask>(TTask task);

    public void AddTask(TTask task)
    {
        TPriority priority = _prioritySelector(task);

        if (!_taskQueue.ContainsKey(priority))
        {
            _taskQueue[priority] = new Queue<TTask>();
        }

        _taskQueue[priority].Enqueue(task);
        Console.WriteLine($"Task with priority {priority} added.");
    }

    public void ExecuteNext(TaskExecution<TTask> executeTask)
    {
        if (_taskQueue.Count == 0)
        {
            Console.WriteLine("No tasks available to execute.");
            return;
        }

        // Get the highest priority task (smallest value)
        var highestPriority = _taskQueue.Keys.First();
        var task = _taskQueue[highestPriority].Dequeue();

        // Execute the task using the provided delegate
        executeTask(task);

        // Remove the task from the queue if it is empty
        if (_taskQueue[highestPriority].Count == 0)
        {
            _taskQueue.Remove(highestPriority);
        }

        Console.WriteLine($"Task with priority {highestPriority} executed.");
    }

    public void DisplayTasks()
    {
        Console.WriteLine("Current tasks in the queue:");
        foreach (var kvp in _taskQueue)
        {
            Console.WriteLine($"Priority {kvp.Key}: {kvp.Value.Count} tasks");
        }
    }
}

class Program
{
    static void Main()
    {
        // Create a scheduler with tasks as strings and priorities as integers
        TaskScheduler<string, int> scheduler = new TaskScheduler<string, int>(task =>
        {
            // Priority logic: Assume the task string starts with a number indicating priority
            if (int.TryParse(task.Split(' ')[0], out int priority))
            {
                return priority;
            }
            else
            {
                return int.MaxValue; // Default low priority if parsing fails
            }
        });

        // Define a delegate for executing tasks
        TaskScheduler<string, int>.TaskExecution<string> executeTask = task =>
        {
            Console.WriteLine($"Executing task: {task}");
        };

        // Main loop for user input
        while (true)
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Add task");
            Console.WriteLine("2. Execute next task");
            Console.WriteLine("3. Display tasks");
            Console.WriteLine("4. Exit");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Enter a task with priority (e.g., '1 Task description'):");
                    string newTask = Console.ReadLine();
                    scheduler.AddTask(newTask);
                    break;

                case "2":
                    scheduler.ExecuteNext(executeTask);
                    break;

                case "3":
                    scheduler.DisplayTasks();
                    break;

                case "4":
                    return;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}
