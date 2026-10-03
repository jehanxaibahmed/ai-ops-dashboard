using System;
using System.Reflection;
using Microsoft.Extensions.AI;

class Program
{
    static void Main()
    {
        foreach (var method in typeof(IChatClient).GetMethods())
        {
            Console.WriteLine(method.Name);
        }
    }
}
