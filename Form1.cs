using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Contexts;

namespace FCFS_SRTF_Project_University
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //قائمه ديناميكيه تكبر وتصغر حسب عدد العمليات والعاناصر التي ستكوه بداخلها من نوع المتغير المعرف
         List<clsParametres> Processes = new List<clsParametres> ();

         List<clsGanttBlock> ganttTimeline = new List<clsGanttBlock> ();
        private void _LoadDataFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); 
            //openFileDialog.DefaultExt = " D:\\ملفات الجامعة";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                string[] lines = File.ReadAllLines(filePath);

                Processes.Clear();// ننضف القيم كل مره نشغل 

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                    {
                        continue;
                    }
                    //هذا الخيار يقول للبرنامج احذفلي اي قيمه فارغه ولا تخزمها مشان ما يطلع معانا  خطا
                    string[] Parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (Parts.Length >= 3)
                    {
                        clsParametres p = new clsParametres();

                        p.Name = Parts[0];
                        p.Arrivetime = int.Parse(Parts[1]);
                        p.BurstTime = int.Parse(Parts[2]);
                        p.RemainingTime = p.BurstTime;

                        Processes.Add(p);

                    }


                }

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = Processes;

                MessageBox.Show("The Operation was successful", "Successfully", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            }
        }



        private void _RefreshGrid()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = Processes;
        }


        private bool _ValidateProcesses()
        {
            if(Processes== null  || Processes.Count == 0)
            {
                MessageBox.Show("There are no items", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void _DrawGanttChart()
        {
           
            flowLayoutPanel1.Controls.Clear();

            //  الدوران على كل فترة زمنية مخزنة في القائمة ورسمها
            foreach (var block in ganttTimeline)
            {
                Panel box = new Panel();
                box.Width = 70;
                box.Height = 55;
                box.Margin = new Padding(2);
                box.BorderStyle = BorderStyle.FixedSingle;
                box.BackColor = (block.ProcessName == "Idle") ? Color.Gray : Color.SteelBlue;

                // اسم العملية في أعلى المربع
                Label lblProcess = new Label();
                lblProcess.Text = block.ProcessName;
                lblProcess.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblProcess.ForeColor = Color.White;
                lblProcess.Dock = DockStyle.Top;
                lblProcess.TextAlign = ContentAlignment.MiddleCenter;

                // نطاق الوقت (من - إلى) في أسفل المربع
                Label lblTime = new Label();
                lblTime.Text = $"{block.StartTime} -> {block.EndTime}";
                lblTime.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                lblTime.ForeColor = Color.LightYellow;
                lblTime.Dock = DockStyle.Bottom;
                lblTime.TextAlign = ContentAlignment.MiddleCenter;

                // تجميع العناصر داخل المربع وإضافتها للبانل الأخضر
                box.Controls.Add(lblProcess);
                box.Controls.Add(lblTime);
                flowLayoutPanel1.Controls.Add(box);
            }
        }


        private void _FCFSAlgor()
        {
            if (!_ValidateProcesses()) return;
            //يتعرف على اسم البيانات تلقائيا 
            ganttTimeline.Clear();
            var sortedProcesses = Processes.OrderBy(p => p.Arrivetime).ToList();

            int currentTime = 0;

            foreach ( var p in sortedProcesses )
            {
                if(currentTime < p.Arrivetime)
                {

                    ganttTimeline.Add(new clsGanttBlock { ProcessName="Idle" ,StartTime =currentTime ,EndTime =p.Arrivetime});
                    currentTime = p.Arrivetime;
                }


                int startTime = currentTime;
                p.CompletionTime = currentTime + p.BurstTime;

                p.TurnaroundTime = p.CompletionTime - p.Arrivetime;

                p.waitingTime = p.TurnaroundTime - p.BurstTime;


                currentTime = p.CompletionTime;

                ganttTimeline.Add(new clsGanttBlock { ProcessName = p.Name, StartTime = startTime, EndTime = p.CompletionTime });



            }

            _RefreshGrid();
            _DrawGanttChart();

            MessageBox.Show("The FCFS algorithm was successful", "FCFS", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        private void _SRTFAlgor()
        {
            if (!_ValidateProcesses()) return;

            foreach(var p in Processes)
            {
                p.RemainingTime = p.BurstTime;
            }


            int CurrentTime = 0;
            int CompletedProceses = 0;

            int totalProcesses = Processes.Count;

            clsParametres PreviousProcess = null;

            while (CompletedProceses < totalProcesses)
            {
                var availableProcesses = Processes.Where(p => p.Arrivetime <= CurrentTime && p.RemainingTime > 0).ToList();

                if(availableProcesses.Count == 0)
                {
                    if(ganttTimeline.Count > 0 && ganttTimeline.Last() .ProcessName =="Idle")
                    {
                        ganttTimeline.Last().EndTime++;
                    }
                    else
                    {
                        ganttTimeline.Add(new clsGanttBlock { ProcessName = "Idle", StartTime = CurrentTime, EndTime = CurrentTime + 1 });
                    }
                    CurrentTime++;
                    continue;
                }

                var shortestProces = availableProcesses.OrderBy(p => p.RemainingTime).ThenBy(p => p.Arrivetime).First();

                if (ganttTimeline.Count > 0 && ganttTimeline.Last().ProcessName == shortestProces.Name)
                {
                    ganttTimeline.Last().EndTime++; // تمديد زمن المربع الحالي
                }
                else
                {
                    ganttTimeline.Add(new clsGanttBlock
                    {
                        ProcessName = shortestProces.Name,
                        StartTime = CurrentTime,
                        EndTime = CurrentTime + 1
                    });
                }

                shortestProces.RemainingTime--;
                CurrentTime++;

                if(shortestProces.RemainingTime ==0)
                {
                    CompletedProceses++;

                    shortestProces.CompletionTime = CurrentTime;
                    shortestProces.TurnaroundTime = shortestProces.CompletionTime - shortestProces.Arrivetime;
                    shortestProces.waitingTime = shortestProces.TurnaroundTime - shortestProces.BurstTime;
                }
            }

            _RefreshGrid();

            _DrawGanttChart();

            MessageBox.Show("The SRTF algorithm was successful", "SRTF", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        private void _ApplyFilter()
        {
           if(comboBox1.Text == "FCFS")
            {

                _FCFSAlgor();

            }
           else if(comboBox1.Text =="SRTF")
            {
                _SRTFAlgor();
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            

            comboBox1.Items.Add("FCFS");
            comboBox1.Items.Add("SRTF");
        }

        private void btnLoadeFile_Click(object sender, EventArgs e)
        {
            _LoadDataFromFile();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _ApplyFilter();
        }
    }
}
