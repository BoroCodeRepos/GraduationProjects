using System;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace CapacitySensor2
{
    public partial class MainForm : Form
    {
        Button ActiveButton { get; set; }

        public double C, T, RH, DP;
        private bool IsConnected = false;

        private bool MultipleMeasurement = false;
        private decimal MeasurementsIndex = 0;
        private Device.Commands MultiMeas_CMD = Device.Commands.TRIGGER_MEAS;

        // Mouse position variables
        private bool MouseButtonState = false;
        private Point MouseBasePosition;

        RichTextBox TBOX_FullLogs = new RichTextBox();

        // Global Instance
        public static MainForm Instance;

        public MainForm()
        {
            Instance = this;
            
            InitializeComponent();
            TCTRL.ItemSize = new Size(0, 1);

            Device.Initialize();
            Calibration.Initialize();
            Theme.Initialize();

            Logs.Create(Logs.From.User, Logs.Type.Info, "The application was opened");
            
            DisableMeasurementButtons();
            CalcCapacityRange();

            ChartCapacity.Series["Capacity"].Points.AddXY(-1, 0);
            ChartTemperature.Series["Temperature"].Points.AddXY(-1, 0);
            ChartTemperature.Series["Humidity"].Points.AddXY(-1, 0);
        }

        private void BTN_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void BTN_Minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        private void BTN_Maximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                TBOXFS_Logs.Visible = true;
                TBOX_Logs.Visible = false;
                return;
            }
            WindowState = FormWindowState.Normal;
            TBOXFS_Logs.Visible = false;
            TBOX_Logs.Visible = true;
        }
        private void PanelTop_MouseDown(object sender, MouseEventArgs e)
        {
            MouseButtonState = true;
            MouseBasePosition = MousePosition;
        }
        private void PanelTop_MouseUp(object sender, MouseEventArgs e)
        {
            MouseButtonState = false;
        }
        private void PanelTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtonState)
            {
                int X = MousePosition.X - MouseBasePosition.X + Location.X;
                int Y = MousePosition.Y - MouseBasePosition.Y + Location.Y;
                MouseBasePosition = MousePosition;
                SetDesktopLocation(X, Y);
            }
        }        
        private void Numeric_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown currentNumeric = (NumericUpDown)sender;
            Font currentFont = currentNumeric.Font;
            currentNumeric.Font = new Font(currentFont, FontStyle.Bold);
            currentNumeric.TabStop = false;
            CalcCapacityRange();
            CreateCorrectionChart();

            if (currentNumeric.Name.Contains("NUM_H_THR"))
                if (NUM_H_THR.Value < NUM_L_THR.Value)
                    NUM_H_THR.Value = NUM_L_THR.Value;

            if (currentNumeric.Name.Contains("NUM_L_THR"))
                if (NUM_L_THR.Value > NUM_H_THR.Value)
                    NUM_L_THR.Value = NUM_H_THR.Value;
        }
        private void TIM_DevCon_Tick(object sender, EventArgs e)
        {
            bool DeviceState = Device.IsAvailable();

            if (DeviceState && !IsConnected)
            {
                IsConnected = true;

                if (Device.Open())
                {
                    GetConstantValues();
                    GetCorrectionValues();
                    CalcCapacityRange();
                    CreateCorrectionChart();
                    EnableMeasurementButtons();
                    Footer("Connecting Successful", Color.Green);
                    LBL_Connection.Text = $"Device: Connected ({Device.GetCurrentPortName()})";
                    LBL_Connection.ForeColor = Color.Green;
                    return;
                }
                Footer("Communication Error, check the USB cable connection.", Color.Maroon);
                DisableMeasurementButtons();
            }
            else if (!DeviceState && IsConnected)
            {
                DisableMeasurementButtons();
                Footer("Try to connect device to PC", Color.DodgerBlue);
                IsConnected = false;
                LBL_Connection.Text = "Device: Disconnected";
                LBL_Connection.ForeColor = Color.Maroon;
                Device.Close();
            }
        }
        private void TIM_Meas_Tick(object sender, EventArgs e)
        {
            TIM_Meas.Interval = (int)Del_Meas.Value * 1000;

            if (MeasurementsIndex < N_Meas.Value)
            {
                if (Device.SendCommand(MultiMeas_CMD, out string Received))
                {
                    Algorithms.ParseT(Received);
                    ChartTemperature.Series["Temperature"].Points.AddXY(MeasurementsIndex + 1, T);
                    ChartTemperature.Series["Humidity"].Points.AddXY(MeasurementsIndex + 1, RH);
                    if (CBOX_DewpointCalc.Checked)
                        ChartTemperature.Series["Dewpoint"].Points.AddXY(MeasurementsIndex + 1, DP);
                    UpdateTemperatureCharts();
                    if (MultiMeas_CMD == Device.Commands.TRIGGER_MEAS)
                    {
                        Algorithms.ParseC(Received);
                        double CapacityAfterCorrection = Algorithms.Correction(C);
                        ChartCapacity.Series["Capacity"].Points.AddXY(MeasurementsIndex + 1, CapacityAfterCorrection);
                        UpdateCapacityChart();
                    }
                    else
                    {
                        LBL_C.Text = "-"; LBL_C_Charts.Text = "-";
                    }
                    PBAR.Value = (int)((double)(MeasurementsIndex + 1) * 100.0 / (double)N_Meas.Value);
                    LBL_PBAR.Text = $"[{MeasurementsIndex + 1}/{N_Meas.Value}]";
                }
                else
                {
                    /* Error Received */
                    LBL_C.Text = "-";  LBL_C_Charts.Text = "-";
                    LBL_T.Text = "-";  LBL_T_Charts.Text = "-";
                    LBL_RH.Text = "-"; LBL_RH_Charts.Text = "-";
                    LBL_DP.Text = "-"; LBL_DP_Charts.Text = "-";
                    TIM_Meas.Enabled = false;
                    MessageBox.Show(
                        "Measurements Failed - Check Logs For More Information",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop
                    );
                    EnableMeasurementButtons();
                }
                ++MeasurementsIndex;
            }
            else
            {
                TIM_Meas.Enabled = false;
                PBAR.Value = 0;
                LBL_PBAR.Text = $"[1/{N_Meas.Value}]";
                Footer("Series of measurements ended successfully", Color.Green);
                Logs.Create(Logs.From.PC, Logs.Type.Info, "Series of measurements ended successfully");
                EnableMeasurementButtons();
            }
        }




        private void ActivateButton(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(Button))
            {
                Button SenderButton = (Button)sender;
                if (ActiveButton != SenderButton)
                {
                    DeactivateButtons();
                    Theme.GetRandomColor();
                    PanelTitle.BackColor = Theme.PrimaryColor;
                    BTN_Close.BackColor = Theme.PrimaryColor;
                    ActiveButton = null;
                    if (SenderButton.Name != "BTN_Close")
                    {
                        SenderButton.Font = new Font(
                            "Microsoft Sans Serif",
                            12f,
                            FontStyle.Regular,
                            GraphicsUnit.Point,
                            0
                        );
                        SenderButton.BackColor = Theme.SecondaryColor;
                        SenderButton.Cursor = Cursors.Default;
                        SenderButton.ForeColor = Color.Gainsboro;
                        ActiveButton = SenderButton;
                        BTN_Close.Visible = true;
                    }
                    OpenForm(SenderButton);
                    LoadFormTheme();
                }
            }
        }
        private void DeactivateButtons()
        {
            foreach (Button btn in PanelMenu.Controls.OfType<Button>())
            {
                if (btn.Name == "BTN_Close") continue;
                btn.Cursor = Cursors.Hand;
                btn.BackColor = PanelMenu.BackColor;
                btn.ForeColor = PanelMenu.ForeColor;
                btn.Font = new Font(
                    "Microsoft Sans Serif",
                    10f,
                    FontStyle.Regular,
                    GraphicsUnit.Point,
                    0
                );
            }
        }
        private void OpenForm(Button sender)
        {
            switch (sender.Name)
            {
                case "BTN_Close":
                    TCTRL.SelectedTab = PageHome;
                    LBL_Title.Text = "HOME";
                    BTN_Close.Visible = false;
                    break;
                case "BTN_Generals":
                    TCTRL.SelectedTab = PageGenerals;
                    LBL_Title.Text = "Generals";
                    break;
                case "BTN_Calibration":
                    TCTRL.SelectedTab = PageCalibration;
                    LBL_Title.Text = "Calibration";
                    break;
                case "BTN_Measurement":
                    TCTRL.SelectedTab = PageMeasurement;
                    LBL_Title.Text = "Measurement";
                    break;
                case "BTN_Charts":
                    TCTRL.SelectedTab = PageCharts;
                    LBL_Title.Text = "Charts";
                    break;
                case "BTN_Logs":
                    TCTRL.SelectedTab = PageLogs;
                    LBL_Title.Text = "Logs";
                    break;
                case "BTN_About":
                    TCTRL.SelectedTab = PageAbout;
                    LBL_Title.Text = "About";
                    break;
            }
        }
        private void LoadFormTheme()
        {
            switch (TCTRL.SelectedTab.Text)
            {
                case "Generals":
                    BTN_Calibrate.BackColor = Theme.PrimaryColor;
                    BTN_MatlabLast.BackColor = Theme.PrimaryColor;
                    BTN_MatlabSession.BackColor = Theme.PrimaryColor;
                    BTN_ExportXML.BackColor = Theme.PrimaryColor;
                    PNL_BTM_GEN1.BackColor = Theme.PrimaryColor;
                    PNL_BTM_GEN2.BackColor = Theme.PrimaryColor;
                    break;
                case "Calibration":
                    CreateCorrectionChart();
                    BTN_ReadConst.BackColor = Theme.PrimaryColor;
                    BTN_ReadConstDef.BackColor = Theme.PrimaryColor;
                    BTN_SetConst.BackColor = Theme.PrimaryColor;
                    BTN_ReadCorr.BackColor = Theme.PrimaryColor;
                    BTN_ReadCorrDef.BackColor = Theme.PrimaryColor;
                    BTN_SetCorr.BackColor = Theme.PrimaryColor;
                    PanelCal1.BackColor = Theme.PrimaryColor;
                    PanelCal2.BackColor = Theme.PrimaryColor;
                    PanelCal3.BackColor = Theme.PrimaryColor;
                    PanelCal4.BackColor = Theme.PrimaryColor;
                    CorrChart.PaletteCustomColors[0] = Theme.SecondaryColor;
                    Panel[] panels = new Panel[] {
                        PNL_H_THR, PNL_L_THR, PNL_H_VOUT, PNL_L_VOUT, PNL_R_MEAS,
                        PNL_C_MIN, PNL_C_MAX, PNL_A0, PNL_A1, PNL_A2, PNL_A3
                    };
                    foreach (Panel globalPanel in panels)
                        foreach (Panel panel in globalPanel.Controls.OfType<Panel>())
                            if (panel.Size.Width == 1 || panel.Size.Height == 1)
                                panel.BackColor = Theme.PrimaryColor;
                    break;
                case "Measurement":
                    PanelMeas1.BackColor = Theme.PrimaryColor;
                    PanelMeas2.BackColor = Theme.PrimaryColor;
                    PanelMeas3.BackColor = Theme.PrimaryColor;
                    PanelMeas4.BackColor = Theme.PrimaryColor;
                    PanelMeas5.BackColor = Theme.PrimaryColor;
                    BTN_MeasBegin.BackColor = Theme.PrimaryColor;
                    BTN_MeasT.BackColor = Theme.PrimaryColor;
                    foreach (Panel panel in PNL_N_Meas.Controls.OfType<Panel>())
                        if (panel.Size.Width == 1 || panel.Size.Height == 1)
                            panel.BackColor = Theme.PrimaryColor;
                    foreach (Panel panel in PNL_Del_Meas.Controls.OfType<Panel>())
                        if (panel.Size.Width == 1 || panel.Size.Height == 1)
                            panel.BackColor = Theme.PrimaryColor;
                    break;
                case "Charts":
                    PanelCharts1.BackColor = Theme.PrimaryColor;
                    PanelCharts2.BackColor = Theme.PrimaryColor;
                    PanelCharts3.BackColor = Theme.PrimaryColor;
                    PanelCharts4.BackColor = Theme.PrimaryColor;
                    break;
                case "Logs":
                    BTN_Send.BackColor = Theme.PrimaryColor;
                    break;
                case "About":
                    break;
            }
        }




        private void GetConstantValues()
        {
            if (Device.SendCommand(Device.Commands.GET_CONSTANTS, out string Received))
            {
                if (Received.Contains("NaN"))
                {
                    Device.SendCommand(Device.Commands.DEF_CONSTANTS, out _);
                    Device.SendCommand(Device.Commands.GET_CONSTANTS, out Received);
                }
                Calibration.SetupConstant(Received);
                NUM_H_THR.Value = new decimal(Calibration.H_THR);
                NUM_L_THR.Value = new decimal(Calibration.L_THR);
                NUM_H_VOUT.Value = new decimal(Calibration.H_VOUT);
                NUM_L_VOUT.Value = new decimal(Calibration.L_VOUT);
                NUM_R_MEAS.Value = new decimal(Calibration.R_MEAS / 1000.0);
                Font currentFont = NUM_H_THR.Font;
                NumericUpDown[] Nums = new NumericUpDown[]
                {
                    NUM_H_THR, NUM_L_THR, NUM_H_VOUT, NUM_L_VOUT, NUM_R_MEAS,
                };
                foreach (NumericUpDown Num in Nums)
                {
                    Num.Font = new Font(currentFont, FontStyle.Regular);
                    Num.TabStop = false;
                }
            }
        }
        private void GetCorrectionValues()
        {
            if (Device.SendCommand(Device.Commands.GET_CORRECTIONS, out string Received))
            {
                if (Received.Contains("NaN"))
                {
                    Device.SendCommand(Device.Commands.DEF_CORRECTIONS, out _);
                    Device.SendCommand(Device.Commands.GET_CORRECTIONS, out Received);
                }
                Calibration.SetupCorrection(Received);
                NUM_A0.Value = new decimal(Calibration.A0);
                NUM_A1.Value = new decimal(Calibration.A1);
                NUM_A2.Value = new decimal(Calibration.A2);
                NUM_A3.Value = new decimal(Calibration.A3);
                Font currentFont = NUM_A0.Font;
                NumericUpDown[] Nums = new NumericUpDown[]
                {
                    NUM_A0, NUM_A1, NUM_A2, NUM_A3
                };
                foreach (NumericUpDown Num in Nums)
                {
                    Num.Font = new Font(currentFont, FontStyle.Regular);
                    Num.TabStop = false;
                }
            }
        }
        private void UpdateCalibrationNumField()
        {
            NUM_H_THR.Value = new decimal(Calibration.H_THR);
            NUM_L_THR.Value = new decimal(Calibration.L_THR);
            NUM_H_VOUT.Value = new decimal(Calibration.H_VOUT);
            NUM_L_VOUT.Value = new decimal(Calibration.L_VOUT);
            NUM_R_MEAS.Value = new decimal(Calibration.R_MEAS / 1000.0);
            NUM_A0.Value = new decimal(Calibration.A0);
            NUM_A1.Value = new decimal(Calibration.A1);
            NUM_A2.Value = new decimal(Calibration.A2);
            NUM_A3.Value = new decimal(Calibration.A3);
            Font currentFont = NUM_H_THR.Font;
            NumericUpDown[] Nums = new NumericUpDown[]
            {
                NUM_H_THR, NUM_L_THR, NUM_H_VOUT, NUM_L_VOUT, NUM_R_MEAS,
                NUM_A0, NUM_A1, NUM_A2, NUM_A3
            };
            foreach (NumericUpDown Num in Nums)
            {
                Num.Font = new Font(currentFont, FontStyle.Regular);
                Num.TabStop = false;
            }
        }
        private void CalcCapacityRange()
        {
            double Charging = Algorithms.Capacity(Algorithms.TCNT_Min * Algorithms.Tick,
                (double)NUM_R_MEAS.Value, Calibration.J,
                (double)NUM_H_THR.Value, (double)NUM_L_THR.Value, (double)NUM_H_VOUT.Value);
            double Discharging = Algorithms.Capacity(Algorithms.TCNT_Min * Algorithms.Tick, 
                (double)NUM_R_MEAS.Value, Calibration.J,
                (double)NUM_L_THR.Value, (double)NUM_H_THR.Value, (double)NUM_L_VOUT.Value);
            LBL_CMIN.Text = string.Format("{0:0.0} pF", Math.Max(Charging, Discharging));


            Charging = Algorithms.Capacity(Algorithms.TCNT_Max * Algorithms.Tick,
                (double)NUM_R_MEAS.Value, Calibration.J,
                (double)NUM_H_THR.Value, (double)NUM_L_THR.Value, (double)NUM_H_VOUT.Value);
            Discharging = Algorithms.Capacity(Algorithms.TCNT_Max * Algorithms.Tick,
                (double)NUM_R_MEAS.Value, Calibration.J,
                (double)NUM_L_THR.Value, (double)NUM_H_THR.Value, (double)NUM_L_VOUT.Value);
            double MaxC = Math.Min(Charging, Discharging);
            if (MaxC >= 1000.0)
            {
                MaxC /= 1000.0;
                LBL_CMAX.Text = string.Format("{0:0.0} nF", MaxC);
            }
            else
            {
                LBL_CMAX.Text = string.Format("{0:0.0} pF", MaxC);
            }
        }
        private void CreateCorrectionChart()
        {
            uint C_MIN = (uint)(10.0 * double.Parse(LBL_CMIN.Text.Split(' ')[0]));
            uint C_MAX = (uint)(10.0 * double.Parse(LBL_CMAX.Text.Split(' ')[0]));
            if (LBL_CMAX.Text.Contains("nF")) C_MAX *= 1000;
            double[] A = new double[]
            {
                (double)NUM_A0.Value,
                (double)NUM_A1.Value,
                (double)NUM_A2.Value,
                (double)NUM_A3.Value,
            };
            double Cx(double Cp) => A[3] * Math.Pow(Cp, 3) +
                A[2] * Math.Pow(Cp, 2) + A[1] * Cp + A[0];

            CorrChart.Series[0].Points.Clear();
            for (uint i = C_MIN; i <= C_MAX; i += 100)
            {
                double Cp = i / 10.0;
                CorrChart.Series[0].Points.AddXY(Cp, Cx(Cp));
            }
        }
        private void UpdateTemperatureCharts()
        {
            /* Temperature Correction */
            double first_min = ChartTemperature.Series["Temperature"].Points.Select(v => v.YValues[0]).Min();
            double first_max = ChartTemperature.Series["Temperature"].Points.Select(v => v.YValues[0]).Max();
            int first_dif = (int)Math.Ceiling(first_max) - (int)Math.Floor(first_min);
            if (CBOX_DewpointCalc.Checked)
            {
                double DP_min = ChartTemperature.Series["Dewpoint"].Points.Select(v => v.YValues[0]).Min();
                double DP_max = ChartTemperature.Series["Dewpoint"].Points.Select(v => v.YValues[0]).Max();
                first_max = Math.Max(first_max, DP_max);
                first_min = Math.Min(first_min, DP_min);
                first_dif = (int)Math.Ceiling(first_max) - (int)Math.Floor(first_min);
            }
            double corr = (first_dif < 2) ? 0.5 : 1.0;
            ChartTemperature.ChartAreas[0].AxisY.Maximum = Math.Ceiling(first_max) + corr;
            ChartTemperature.ChartAreas[0].AxisY.Minimum = Math.Floor(first_min) - corr;
            if (ChartTemperature.ChartAreas[0].AxisY.Minimum < 0.0)
                ChartTemperature.ChartAreas[0].AxisY.Minimum = 0.0;

            /* Humidity Correction */
            double second_min = ChartTemperature.Series["Humidity"].Points.Select(v => v.YValues[0]).Min();
            double second_max = ChartTemperature.Series["Humidity"].Points.Select(v => v.YValues[0]).Max();
            int second_dif = (int)Math.Ceiling(second_max) - (int)Math.Floor(second_min);
            corr = (second_dif < 2) ? 0.5 : 1.0;
            ChartTemperature.ChartAreas[0].AxisY2.Maximum = Math.Ceiling(second_max) + corr;
            ChartTemperature.ChartAreas[0].AxisY2.Minimum = Math.Floor(second_min) - corr;
            if (ChartTemperature.ChartAreas[0].AxisY2.Minimum < 0.0)
                ChartTemperature.ChartAreas[0].AxisY2.Minimum = 0.0;
            if (ChartTemperature.ChartAreas[0].AxisY2.Maximum > 100.0)
                ChartTemperature.ChartAreas[0].AxisY2.Maximum = 100.0;
        }
        private void UpdateCapacityChart()
        {
            double cap_min = ChartCapacity.Series["Capacity"].Points.Select(v => v.YValues[0]).Min();
            double cap_max = ChartCapacity.Series["Capacity"].Points.Select(v => v.YValues[0]).Max();
            int second_dif = (int)Math.Ceiling(cap_max) - (int)Math.Floor(cap_min);
            double corr = (second_dif < 2) ? 0.5 : 1.0;
            ChartCapacity.ChartAreas[0].AxisY.Maximum = Math.Ceiling(cap_max) + corr;
            ChartCapacity.ChartAreas[0].AxisY.Minimum = Math.Floor(cap_min) - corr;
            if (ChartCapacity.ChartAreas[0].AxisY.Minimum < 0.0)
                ChartCapacity.ChartAreas[0].AxisY.Minimum = 0.0;
        }




        private void EnableMeasurementButtons()
        {
            PNL_Measurement.Enabled = true;

            if (!CBOX_MulMeas.Checked)
            {
                LBL_N_Meas.Enabled = false;
                LBL_Del_Meas.Enabled = false;
                N_Meas.Enabled = false;
                Del_Meas.Enabled = false;
            }

            foreach (Panel panel in PageCalibration.Controls.OfType<Panel>())
                foreach (Control control in panel.Controls)
                    if (!control.Name.Contains("LBL_BTM_CAL"))
                        control.Enabled = true;

            BTN_Calibrate.Enabled = true;
            BTN_MatlabLast.Enabled = true;
            BTN_MatlabSession.Enabled = true;
            BTN_Send.Enabled = true;
            TBOX_Send.Enabled = true;
            LBL_GEN1.Enabled = true;
            LBL_GEN2.Enabled = true;
            LBL_GEN3.Enabled = true;
        }
        private void DisableMeasurementButtons()
        {
            PNL_Measurement.Enabled = false;
            foreach (Panel panel in PageCalibration.Controls.OfType<Panel>())
                foreach (Control control in panel.Controls)
                    if (!control.Name.Contains("LBL_BTM_CAL"))
                        control.Enabled = false;

            BTN_Calibrate.Enabled = false;
            BTN_MatlabLast.Enabled = false;
            BTN_MatlabSession.Enabled = false;
            BTN_Send.Enabled = false;
            TBOX_Send.Enabled = false;
            LBL_GEN1.Enabled = false;
            LBL_GEN2.Enabled = false;
            LBL_GEN3.Enabled = false;
        }
        private void InitMultiMeas()
        {
            MeasurementsIndex = 0;
            TIM_Meas.Interval = 1;
            PBAR.Enabled = true;
            ChartCapacity.Series["Capacity"].Points.Clear();
            ChartTemperature.Series["Temperature"].Points.Clear();
            ChartTemperature.Series["Humidity"].Points.Clear();
            ChartTemperature.Series["Dewpoint"].Points.Clear();
        }




        private void CBOX_MulMeas_CheckedChanged(object sender, EventArgs e)
        {
            if (CBOX_MulMeas.Checked)
            {
                N_Meas.Enabled = true;
                Del_Meas.Enabled = true;
                PBAR.Visible = true;
                LBL_PBAR.Visible = true;
                LBL_N_Meas.Enabled = true;
                LBL_Del_Meas.Enabled = true;
                MultipleMeasurement = true;
                return;
            }
            N_Meas.Enabled = false;
            Del_Meas.Enabled = false;
            PBAR.Enabled = false;
            LBL_N_Meas.Enabled = false;
            LBL_Del_Meas.Enabled = false;
            MultipleMeasurement = false;
            PBAR.Visible = false;
            LBL_PBAR.Visible = false;
        }
        private void N_Meas_ValueChanged(object sender, EventArgs e)
        {
            ChartCapacity.ChartAreas[0].AxisX.Minimum = 1;
            ChartTemperature.ChartAreas[0].AxisX.Minimum = 1;
            ChartCapacity.ChartAreas[0].AxisX.Maximum = (double)N_Meas.Value;
            ChartTemperature.ChartAreas[0].AxisX.Maximum = (double)N_Meas.Value;
            LBL_PBAR.Text = $"[1/{N_Meas.Value}]";
        }
        private void BTN_MeasT_Click(object sender, EventArgs e)
        {
            BTN_MatlabLast.Enabled = false;
            if (!MultipleMeasurement)
            {
                /* Single Measurement */
                if (Device.SendCommand(Device.Commands.GET_TEMP_RH, out string Received))
                {
                    LBL_C.Text = "-"; LBL_C_Charts.Text = "-";
                    Algorithms.ParseT(Received);
                    Footer("Temp. and RH measurement was successful", Color.Green);
                    return;
                }
                /* Device Error */
                LBL_C.Text = "-"; LBL_C_Charts.Text = "-";
                LBL_T.Text = "-"; LBL_T_Charts.Text = "-";
                LBL_RH.Text = "-"; LBL_RH_Charts.Text = "-";
                LBL_DP.Text = "-"; LBL_DP_Charts.Text = "-";
                Footer("Temp. and RH measurement ended in error", Color.Maroon);
                return;
            }
            /* Multiple Measurement */
            Logs.Create(Logs.From.PC, Logs.Type.Info, "Series start of T, RH measurements");
            Footer("Series start of T, RH measurements", Color.Green);
            MultiMeas_CMD = Device.Commands.GET_TEMP_RH;
            InitMultiMeas();
            ChartCapacity.Series["Capacity"].Points.AddXY(-1, 0);
            DisableMeasurementButtons();
            TIM_Meas.Enabled = true;
        }
        private void BTN_MeasBegin_Click(object sender, EventArgs e)
        {
            UpdateCalibrationNumField();
            if (!MultipleMeasurement)
            {
                /* Single Measurement */
                if (Device.SendCommand(Device.Commands.TRIGGER_MEAS, out string Received))
                {
                    Algorithms.ParseT(Received);
                    Algorithms.ParseC(Received);
                    Footer("Full measurement was successful", Color.Green);
                    return;
                }
                /* Device Error */
                LBL_C.Text = "-";  LBL_C_Charts.Text = "-";
                LBL_T.Text = "-";  LBL_T_Charts.Text = "-";
                LBL_RH.Text = "-"; LBL_RH_Charts.Text = "-";
                LBL_DP.Text = "-"; LBL_DP_Charts.Text = "-";
                Footer("Full measurement ended in error", Color.Maroon);
                return;
            }
            /* Multiple Measurement */
            Logs.Create(Logs.From.PC, Logs.Type.Info, "Series start of full measurements");
            Footer("Series start of full measurements", Color.Green);
            MultiMeas_CMD = Device.Commands.TRIGGER_MEAS;
            InitMultiMeas();
            DisableMeasurementButtons();
            TIM_Meas.Enabled = true;
        }





        private void BTN_Send_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TBOX_Send.Text))
            {
                Footer("Sending message success", Color.Green);
                Device.SendString(TBOX_Send.Text, out string Received);
                TBOX_Send.Text = "";
                TBOX_Send.Focus();
                return;
            }
            MessageBox.Show(
                "Unable to send blank message",
                "Message sending warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            TBOX_Send.Focus();
            Footer("Blank message recognized", Color.Orange);
        }
        private void BTN_ExportXML_Click(object sender, EventArgs e)
        {
            var Lines = TBOX_FullLogs.Text.Split('\n');
            DateTime now = DateTime.Now;
            string PathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); 
            string FileName = $"Logs-{now.Hour}{now.Minute}{now.Second}.xml";
            string Path = PathDesktop + "\\" + FileName;
            Logs.Create(Logs.From.User, Logs.Type.Info, $"Log XML Document Generated - Desktop\\{FileName}");

            XmlWriterSettings Settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("    "),
                CloseOutput = true,
                OmitXmlDeclaration = true
            };

            using (XmlWriter xml = XmlWriter.Create(Path, Settings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("CapacitySensorLogs");
                xml.WriteAttributeString("DateGenerated", DateTime.Now.ToString());
                for (int i = 0; i < Lines.Length - 1; i++)
                {
                    var Parts = Lines[i].Split('[', ']');
                    string From = Parts[1];
                    string Type = Parts[3];
                    string Date = Parts[5];
                    string Message = Parts[7];
                    xml.WriteStartElement("Log");
                    xml.WriteAttributeString("Date", Date);
                    if (From == "U")
                        xml.WriteAttributeString("From", "User");
                    if (From == "P")
                        xml.WriteAttributeString("From", "PC");
                    if (From == "D")
                        xml.WriteAttributeString("From", "Device");
                    if (Type == "Inf")
                        xml.WriteAttributeString("Type", "Info");
                    if (Type == "Err")
                        xml.WriteAttributeString("Type", "ERROR");
                    if (Type == "War")
                        xml.WriteAttributeString("Type", "Warning");
                    xml.WriteString(Message);
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
                xml.Flush();
                xml.Close();
            }
            Footer($"LogFile Generated: Desktop\\{FileName}", Color.Green);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C Explorer \"{PathDesktop}\\{FileName}\""
            };
            Process process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
        }
        private void BTN_Calibrate_Click(object sender, EventArgs e)
        {
            Footer("Calibration Device", Color.Orange);
            if (Device.SendCommand(Device.Commands.SET_GENERATIONS, out _))
            {
                Font f = LBL_GEN1.Font;
                LBL_GEN1.Font = new Font(f, FontStyle.Bold);
                MessageBox.Show("Generations 1MHz on Signal Pin", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LBL_GEN1.Font = new Font(f, FontStyle.Regular);
                if (Device.SendCommand(Device.Commands.SET_H_VOUT, out _))
                {
                    LBL_GEN2.Font = new Font(f, FontStyle.Bold);
                    MessageBox.Show("H_VOUT - High State on Signal Pin", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LBL_GEN2.Font = new Font(f, FontStyle.Regular);
                    if (Device.SendCommand(Device.Commands.SET_L_VOUT, out _))
                    {
                        LBL_GEN3.Font = new Font(f, FontStyle.Bold);
                        MessageBox.Show("L_VOUT - Low State on Signal Pin", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LBL_GEN3.Font = new Font(f, FontStyle.Regular);
                    }
                }
            }
            if (!Device.SendCommand(Device.Commands.SET_NOMINAL, out _))
            {
                MessageBox.Show("Device Error: Please, Restart Device", "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Footer("Calibration Ended in Error: Restart Device", Color.Maroon);
                return;
            }
            Footer("Calibration Device was successfully", Color.Green);
        }




        private void BTN_ReadConstDef_Click(object sender, EventArgs e)
        {
            Footer("Reading Default Constant Values", Color.Green);
            if (Device.SendCommand(Device.Commands.DEF_CONSTANTS, out _))
            {
                if (Device.SendCommand(Device.Commands.GET_CONSTANTS, out string Received))
                {
                    if (Received.Contains("NaN"))
                    {
                        MessageBox.Show(
                            "Device Error: Can not save default constant values - EEPROM Error", 
                            "Device Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Calibration.SetupConstant(Received);
                    NUM_H_THR.Value = new decimal(Calibration.H_THR);
                    NUM_L_THR.Value = new decimal(Calibration.L_THR);
                    NUM_H_VOUT.Value = new decimal(Calibration.H_VOUT);
                    NUM_L_VOUT.Value = new decimal(Calibration.L_VOUT);
                    NUM_R_MEAS.Value = new decimal(Calibration.R_MEAS / 1000.0);
                    Font currentFont = NUM_H_THR.Font;
                    NumericUpDown[] Nums = new NumericUpDown[]
                    {
                        NUM_H_THR, NUM_L_THR, NUM_H_VOUT, NUM_L_VOUT, NUM_R_MEAS,
                    };
                    foreach (NumericUpDown Num in Nums)
                    {
                        Num.Font = new Font(currentFont, FontStyle.Regular);
                        Num.TabStop = false;
                    }
                }
            }
        }
        private void BTN_ReadCorrDef_Click(object sender, EventArgs e)
        {
            Footer("Reading Default Correction Values", Color.Green);
            if (Device.SendCommand(Device.Commands.DEF_CORRECTIONS, out _))
            {
                if (Device.SendCommand(Device.Commands.GET_CORRECTIONS, out string Received))
                {
                    if (Received.Contains("NaN"))
                    {
                        MessageBox.Show(
                            "Device Error: Can not save default correction values - EEPROM Error",
                            "Device Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Calibration.SetupCorrection(Received);
                    NUM_A0.Value = new decimal(Calibration.A0);
                    NUM_A1.Value = new decimal(Calibration.A1);
                    NUM_A2.Value = new decimal(Calibration.A2);
                    NUM_A3.Value = new decimal(Calibration.A3);
                    Font currentFont = NUM_A0.Font;
                    NumericUpDown[] Nums = new NumericUpDown[]
                    {
                        NUM_A0, NUM_A1, NUM_A2, NUM_A3
                    };
                    foreach (NumericUpDown Num in Nums)
                    {
                        Num.Font = new Font(currentFont, FontStyle.Regular);
                        Num.TabStop = false;
                    }
                }
            }
        }
        private void BTN_ReadConst_Click(object sender, EventArgs e)
        {
            Footer("Reading Constant Values", Color.Orange);
            GetConstantValues();
        }
        private void BTN_ReadCorr_Click(object sender, EventArgs e)
        {
            Footer("Reading Correction Values", Color.Orange);
            GetCorrectionValues();
        }
        private void BTN_SetConst_Click(object sender, EventArgs e)
        {
            string ToSend = string.Format("{0:0.0000} {1:0.0000} {2:0.0000} {3:0.0000} {4:000000}",
                NUM_H_THR.Value,
                NUM_L_THR.Value,
                NUM_H_VOUT.Value,
                NUM_L_VOUT.Value,
                (uint)NUM_R_MEAS.Value * 1E3
            ).Replace(',', '.');
            if (Device.SendString(Device.Commands.SET_CONSTANTS, ToSend, out _))
            {
                Font currentFont = NUM_H_THR.Font;
                NumericUpDown[] Nums = new NumericUpDown[]
                {
                        NUM_H_THR, NUM_L_THR, NUM_H_VOUT, NUM_L_VOUT, NUM_R_MEAS,
                };
                foreach (NumericUpDown Num in Nums)
                {
                    Num.Font = new Font(currentFont, FontStyle.Regular);
                    Num.TabStop = false;
                }
                Footer("Settings Correction Values Successfully", Color.Green);
                return;
            }
            Footer("Settings Correction Values Ended in Error", Color.Maroon);
        }
        private void BTN_SetCorr_Click(object sender, EventArgs e)
        {
            string ToSend = string.Format("{0:0.0000} {1:0.0000} {2:0.0000} {3:0.0000}",
                NUM_A0.Value,
                NUM_A1.Value,
                NUM_A2.Value,
                NUM_A3.Value
            ).Replace(',', '.');
            if (Device.SendString(Device.Commands.SET_CORRECTIONS, ToSend, out _))
            {
                Font currentFont = NUM_A0.Font;
                NumericUpDown[] Nums = new NumericUpDown[]
                {
                    NUM_A0, NUM_A1, NUM_A2, NUM_A3
                };
                foreach (NumericUpDown Num in Nums)
                {
                    Num.Font = new Font(currentFont, FontStyle.Regular);
                    Num.TabStop = false;
                }
                Footer("Settings Correction Values Successfully", Color.Green);
                return;
            }
            Footer("Settings Correction Values Ended in Error", Color.Maroon);
        }

        

        
        public void AppendLog(Logs.Log Log)
        {
            Color SelectionColor = Color.DarkOrange;
            string NormalScreenMessage = Log.message;
            string FullScreenMessage = Log.message;
            int FullScreenLen = 110;
            int NormalScreenLen = 50;
            if (Log.message.Length - 5 > FullScreenLen)
                FullScreenMessage = Log.message.Substring(0, FullScreenLen - 3) + "...";
            if (Log.message.Length - 5 > NormalScreenLen)
                NormalScreenMessage = Log.message.Substring(0, NormalScreenLen - 3) + "...";

            if (!LBL_Columns.Enabled)
            {
                LBL_Columns.Enabled = true;
                TBOX_Logs.Clear();
                TBOXFS_Logs.Clear();
            }
            if (Log.from == Logs.From.User)
                SelectionColor = Color.Goldenrod;
            else if (Log.from == Logs.From.PC)
                SelectionColor = Color.DarkOliveGreen;
            TBOX_Logs.SelectionColor = SelectionColor;
            TBOXFS_Logs.SelectionColor = SelectionColor;
            TBOX_Logs.SelectedText += $" [{Log.from.ToString()[0]}]\t";
            TBOXFS_Logs.SelectedText += $" [{Log.from.ToString()[0]}]\t";
            TBOX_FullLogs.SelectedText += $" [{Log.from.ToString()[0]}]\t";

            if (Log.type == Logs.Type.Warning)
                SelectionColor = Color.Orange;
            else if (Log.type == Logs.Type.Error)
                SelectionColor = Color.Red;
            else if (Log.type == Logs.Type.Info)
                SelectionColor = Color.Gray;
            TBOX_Logs.SelectionColor = SelectionColor;
            TBOXFS_Logs.SelectionColor = SelectionColor;
            TBOX_Logs.SelectedText += $" [{Log.type.ToString().Substring(0, 3)}]\t  ";
            TBOXFS_Logs.SelectedText += $" [{Log.type.ToString().Substring(0, 3)}]\t  ";
            TBOX_FullLogs.SelectedText += $" [{Log.type.ToString().Substring(0, 3)}]\t  ";

            TBOX_Logs.SelectionColor = Color.Coral;
            TBOXFS_Logs.SelectionColor = Color.Coral;
            TBOX_Logs.SelectedText += $"[{Log.time}]\t";
            TBOXFS_Logs.SelectedText += $"[{Log.time}]\t";
            TBOX_FullLogs.SelectedText += $"[{Log.time}]\t";

            TBOX_Logs.SelectionColor = Color.Black;
            TBOXFS_Logs.SelectionColor = Color.Black;
            TBOX_Logs.SelectedText += $"[{NormalScreenMessage}]" + Environment.NewLine;
            TBOXFS_Logs.SelectedText += $"[{FullScreenMessage}]" + Environment.NewLine;
            TBOX_FullLogs.SelectedText += $"[{Log.message}]" + Environment.NewLine;
            TBOX_Logs.ScrollToCaret();
            TBOXFS_Logs.ScrollToCaret();
        }
        public void Footer(string Msg, Color color)
        {
            LBL_Footer.Text = Msg;
            LBL_Footer.ForeColor = color;
        }
    }
}
