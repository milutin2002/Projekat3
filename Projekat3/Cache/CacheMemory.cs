using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Projekat3.Cache
{
    public class CacheMemory
    {
        private ConcurrentDictionary<string, Log> dictionaryLog = new ConcurrentDictionary<string, Log>();
        private static readonly object locker = new object();
        private Timer t;
        public (bool, Log) getLog(string url)
        {
            bool ima=false;
            Log l = null;
            try
            {
                ima = dictionaryLog.TryGetValue(url, out l);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (ima, l);
        }

        public CacheMemory()
        {
            Console.WriteLine("Timer started");
            t = new Timer(remove, null, 0, 60000);
        }
        public void writeResponse(string url, Log l)
        {
            try
            {
                Console.WriteLine("Writing in cache");
                dictionaryLog.TryAdd(url, l);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void remove(object data)
        {
            foreach (var key in dictionaryLog.Keys)
            {
                try
                {
                    if (dictionaryLog[key].expires < DateTime.Now)
                    {
                        Log l;
                        Console.WriteLine("Brisanje iz cache");
                        dictionaryLog.TryRemove(key, out l);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}