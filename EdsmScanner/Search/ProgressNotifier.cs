using System;
using System.Threading;

namespace EdsmScanner.Search
{
    internal class ProgressNotifier
    {
        private readonly string _messageFormat;
        private int _totals;

        public ProgressNotifier(string messageFormat)
        {
            _messageFormat = $"\r{messageFormat}";
        }

        public void NotifyIncrease()
        {
            Console.Write(_messageFormat, Interlocked.Increment(ref _totals));
        }

        public void Finish()
        {
            Console.WriteLine();
        }
    }
}