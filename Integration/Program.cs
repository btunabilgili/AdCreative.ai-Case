using Integration.Service;
using System.Diagnostics;

namespace Integration;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        //Both of them will be threadsafe so there wont be any duplicate record.

        //Testing it with lock and its performance will worse than concurrent dictionary because its gonna wait other thread to finish even if it has different itemContent
        var serviceWithLock = new ItemIntegrationServiceWithLock();

        var stopWatch = new Stopwatch();
        var taskList = new List<Task>();

        stopWatch.Start();

        for (int i = 0; i < 2; i++)
        {
            taskList.Add(Task.Run(() => serviceWithLock.SaveItem("a")));
            taskList.Add(Task.Run(() => serviceWithLock.SaveItem("b")));
            taskList.Add(Task.Run(() => serviceWithLock.SaveItem("c")));

            taskList.Add(Task.Run(() => serviceWithLock.SaveItem("a")));
            taskList.Add(Task.Run(() => serviceWithLock.SaveItem("b")));
            taskList.Add(Task.Run(() => serviceWithLock.SaveItem("c")));
        }

        await Task.WhenAll(taskList);

        stopWatch.Stop();

        Console.WriteLine($"serviceWithLock took {stopWatch.ElapsedMilliseconds} milliseconds.");

        Console.WriteLine("Everything recorded(serviceWithLock):");

        serviceWithLock.GetAllItems().ForEach(Console.WriteLine);

        stopWatch.Restart();

        taskList.Clear();

        //It will perform better because its not waiting for different itemContent types so it can save them concurrently.
        var serviceConcurrentDictionary = new ItemIntegrationService();

        stopWatch.Start();

        for (int i = 0; i < 2; i++)
        {
            taskList.Add(Task.Run(() => serviceConcurrentDictionary.SaveItem("a")));
            taskList.Add(Task.Run(() => serviceConcurrentDictionary.SaveItem("b")));
            taskList.Add(Task.Run(() => serviceConcurrentDictionary.SaveItem("c")));

            taskList.Add(Task.Run(() => serviceConcurrentDictionary.SaveItem("a")));
            taskList.Add(Task.Run(() => serviceConcurrentDictionary.SaveItem("b")));
            taskList.Add(Task.Run(() => serviceConcurrentDictionary.SaveItem("c")));
        }

        await Task.WhenAll(taskList);

        stopWatch.Stop();

        Console.WriteLine($"serviceConcurrentDictionary took {stopWatch.ElapsedMilliseconds} milliseconds.");

        Console.WriteLine("Everything recorded (serviceConcurrentDictionary):");

        serviceConcurrentDictionary.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
    }
}