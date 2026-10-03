using System;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.AI;

class Program
{
    static void Main()
    {
        foreach(var prop in typeof(ChatResponse).GetProperties()) {
            Console.WriteLine(prop.Name + " " + prop.PropertyType.Name);
        }
    }
}
