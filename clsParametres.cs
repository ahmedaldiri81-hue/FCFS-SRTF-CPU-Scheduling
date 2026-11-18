using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCFS_SRTF_Project_University
{
    public class clsParametres
    {

        public string Name { get; set; }

        public int Arrivetime { get; set; }

        public int BurstTime { get; set; }

        public int RemainingTime { get; set; }

        public int CompletionTime   { get; set; }

        public int TurnaroundTime { get; set; }

        public int waitingTime { get; set; }
    }
}
