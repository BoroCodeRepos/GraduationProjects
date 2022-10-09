using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace CapacitySensor
{
    public static class Matlab
    {
        public static void Generate()
        {
            StringBuilder builder = new StringBuilder();
            DateTime now = DateTime.Now;
            string PathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string FileName = $"Script{now.Hour}{now.Minute}{now.Second}.m";
            string Path = PathDesktop + "\\" + FileName;
            Logs.Create(Logs.From.PC, Logs.Type.Info, 
                $"Matlab Script Generated: Desktop\\{FileName}");
            MainForm.Instance.Footer($"Script Generated: Desktop\\{FileName}", Color.Green);

            AppendStartSection(builder);
            AppendGlobalVariables(builder);
            AppendMeasurementsVariables(builder);
            AppendMainLoop(builder);
            AppendCharts(builder);
            AppendFunctions(builder);

            File.WriteAllLines(Path, builder.ToString().Split('\n'));
            MainForm.Instance.OpenFile(Path);
        }

        public static void AppendStartSection(StringBuilder builder)
        {
            builder.Append("clear; clc; close all;\n \n");
        }

        public static void AppendGlobalVariables(StringBuilder builder)
        {
            string Format = "%% Zmienne globalne\n \n" +
                "global A J C R H_THR L_THR H_VOUT L_VOUT VcapFun;\n" +
                "% współczynniki korekcji \n" +
                "A = [{0:0.0000} {1:0.0000} {2:0.0000} {3:0.0000}];\n" +
                "J = {4:000.##E+00};          % prąd Ibias komparatorów\n" +
                "R = {5:000000};           % rezystancja pomiarowa\n \n" +

                "H_THR = {6:0.0000};       % próg napięcia referencyjnego (H)\n" +
                "L_THR = {7:0.0000};       % próg napięcia referencyjnego (L)\n \n" +

                "H_VOUT = {8:0.0000};      % napięcie stanu logicznego '1' na wyjściu bufora\n" +
                "L_VOUT = {9:0.0000};      % napięcie stanu logicznego '0' na wyjściu bufora\n \n" +

                "TCNT_min = {10:00};        % minimalna liczba taktów zegara podczas pomiaru\n" +
                "                      % ograniczenie spowodowane czasem trwania przerwania\n" +
                "TCNT_max = {11:0000};      % maksymalna liczba taktów zegara podczas pomiaru\n" +
                "                      % ograniczenie przez ilość próbek do oversamplingu\n \n";
            string s = string.Format(Format,
                Calibration.A3,
                Calibration.A2,
                Calibration.A1,
                Calibration.A0,
                Calibration.J,
                Calibration.R_MEAS,
                Calibration.H_THR,
                Calibration.L_THR,
                Calibration.H_VOUT,
                Calibration.L_VOUT,
                Algorithms.TCNT_Min,
                Algorithms.TCNT_Max
            ).Replace(",", ".");
            builder.Append(s);
            builder.Append("%% Rozwiązanie matematyczne układu\n \n");
            builder.Append("syms s t Jbias Cap Res Vout Vcstart Vcstop;\n");
            builder.Append("Flaplace(Cap, Res, s, Vout, Vcstart, Vcstop, Jbias) = ...\n");
            builder.Append("    (Cap * Vcstart + Vout / Res / s - Jbias / s) / (1 / Res + s * Cap);\n");
            builder.Append("VcapFun = ilaplace(Flaplace) == Vcstop;\n \n");

            builder.Append("% maksymalny zakres badanych pojemności\n");
            builder.Append("[C_min, C_max] = CapacityRange(TCNT_min, TCNT_max);\n \n");
        }

        public static void AppendMeasurementsVariables(StringBuilder builder)
        {
            List<string> DischargingProbes = new List<string>();
            List<string> ChargingProbes = new List<string>();
            List<string> Temperature = new List<string>();
            List<string> Humidity = new List<string>();

            List<string> List = CollectLogs();
            bool HasTemp = ParseLogList(List, DischargingProbes, ChargingProbes, 
                Temperature, Humidity);

            builder.Append("%% Measurements Results\n \nDischargingProbes = [\n");
            foreach (string DP in DischargingProbes) builder.Append("    " + DP);
            builder.Append("];\n \n");

            builder.Append("ChargingProbes = [\n");
            foreach (string CP in ChargingProbes) builder.Append("    " + CP);
            builder.Append("];\n \n");

            if (HasTemp && Temperature.Count == ChargingProbes.Count)
            {
                if (Temperature.Count > 1)
                {
                    builder.Append("Temperature = [\n");
                    builder.Append("    " + string.Join(", ", Temperature) + ";\n");
                    builder.Append("]';\n \nHumidity = [\n");
                    builder.Append("    " + string.Join(", ", Humidity) + ";\n");
                    builder.Append("]';\n \n");
                }
                else
                {
                    builder.Append($"Temperature = {Temperature[0]};\n \n");
                    builder.Append($"Humidity = {Humidity[0]};\n \n \n");
                }
            }

            builder.Append("% wyniki pojedynczych pomiarów\n");
            builder.Append("Capacity = [ \n");
            builder.Append("    % pierwszy wiersz - pojemności z czasu ładowania\n");
            builder.Append("    CapacityFromCharging(Oversampling(ChargingProbes));\n");
            builder.Append("    % wiersz drugi - pojemności z czasu rozładowania\n");
            builder.Append("    CapacityFromDischarging(Oversampling(DischargingProbes)) \n");
            builder.Append("] * 1E12;\n \n");
            builder.Append("% średnia wartość pojemności\n");
            builder.Append("CapacityWithoutCorrection = mean(Capacity);\n");
            builder.Append("% pojemność właściwa - po korekcji\n");
            builder.Append("C = Correction(CapacityWithoutCorrection); \n \n");
        }

        public static void AppendMainLoop(StringBuilder builder)
        {
            builder.Append("%% Twój Kod\n \n \n \n");
        }

        public static void AppendCharts(StringBuilder builder)
        {
            builder.Append("%% Wyznaczenie charakterystyk\n");
            builder.Append("PrintResults(Capacity, ChargingProbes, DischargingProbes);\n");
            builder.Append("MeasurementTimePlt();\n");
            builder.Append("if size(Humidity, 1) > 1\n");
            builder.Append("    HumidityPlt(ChargingProbes, DischargingProbes, Humidity, Temperature);\n");
            builder.Append("end\n");
            builder.Append("%GenerateSignals(2);\n \n");
        }

        public static void AppendFunctions(StringBuilder builder)
        {
            builder.Append(
@"
%% Definicje funkcji
% Funkcja wyznaczająca wartość pojemności
function C = CapacityFromCharging(Time)
    global H_THR L_THR H_VOUT J R VcapFun
    syms s t Cap Res Vout Vcstart Vcstop Jbias;
    Cfun = isolate(VcapFun, Cap);
    C = eval(rhs(subs(Cfun, {Res, t, Vout, Vcstart, Vcstop, Jbias}, ...
        {R, Time, H_VOUT, L_THR, H_THR, J})));
end
function C = CapacityFromDischarging(Time)
    global H_THR L_THR L_VOUT J R VcapFun
    syms s t Cap Res Vout Vcstart Vcstop Jbias;
    Cfun = isolate(VcapFun, Cap);
    C = eval(rhs(subs(Cfun, {Res, t, Vout, Vcstart, Vcstop, Jbias}, ...
        {R, Time, L_VOUT, H_THR, L_THR, J})));
end
 
% Funkcja wyznaczająca czas ładowania pojemności
function Time = ChargingTime(Resistance, Capacity)
    global H_THR L_THR H_VOUT J VcapFun
    syms s t Cap Res Vout Vcstart Vcstop Jbias;
    Tfun = isolate(VcapFun, t);
    Time = eval(rhs(subs(Tfun, {Res, Cap, Vout, Vcstart, Vcstop, Jbias}, ...
        {Resistance, Capacity, H_VOUT, L_THR, H_THR, J})));
end
 
% Funkcja wyznaczająca czas rozładowania pojemności
function Time = DischargingTime(Resistance, Capacity)
    global H_THR L_THR L_VOUT J VcapFun
    syms s t Cap Res Vout Vcstart Vcstop Jbias;
    Tfun = isolate(VcapFun, t);
    Time = eval(rhs(subs(Tfun, {Res, Cap, Vout, Vcstart, Vcstop, Jbias}, ...
        {Resistance, Capacity, L_VOUT, H_THR, L_THR, J})));
end
 
% Wyznaczenie maksymalnego zakresu badanych pojemności
function[C_min, C_max] = CapacityRange(Ticks_min, Ticks_max)
    C_min = max([CapacityFromCharging(Ticks_min / 16E6)...
        CapacityFromDischarging(Ticks_min / 16E6)]) ;
    C_max = min([CapacityFromCharging(Ticks_max / 16E6)...
        CapacityFromDischarging(Ticks_max / 16E6)]);
end
 
% Funkcja implementująca Oversampling
function Time = Oversampling(TicksIn)
    temp = 16 - ceil(log2(max(TicksIn)));
    bits = 3;
    if temp < 4
        bits = temp;
    end
    N = power(4, bits);
    sum = 0;
    for i = (size(TicksIn, 2) - N + 1):1:(size(TicksIn, 2))
        sum = sum + TicksIn(i);
    end
    result = bitshift(sum, -bits);
    Time = result * power(2, -bits) / 16E6;
end
 
% Korekcja wartości pojemności
function Cout = Correction(Cin)
    global A;
    C = Cin;
    if Cin > 1
        C = Cin * 1E-12;
    end
    Cout = polyval(A, C) * 1E12;
end
 
% Wyznaczenie temperatury punktu rosy
function DP = DewPoint(T, RH)
    DP = power(RH / 100.0, 0.125) * (112 + 0.9 * T) + 0.1 * T - 112;
end

% Wyświetl komunikaty
function PrintResults(Capacity, ChargingProbes, DischargingProbes)
    global C A;
    CapacityWithoutCorrection = mean(Capacity);
    fprintf('Capacity Average Value (charging):    %3.4f pF     [ %3.4f us ]\n', ...
        mean(Capacity(1,:)), Oversampling(ChargingProbes) * 1E6);
    fprintf('Capacity Average Value (discharging): %3.4f pF     [ %3.4f us ]\n\n', ...
        mean(Capacity(2,:)), Oversampling(DischargingProbes) * 1E6);
    fprintf('    Measured Capacity: %3.4f pF\n\n', CapacityWithoutCorrection);
    fprintf('    Capacity With Correction: %3.4f pF\n\n', C);
    fprintf('Correction Poly: [%1.4e %1.4e %1.4e %1.4e]\n', A);
end

% Wyznaczenie charakterystyki czasu pomiaru
function MeasurementTimePlt()
    global R C;
    figure('Name', 'Time of measurements');
    Caxis = (100:20:300);
    Taxis = [
        ChargingTime(R, Caxis * 1E-12);
        DischargingTime(R, Caxis * 1E-12)
    ] * 1E6;
    subplot(2, 1, 1);
    plot(Caxis, Taxis(1,:), Caxis, Taxis(2,:));
    title(""Measurement time depends on Capacity[R = ""+R * 1E-3+"" kOhm]"");
    legend('ChargingTime', 'DischargingTime');
    xlabel('Capacity [pF]'); ylabel('Time [us]');

    Raxis = 10E3:10E3:1E6;
    Taxis = [
        ChargingTime(Raxis, C * 1E-12);
        DischargingTime(Raxis, C * 1E-12)
    ] *1E6;
    subplot(2, 1, 2);
    plot(Raxis * 1E-3, Taxis(1,:), Raxis * 1E-3, Taxis(2,:));
    title(""Measurement time depends on Resistance  [C = "" + C + "" pF]"");
    legend('ChargingTime', 'DischargingTime');
    xlabel('Resistance [kOhm]'); ylabel('Time [us]');
end

% Wyznaczenie charakterystyki wilgotności(czujnik HS1101)
function HumidityPlt(ChargingProbes, DischargingProbes, Humidity, Temp)
    HS1101_min = 161; HS1101_max = 193;
    Capacity = [];
    DP = [];
    for i = 1:1:size(Humidity, 1)
        DP = [DP DewPoint(Temp(i), Humidity(i))];
        Capacity = [Capacity mean([...
            CapacityFromCharging(Oversampling(ChargingProbes(i,:))) * 1E12...
            CapacityFromDischarging(Oversampling(DischargingProbes(i,:))) * 1E12])];
    end
    CorrectCapacity = Correction(Capacity);
    RH = (CorrectCapacity - HS1101_min)./ (HS1101_max - HS1101_min) * 100.0;
    for i = 1:1:size(RH, 2)
        if RH(i) > 100
            RH(i) = 100;
        elseif RH(i) < 0
            RH(i) = 0;
        end
    end
    figure('Name', 'Humidity');
    subplot(2, 1, 1);
    N = 1:1:size(Humidity, 1);
    plot(N, RH, N, Humidity);
    title('Humidity');
    xlabel('N'); ylabel('RH [%]');
    legend('HS1101', 'SHTC3');
    xlim([1 N(end)]);
    subplot(2, 1, 2);
    plot(N, Temp, N, DP);
    title('Temperature');
    xlabel('N'); ylabel('Temperature [°C]');
    legend('Temperature', 'DewPoint');
    xlim([1 N(end)]);
end

% Funkcja wyznaczająca przebiegi czasowe:
function GenerateSignals(n_periods)
    global H_VOUT L_VOUT J C R H_THR L_THR;
    syms s t Cap Res Vout Vcstart Jbias;
    Vlaplace(Vout, Jbias, Cap, Res, Vcstart, s) = ...
    (Cap * Vcstart + Vout / Res / s - Jbias / s) / (1 / Res + s * Cap);
    Vcap_t = matlabFunction(ilaplace(Vlaplace));
    VCAP = []; S_THR = []; SIGNAL = [];
    TIME = 0; tc = []; td = [];
    V = 0; state = 0;
    step = 1E-9;
    for t = 0:step:1000E-6
        if state == 0
            % charging
            V = Vcap_t(H_VOUT, J, C * 1E-12, R, L_THR, t - TIME);
            VCAP(end + 1) = V;
            SIGNAL(end + 1) = H_VOUT;
            S_THR(end + 1) = L_VOUT;
            if V > H_THR
                tc(end + 1) = t - TIME;
                TIME = t;
                state = 1;
                S_THR(end) = H_VOUT;
            end
        else
            % discharging
            V = Vcap_t(L_VOUT, J, C * 1E-12, R, H_THR, t - TIME);
            VCAP(end + 1) = V;
            SIGNAL(end + 1) = L_VOUT;
            S_THR(end + 1) = L_VOUT;
            if V < L_THR
                td(end + 1) = t - TIME;
                TIME = t;
                state = 0;
                S_THR(end) = H_VOUT;
                if TIME >= n_periods * (tc(1) + td(1))
                    break;
                end
            end
        end
    end
    time = 0:step: (n_periods * (tc(1) + td(1)));

    % Przebieg napięcia na pojemności
    figure('Name', 'Measurement System Signals');
    subplot(3, 1, 1);
    plot(time * 1E6, VCAP(1, 1:size(time, 2)));
    xlabel(""Time [us]""); ylabel(""Capacitor Voltage[V]"");
    title(""Capacitor Voltage Plot"");
    axis([0 time(end) * 1E6 L_THR H_THR]);


    % Przebieg przekroczenia progów napięcia:
    subplot(3, 1, 2);
    plot(time * 1E6, S_THR(1, 1:size(time, 2)));
    xlabel(""Time [us]""); ylabel(""Voltage [V]"");
    title(""Signal of exceeding the voltage threshold [S-THR]"");
    axis([0 time(end) * 1E6 0 6]);

    % przebieg sygnału z MK:
    subplot(3, 1, 3);
    plot(time * 1E6, SIGNAL(1, 1:size(time, 2)));
    xlabel(""Time [us]"");
    ylabel(""Voltage [V]"");
    title(""Microcontroller Signal"");
    axis([0 time(end) * 1E6 0 6]);
end
".Replace(Environment.NewLine, "\n")
             );
        }

        public static List<string> CollectLogs()
        {
            bool IsMulMeasurement = false;
            List<string> List = new List<string>();
            for (int i = Logs.List.Count - 1; i >= 0; i--)
            {
                var msg = Logs.List[i].message;
                if (msg.Contains(((char)Device.Commands.SAMPLES).ToString() + " "))
                {
                    if (!IsMulMeasurement && List.Count == 0)
                    {
                        List.Add(msg);
                        break;
                    }
                    else List.Add(msg);
                }
                if (msg.Contains("Series of measurements ended successfully"))
                {
                    if (!IsMulMeasurement && List.Count == 0) IsMulMeasurement = true;
                    else break;
                }
                if (msg.Contains("Series start of")) break;
            }
            return List;
        }

        public static bool ParseLogList(List<string> List, List<string> DischargingProbes,
            List<string> ChargingProbes, List<string> Temperature, List<string> Humidity)
        {
            bool HasTemp = false;
            for (int i = List.Count - 1; i >= 0; i--)
            {
                List<string> DP = new List<string>();
                List<string> CP = new List<string>();
                string[] Probes = List[i].Split((char)Device.Commands.SAMPLES)[1].Split(' ');
                for (int j = 2; j < Probes.Count() - 1; j++)
                {
                    if (j % 2 == 1) DP.Add(Probes[j + 1]);
                    else CP.Add(Probes[j + 1]);
                }
                DischargingProbes.Add(string.Join(", ", DP) + "; \n");
                ChargingProbes.Add(string.Join(", ", CP) + "; \n");
                if (List[i].Contains((char)Device.Commands.TEMP))
                {
                    HasTemp = true;
                    string[] Temp = List[i].Split((char)Device.Commands.SAMPLES)[0]
                        .Split((char)Device.Commands.TEMP, (char)Device.Commands.RH);
                    Temperature.Add(Temp[1].Trim());
                    Humidity.Add(Temp[2].Trim());
                }
                else if (HasTemp)
                {
                    Temperature.Add(" - ");
                    Humidity.Add(" - ");
                }
            }
            return HasTemp;
        }   
    }
}
