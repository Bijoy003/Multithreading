using System.Diagnostics.Metrics;
using System.Reflection;

namespace Multithreading
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      //RaceConditionExample();
      //DeadlockExample().Wait();
      //AsynchronousExample().Wait();
    }

    public async static Task AsynchronousExample()
    {
      Counter counter = new Counter();
      List<Task<int>> tasks = new List<Task<int>>();

      for (int i = 0; i < 1000; i++)
      {
        tasks.Add(counter.Increment(1));
      }

      int[] results = await Task.WhenAll(tasks);
      Console.WriteLine("Result = {0}", results.Sum());
    }

    public async static Task DeadlockExample()
    {
      var resource1 = new object();
      var resource2 = new object();

      Task task1 = null;
      Task task2 = null;

      task1 = Task.Run(() =>
      {
        lock (resource1)
        {
          Console.WriteLine("Task1 started using resource1");
          Thread.Sleep(100);
          lock (resource2)
          {
            Console.WriteLine("Task1 started using lock2");
          }
        }
      });

      task2 = Task.Run(() =>
      {
        lock (resource2)
        {
          Console.WriteLine("Task2 started using resource2");
          Thread.Sleep(100);
          lock (resource1)
          {
            Console.WriteLine("Task2 started using resource1");
          }
        }
      });

      await Task.WhenAll(task1, task2);
      Console.WriteLine("Completed.");
    }

    public static void RaceConditionExample()
    {
      Counter counter = new Counter();
      Thread thread1 = new Thread(() =>
      {
        for (int i = 0; i < 100000; i++)
        {
          counter.Increment();
          //counter.IncrementWithoutLock();
        }
      });

      Thread thread2 = new Thread(() =>
      {
        for (int i = 0; i < 100000; i++)
        {
          counter.Increment();
          //counter.IncrementWithoutLock();

        }
      });

      thread1.Start();
      thread2.Start();
      thread1.Join();
      thread2.Join();

      Console.WriteLine("Count: {0}", counter.GetCount());
    }
  }
  class Counter
  {
    private int count = 0;
    private object lockObject = new object();

    public async Task<int> Increment(int inc)
    {
      lock (lockObject)
      {
        count += inc;
      }
      await Task.Delay(10);
      return 1;
    }

    public void Increment()
    {
      lock (lockObject)
      {
        count++;
      }
    }

    public void IncrementWithoutLock()
    {
      count++;
    }

    public int GetCount()
    {
      lock (lockObject)
      {
        //Thread.Sleep(2000);
        return count;
      }
    }
  }
}
