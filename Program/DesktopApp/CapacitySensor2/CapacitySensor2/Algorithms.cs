using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapacitySensor2
{
    public static class Algorithms
    {
        public static List<int> ChargingProbes = new List<int>();
        public static List<int> DischargingProbes = new List<int>();

        public static double Tick = 62.5; // ns

        public static int TCNT_Min = 30;
        public static int TCNT_Max = (int)Math.Pow(2, 16 - 3);

        public static double DewPoint(double T, double RH)
        {
            return Math.Pow(RH / 100.0, 0.125) * (112 + 0.9 * T) + 0.1 * T - 112;
        }




        public static void ParseT(string Received)
        {
            double T = 0.0, RH = 0.0;
            var Parts = Received.Replace('.', ',').Split(' ');
            if (Received[0] == (char)Device.Commands.TEMP)
            {
                double.TryParse(Parts[1], out T);
                double.TryParse(Parts[3], out RH);
            }
            else if (Received[0] != (char)Device.Commands.SAMPLES)
            {
                double.TryParse(Parts[0], out T);
                double.TryParse(Parts[1], out RH);
            }

            MainForm.Instance.T = T;
            MainForm.Instance.RH = RH;
            MainForm.Instance.DP = DewPoint(T, RH);
            if (T > 0.0 && RH > 0.0)
            {
                MainForm.Instance.LBL_T.Text = string.Format("{0:0.0} °C", T);
                MainForm.Instance.LBL_RH.Text = string.Format("{0:0.0} % ", RH);
                MainForm.Instance.LBL_DP.Text = string.Format("{0:0.0} °C", MainForm.Instance.DP);
            }
            else
            {
                MainForm.Instance.LBL_T.Text = "-";
                MainForm.Instance.LBL_RH.Text = "-";
                MainForm.Instance.LBL_DP.Text = "-";
            }
            MainForm.Instance.LBL_T_Charts.Text = MainForm.Instance.LBL_T.Text;
            MainForm.Instance.LBL_RH_Charts.Text = MainForm.Instance.LBL_RH.Text;
            MainForm.Instance.LBL_DP_Charts.Text = MainForm.Instance.LBL_DP.Text;
        }
        public static void ParseC(string Received)
        {
            var Samples = Received.Split(' ');
            var SamplesIndex = (Received[0] == (char)Device.Commands.SAMPLES) ? 2 : 6;

            DischargingProbes.Clear();
            ChargingProbes.Clear();

            int Size = Samples.Length - SamplesIndex;
            int[] Probes = new int[Size];

            for (int i = SamplesIndex; i < Samples.Length; i++)
                Probes[i - SamplesIndex] = int.Parse(Samples[i]);

            for (int i = 0; i < Size - 1; i++)
            {
                if (i % 2 == 1)
                {
                    DischargingProbes.Add(Probes[i + 1]);
                }
                else
                {
                    ChargingProbes.Add(Probes[i + 1]);
                }
            }
            var DP = Oversample(DischargingProbes);
            DP *= Math.Pow(2, - GetBits(DischargingProbes.Max()));
            DP *= Tick;
            var CP = Oversample(ChargingProbes);
            CP *= Math.Pow(2, - GetBits(DischargingProbes.Max()));
            CP *= Tick;

            var DP_Capacity = -DP / Calibration.R_MEAS / Math.Log(1 - (Calibration.H_THR - Calibration.L_THR) / (Calibration.H_THR - Calibration.L_VOUT) + Calibration.J * Calibration.R_MEAS);
            var CP_Capacity = -CP / Calibration.R_MEAS / Math.Log(1 + (Calibration.H_THR - Calibration.L_THR) / (Calibration.L_THR - Calibration.H_VOUT) + Calibration.J * Calibration.R_MEAS);

            Console.WriteLine(string.Format("CP: {0:0.0}pF   DP: {1:0.0}pF\n", CP_Capacity * 1E3, DP_Capacity * 1E3));

            MainForm.Instance.C = CP_Capacity * 1E3;
            MainForm.Instance.LBL_C.Text = string.Format("{0:0.0} pF", MainForm.Instance.C);
            MainForm.Instance.LBL_C_Charts.Text = string.Format("{0:0.0} pF", MainForm.Instance.C);
        }




        public static double Correction(double Capacity)
        {
            double CapacityCorr =
                Calibration.A3 * Math.Pow(Capacity, 3) +
                Calibration.A2 * Math.Pow(Capacity, 2) +
                Calibration.A1 * Capacity + 
                Calibration.A0;

            return CapacityCorr;
        }



        public static double Oversample(List<int> Probes)
        {
            int bits = GetBits(Probes.Max());
            int N = (int)Math.Pow(4, bits);
            long sum = 0;
            for (int i = Probes.Count - N; i < Probes.Count; i++)
                sum += Probes[i];
            return sum >> bits;
        }


        public static int GetBits(int MaxValue)
        {
            int i = 16 - (int)Math.Ceiling(Math.Log(MaxValue, 2));
            return (i < 4) ? i : 3;
        }




        public static double Capacity(double T, double RM, double JC,
            double VCapStop, double VCapStart, double VOut)
        {
            return -T/RM/Math.Log((VCapStop - VOut + JC * RM) / (VCapStart - VOut + JC * RM));
        }





        public static double ChargingTime(double C)
        {
            return Time(C, Calibration.R_MEAS, Calibration.J,
                Calibration.H_THR, Calibration.L_THR, Calibration.H_VOUT);
        }
        public static double DischargingTime(double C)
        {
            return Time(C, Calibration.R_MEAS, Calibration.J,
                Calibration.L_THR, Calibration.H_THR, Calibration.L_VOUT);
        }
        public static double Time(double CX, double RM, double JC, 
            double VCapStop, double VCapStart, double VOut)
        {
            return -CX * RM * Math.Log((VCapStop - VOut + JC*RM)/(VCapStart - VOut + JC*RM));
        }
    }
}
