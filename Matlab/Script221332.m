clear; clc; close all;
 
%% Zmienne globalne
 
global A J C R H_THR L_THR H_VOUT L_VOUT VcapFun;
% współczynniki korekcji 
A = [-1.4142e+19, 9.2461e+09, -9.7074e-01, 1.0698e-10] ;
J = -400E-12;          % prąd Ibias komparatorów
R = 191100;           % rezystancja pomiarowa
 
H_THR = 3.3500;       % próg napięcia referencyjnego (H)
L_THR = 1.6770;       % próg napięcia referencyjnego (L)
 
H_VOUT = 5.0230;      % napięcie stanu logicznego '1' na wyjściu bufora
L_VOUT = 0.0000;      % napięcie stanu logicznego '0' na wyjściu bufora
 
TCNT_min = 30;        % minimalna liczba taktów zegara podczas pomiaru
                      % ograniczenie spowodowane czasem trwania przerwania
TCNT_max = 8192;      % maksymalna liczba taktów zegara podczas pomiaru
                      % ograniczenie przez ilość próbek do oversamplingu
 
%% Rozwiązanie matematyczne układu
 
syms s t Jbias Cap Res Vout Vcstart Vcstop;
Flaplace(Cap, Res, s, Vout, Vcstart, Vcstop, Jbias) = ...
    (Cap * Vcstart + Vout / Res / s - Jbias / s) / (1 / Res + s * Cap);
VcapFun = ilaplace(Flaplace) == Vcstop;
 
% maksymalny zakres badanych pojemności
[C_min, C_max] = CapacityRange(TCNT_min, TCNT_max);
 
%% Measurements Results
 
DischargingProbes = [
    456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    456, 456, 456, 456, 456, 456, 457, 457, 457, 456, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 457, 457, 457, 457, 456, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457; 
    457, 456, 456, 456, 456, 456, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 457, 456, 457, 456, 456, 456, 456, 456, 456, 457, 457, 456, 456, 456, 456, 457, 457, 457, 456, 457, 457, 456, 457, 457, 457, 457, 456, 457, 457, 457, 456, 457, 456, 456, 457, 457, 457, 457, 457, 457, 456, 457, 456, 457, 457, 457, 457, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 456, 457, 457, 456, 456, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 456, 456, 456, 456, 456, 456, 456, 456, 457, 457, 456, 456, 456, 456, 456, 456, 456, 457, 456, 456, 456, 456, 456, 456, 457, 457, 457, 456, 457, 457, 457, 457, 456, 457, 456, 457, 457, 457, 457, 457, 456, 457, 456, 456, 457, 457, 457, 456, 457, 457, 457, 456, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 457, 456, 456, 457, 456, 456, 456, 457, 456, 457, 456, 456, 456, 456, 456, 456, 456, 456, 457, 456, 456, 456, 456, 456, 457, 457, 457, 457, 456, 456, 456, 457, 457, 457, 456, 457; 
    457, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 456, 457, 457, 456, 457, 456, 456, 457, 456, 456, 457, 457, 456, 456, 457, 457, 457, 456, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457, 457, 457, 456, 457, 457, 457, 456, 457, 457, 457, 457, 457, 456, 457, 457, 457, 457, 457, 457, 457; 
];
 
ChargingProbes = [
    457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
    458, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457, 457; 
];
 
Temperature = [
    24.2, 24.2, 24.2, 24.2, 24.2, 24.2, 24.2, 24.2, 24.2, 24.2;
]';
 
Humidity = [
    46.1, 46.1, 46.1, 46.1, 46.2, 46.2, 46.1, 46.1, 46.1, 46.1;
]';
 
% wyniki pojedynczych pomiarów
Capacity = [ 
    % pierwszy wiersz - pojemności z czasu ładowania
    CapacityFromCharging(Oversampling(ChargingProbes));
    % wiersz drugi - pojemności z czasu rozładowania
    CapacityFromDischarging(Oversampling(DischargingProbes)) 
] * 1E12;
 
% średnia wartość pojemności
CapacityWithoutCorrection = mean(Capacity);
% pojemność właściwa - po korekcji
C = Correction(CapacityWithoutCorrection); 
 
%% Twój Kod
 
 
 
%% Wyznaczenie charakterystyk
PrintResults(Capacity, ChargingProbes, DischargingProbes);
MeasurementTimePlt();
if size(Humidity, 1) > 1
    HumidityPlt(ChargingProbes, DischargingProbes, Humidity, Temperature);
end
GenerateSignals(2);
 

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
    title("Measurement time depends on Capacity[R = "+R * 1E-3+" kOhm]");
    legend('ChargingTime', 'DischargingTime');
    xlabel('Capacity [pF]'); ylabel('Time [us]');

    Raxis = 10E3:10E3:1E6;
    Taxis = [
        ChargingTime(Raxis, C * 1E-12);
        DischargingTime(Raxis, C * 1E-12)
    ] *1E6;
    subplot(2, 1, 2);
    plot(Raxis * 1E-3, Taxis(1,:), Raxis * 1E-3, Taxis(2,:));
    title("Measurement time depends on Resistance  [C = " + C + " pF]");
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
    xlabel("Time [us]"); ylabel("Capacitor Voltage[V]");
    title("Capacitor Voltage Plot");
    axis([0 time(end) * 1E6 L_THR H_THR]);


    % Przebieg przekroczenia progów napięcia:
    subplot(3, 1, 2);
    plot(time * 1E6, S_THR(1, 1:size(time, 2)));
    xlabel("Time [us]"); ylabel("Voltage [V]");
    title("Signal of exceeding the voltage threshold [S-THR]");
    axis([0 time(end) * 1E6 0 6]);

    % przebieg sygnału z MK:
    subplot(3, 1, 3);
    plot(time * 1E6, SIGNAL(1, 1:size(time, 2)));
    xlabel("Time [us]");
    ylabel("Voltage [V]");
    title("Microcontroller Signal");
    axis([0 time(end) * 1E6 0 6]);
end

