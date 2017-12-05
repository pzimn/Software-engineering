using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IO_lab2
{
    class Program
    {

        delegate int DelegateType(int n);
        static DelegateType fibIter;
        static DelegateType fibRec;
        static DelegateType silniaIter;
        static DelegateType silniaRec;


        static void Main(string[] args)
        {
         
        }

        static void readFromFile(String path, FileMode fm, IAsyncResult asyncResult)
        {

            byte[] byteData = null;

            FileStream fs = new FileStream(path, FileMode.Open);
            fs.BeginRead(byteData, 0, 1024, myAsyncCallback, new Tuple<byte[], FileStream>(byteData, fs));
            Thread.Sleep(2000);

        }

        static void myAsyncCallback(IAsyncResult state)
        {
            var data = state as Tuple<byte[], FileStream>;
            Console.WriteLine(Encoding.ASCII.GetString(data.Item1));
            data.Item2.Close();
        }

        static void readFromFileEndInvoke(String path, FileMode fm)
        {
           
            byte[] byteData = null;
            FileStream fs = new FileStream(path, FileMode.Open);
            var res = fs.BeginRead(byteData, 0, 1024, null, new Tuple<byte[], FileStream>(byteData, fs));
            fs.EndRead(res);
            Thread.Sleep(2000);

        }

        static int fibIteration(int n)
        { 

            if(n <= 1){ return n; }
            int fib = 1, prevFib = 1;
            int sum = 0;
            for(int i=0; i < n; i++)
            {
                int tmp = fib;
                fib += prevFib;
                prevFib = fib;
            }
            return fib;
        }

        static int fibRecursion(int n)
        {
            if(n <= 1)
            {
                return n;
            }
            return (fibRec(n - 1) + fibRec(n - 2));
        }

        static int silniaIteration(int n)
        {
            if (n == 0) return 0;
            int j = 1;
            while(n > 0)
            {
                j *= n;
                n--;
            }
            return j;
        }

        static int silniaRecursion(int n)
        {
            if (n <1) return 1;
            return n * silniaRec(n - 1);
        }

        static void ThreadProc(IAsyncResult stateInfo)
        {
            DelegateType del = ((object[])(stateInfo.AsyncState))[0] as DelegateType;
            AutoResetEvent resetEvent = ((object[]) (stateInfo.AsyncState))[2] as AutoResetEvent;
            resetEvent.Set();
        }

        static void zad_8()
        {
            AutoResetEvent[] resetEvents = new AutoResetEvent[4];
            for(int i = 0; i < 4; i++)
            {
                resetEvents[i] = new AutoResetEvent(false);
            }

            fibIter = fibIteration;
            fibRec = fibRecursion;
            silniaIter = silniaIteration;
            silniaRec = silniaRecursion;

            fibIter.BeginInvoke(20, ThreadProc, new object[] { fibIter, resetEvents[0] });
            fibRec.BeginInvoke(20, ThreadProc, new object[] { fibIter, resetEvents[1] });
            silniaRec.BeginInvoke(6, ThreadProc, new object[] { fibIter, resetEvents[2] });
            silniaIter.BeginInvoke(6, ThreadProc, new object[] { fibIter, resetEvents[3] });
            WaitHandle.WaitAll(resetEvents);
        }
    }
}
