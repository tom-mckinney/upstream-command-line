using System;
using System.Collections.Generic;
using System.Text;

namespace SampleConsoleApp.Services
{
    public interface IDeterministicService
    {
        string GetWhatEveryoneLoves();
    }

    public class DeterministicService : IDeterministicService
    {
        public string GetWhatEveryoneLoves()
        {
            return "Ice Cream";
        }
    }
}
