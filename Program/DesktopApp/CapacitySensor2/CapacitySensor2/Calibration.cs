using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapacitySensor2
{
    public static class Calibration
    {
        public static double H_THR;
        public static double L_THR;
        public static double H_VOUT;
        public static double L_VOUT;
        public static uint R_MEAS;

        public static double A0, A1, A2, A3;

        public static double J = 242E-9;

        public static void SetupConstant(string Received)
        {
            var Parts = Received.Replace('.', ',').Split(' ');
            H_THR   = double.Parse(Parts[0]);
            L_THR   = double.Parse(Parts[1]);
            H_VOUT  = double.Parse(Parts[2]);
            L_VOUT  = double.Parse(Parts[3]);
            R_MEAS  = uint.Parse(Parts[4]);
        }

        public static void SetupCorrection(string Received)
        {
            var Parts = Received.Replace('.', ',').Split(' ');
            A0 = double.Parse(Parts[0]);
            A1 = double.Parse(Parts[1]);
            A2 = double.Parse(Parts[2]);
            A3 = double.Parse(Parts[3]);
        }

        public static void Initialize()
        {
            H_THR = (double)MainForm.Instance.NUM_H_THR.Value;
            L_THR = (double)MainForm.Instance.NUM_L_THR.Value;
            H_VOUT = (double)MainForm.Instance.NUM_H_VOUT.Value;
            L_VOUT = (double)MainForm.Instance.NUM_L_VOUT.Value;
            R_MEAS = (uint)(MainForm.Instance.NUM_R_MEAS.Value * 1000);
            A0 = (double)MainForm.Instance.NUM_A0.Value;
            A1 = (double)MainForm.Instance.NUM_A1.Value;
            A2 = (double)MainForm.Instance.NUM_A2.Value;
            A3 = (double)MainForm.Instance.NUM_A3.Value;
        }
    }
}
