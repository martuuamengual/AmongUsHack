using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectX_Renderer.Threads
{
    class ThreadException
    {
        public static void Task_UnhandledException(Task task)
        {
            var exception = task.Exception;
            Console.WriteLine(exception);
        }
    }
}
