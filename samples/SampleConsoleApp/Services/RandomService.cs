using System;
using System.Collections.Generic;
using System.Text;

namespace SampleConsoleApp.Services
{
    public interface IRandomService
    {
        int GetInt();
    }

    public class RandomService : IRandomService
    {
        private readonly Random _random;

        public RandomService()
        {
            _random = new Random();
        }

        public int GetInt()
        {
            return _random.Next();
        }
    }
}
