using System;
using System.Collections.Generic;
using System.Linq;

namespace CapacitySensor
{
    public static class Algorithms
    {
        public static List<int> ChargingProbes = new List<int>();
        public static List<int> DischargingProbes = new List<int>();

        public static double Tick = 62.5; // ns
        public static double Gate = 0.0052;

        public static int TCNT_Min = 50;
        public static int TCNT_Max = 2048;

        public static double DewPoint(double T, double RH)
        {
            return Math.Pow(RH / 100.0, 0.125) * (112 + 0.9 * T) + 0.1 * T - 112;
        }
        public static void ParseT(string Received)
        {
            double T = 0.0, RH = 0.0;
            var Parts = Received.Split(' ');
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
            var SamplesIndex = (Received[0] == (char)Device.Commands.SAMPLES) ? 1 : 5;

            DischargingProbes.Clear();
            ChargingProbes.Clear();

            var Pulses = double.Parse(Samples[SamplesIndex]);
            var Gates  = double.Parse(Samples[SamplesIndex+1]);
            var Freq   = Pulses / Gates / Gate;

            var capacity = Capacity(Freq) * 1E12;

            Console.WriteLine(string.Format("Freq {0:0.0} Hz, Capacity {1:0.000000} pF", Freq, capacity));

            var CapacityCorr = capacity;// Correction(capacity);
            var CapacityRound = double.Parse(string.Format("{0:0.000}", CapacityCorr));
            MainForm.Instance.C = CapacityRound;
            MainForm.Instance.LBL_C.Text = string.Format("{0:0.0} pF", CapacityRound);
            MainForm.Instance.LBL_C_Charts.Text = string.Format("{0:0.0} pF", CapacityRound);

            double RH = double.Parse(string.Format("{0:0.0}", CalcHumidity(CapacityCorr)));
            string strRH = string.Format("[{0:0.0} %]", RH);
            if (MainForm.Instance.LBL_RH.Text.Contains("-"))
            {
                MainForm.Instance.LBL_RH.Text = strRH;
                MainForm.Instance.LBL_RH_Charts.Text = strRH;
            }
            else
            {
                MainForm.Instance.LBL_RH.Text += $"\n{strRH}";
                MainForm.Instance.LBL_RH_Charts.Text += $"\n{strRH}";
            }
        }
        public static double Correction(double Capacity)
        {
            Capacity *= 1E-12;
            double CapacityCorr =
                Calibration.A3 * 1E19 * Math.Pow(Capacity, 3) +
                Calibration.A2 * 1E10 * Math.Pow(Capacity, 2) +
                Calibration.A1 * Capacity + 
                Calibration.A0 * 1E-10;

            return CapacityCorr * 1E12;
        }
        public static double Oversampling(List<int> Probes)
        {
            int temp = 16 - (int)Math.Ceiling(Math.Log(Probes.Max(), 2));
            int bits = (temp < 4) ? temp : 3;
            int N = (int)Math.Pow(4, bits);
            long sum = 0;
            for (int i = Probes.Count - N; i < Probes.Count; i++)
                sum += Probes[i];
            double result = sum >> bits;
            return result * Math.Pow(2, -bits);
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
            return -CX * RM * Math.Log((VCapStop - VOut + JC * RM)/(VCapStart - VOut + JC * RM));
        }
        public static double Capacity(double Freq/*, double RM, double JC, double VCapStop, double VCapStart, double VOut*/)
        {
            double CeoffCharging = Calibration.R_MEAS *
                Math.Log(
                    (Calibration.H_THR - Calibration.H_VOUT + Calibration.J * Calibration.R_MEAS) /
                    (Calibration.L_THR - Calibration.H_VOUT + Calibration.J * Calibration.R_MEAS)
                );
            double CeoffDischarging = Calibration.R_MEAS *
                Math.Log(
                    (Calibration.L_THR - Calibration.L_VOUT + Calibration.J * Calibration.R_MEAS) /
                    (Calibration.H_THR - Calibration.L_VOUT + Calibration.J * Calibration.R_MEAS)
                );

            return -1 / Freq / (CeoffCharging + CeoffDischarging);
            //return -T / RM / Math.Log((VCapStop - VOut + JC * RM) / (VCapStart - VOut + JC * RM));
        }

        public static double CalcHumidity(double HS1101_Capacity)
        {
            double X = HS1101_Capacity / 180.0;
            double RH = -3465.5 * X * X * X + 10732 * X * X - 10457 * X + 3245.9;
            if (RH < 0.0) RH = 0.0;
            if (RH > 100.0) RH = 100.0;
            return RH;
        }
    }
}
