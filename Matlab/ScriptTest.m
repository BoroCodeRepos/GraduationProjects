clear; clc; close all;

syms s t T Vout Jbias Cap Res Ucstart;

%% deklaracja zmiennych:
global A J C R H_THR L_THR H_VOUT L_VOUT;
A = [0 1 0 0];
J = 242E-9;             % prąd Ibias komparatorów
C = 800E-12;            % badana pojemność
R = 191100;             % rezystancja pomiarowa

H_THR = 3.3500;         % próg napięcia referencyjnego (H)
L_THR = 1.6770;         % próg napięcia referencyjnego (L)

H_VOUT = 5.0230;        % napięcie stanu logicznego '1' na wyjściu bufora
L_VOUT = 0.0000;        % napięcie stanu logicznego '0' na wyjściu bufora

TCNT_min = 30;
TCNT_max = power(2, 16 - 3);
[C_min, C_max] = CapacityRange(TCNT_min, TCNT_max);

%% układ rozwiązany metodą potencjałów węzłowych:
Vlaplace(Vout, Jbias, Cap, Res, Ucstart, s) = ...
    (Cap * Ucstart + Vout / Res / s - Jbias / s) / (1 / Res + s * Cap);
VcapFun = matlabFunction(ilaplace(Vlaplace));

%% Wyznaczenie przebiegów czasowych: 
[Vcap, S_THR, Signal, charging_time, discharging_time, time] = ...
    GenerateSignals(VcapFun, 2);

%% Zmierzone czasy opadania i narastania napięcia na badanej pojemności:
disp("charging  time: " + charging_time);
disp("discharging time: " + discharging_time);

%% Wykres przebiegu napięcia na badanej pojemności:
figure
subplot(3, 1, 1);
plot(time, Vcap);
xlabel("Time t [s]"); ylabel("Capacitor Voltage [V]");
title("Przebieg napięcia na badanej pojemności");
axis([0 time(end) L_THR H_THR]);

%% Przebieg przekroczenia progów napięcia: 
subplot(3, 1, 2);
plot(time, S_THR);
xlabel("Time t [s]"); ylabel("Voltage [V]");
title("Przebieg STHR (moment przekroczenia progów napięcia)");
axis([0 time(end) 0 6]);

%% przebieg sygnału z MK: 
subplot(3, 1, 3);
plot(time, Signal);
xlabel("Time t [s]");
ylabel("Voltage [V]");
title("Sygnał podawany z MK.");
axis([0 time(end) 0 6]);

%% dobór rezystancji pomiarowej:
Resistance = 10000:5000:550000;
max_time = [];
for Res = Resistance
    max_time(end + 1) = TCNT_max / 16;
end
ResistanceAxisX = Resistance / 1000;

figure;
subplot(2, 1, 1);
plot(ResistanceAxisX, ChargingTime(Resistance, C) * 1E6, ...
     ResistanceAxisX, max_time);
xlabel("Resistance [kOhm]"); ylabel("Charging time [us]");
title("Wykres czasu ładowania wzgl. rezystancji w układzie pomiarowym");
legend('charging time', 'max measure time');

subplot(2, 1, 2);
plot(ResistanceAxisX, DischargingTime(Resistance, C) * 1E6, ...
     ResistanceAxisX, max_time);
xlabel("Resistance [kOhm]"); ylabel("Discharging time [us]");
title("Wykres czasu rozładowania wzg. rezystancji w układzie pomiarowym");
legend('discharging time', 'max measure time');

%% Wykresy zmiany czasu ładowania / rozładowania wzgl. badanej pojemności
C_Meas = C_min:0.5E-12:C_max;
C_MeasAxisX = C_Meas * 1E12;
figure;
subplot(2, 1, 1);
plot(C_MeasAxisX, DischargingTime(R, C_Meas) * 1E6, ...
     C_MeasAxisX, ChargingTime(R, C_Meas) * 1E6);
xlabel("Capacity [pF]"); ylabel("Charging / discharging  time [us]");
title("Wykres czasu ładowania / rozładowania wzg. badanej pojemności");
legend("discharging", "charging");

HS1101 = 161E-12:0.5E-12:193E-12;
HS1101AxisX = HS1101 * 1E12;
subplot(2, 1, 2);
plot(HS1101AxisX, DischargingTime(R, HS1101) * 1E6, ...
     HS1101AxisX, ChargingTime(R, HS1101) * 1E6);
xlabel("HS1101 capacity [pF]"); ylabel("Charging / discharging time [us]");
title("Wykres czasu rozładowania wzg. badanej pojemności");
xlim([HS1101(1) HS1101(end)]);
legend("discharging", "charging");

disp("HS1101:")
hs_min_c = ChargingTime(R, HS1101(1));
hs_min_d = DischargingTime(R, HS1101(1));
hs_max_c = ChargingTime(R, HS1101(end));
hs_max_d = DischargingTime(R, HS1101(end));
hs_res_c = 100 / floor((hs_max_c - hs_min_c) * 16E6);
hs_res_d = 100 / floor((hs_max_d - hs_min_d) * 16E6);
disp("theoretical resolution: " + hs_res_c + " / " + hs_res_d + " % RH");


% Funkcja wyznaczająca przebiegi czasowe:
function [VCAP, S_THR, SIGNAL, TC, TD, time] = ...
    GenerateSignals(Vcap_t, n_periods)
    global H_VOUT L_VOUT J C R H_THR L_THR;
    VCAP = [];
    S_THR = [];
    SIGNAL = [];
    TIME = 0;
    tc = [];
    td = [];
    V = 0;
    state = 0;
    C
    for t = 0:1E-9:1000E-6
        if state == 0
            % charging
            V = Vcap_t(H_VOUT, J, C, R, L_THR, t - TIME);
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
            V = Vcap_t(L_VOUT, J, C, R, H_THR, t - TIME);
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
    TC = tc(1);
    TD = td(1);
    time = 0:1E-9:(n_periods * (TD + TC));

end
%% Functions
% Funkcja wyznaczająca wartość pojemności
function C = Capacity(Time, ChargingType)
    global J R H_VOUT L_VOUT L_THR H_THR;
    T = mean(Time);
    if ChargingType == 'D' 
        C = -T / R / log(1 - (H_THR - L_THR)/(H_THR - L_VOUT + J * R));
    elseif ChargingType == 'C'
        C = -T / R / log(1 + (H_THR - L_THR)/(L_THR - H_VOUT + J * R));
    else
        C = -1;
    end
end
function C = CapacityFromCharging(Time); C = Capacity(Time, 'C'); end
function C = CapacityFromDischarging(Time); C = Capacity(Time, 'D'); end

% Funkcja wyznaczająca czas ładowania pojemności
function Time = ChargingTime(Resistance, Capacity)
    global J H_VOUT L_THR H_THR;
    Time = [];
    for R = Resistance
        for C = Capacity
            Time(end + 1) = -C * R * ...
                log(1 + (H_THR - L_THR)/(L_THR - H_VOUT + J * R));
        end
    end
end 

% Funkcja wyznaczająca czas rozładowania pojemności
function Time = DischargingTime(Resistance, Capacity)
    global J L_VOUT L_THR H_THR;
    Time = [];
    for R = Resistance
        for C =Capacity
            Time(end + 1) = -C * R * ...
                log(1 - (H_THR - L_THR)/(H_THR - L_VOUT + J * R));
        end
    end
end 

% Wyznaczenie zakresu badanych pojemności
function [C_min, C_max] = CapacityRange(Ticks_min, Ticks_max)
    C_min = max([CapacityFromCharging(Ticks_min / 16E6) ...
        CapacityFromDischarging(Ticks_min / 16E6)]);
    C_max = min([CapacityFromCharging(Ticks_max / 16E6) ...
        CapacityFromDischarging(Ticks_max / 16E6)]);
end

% Funkcja implementująca Oversampling
function Cout = Oversampling(TicksIn)
    temp = 16 - ceiling(log(max(TicksIn), 2));
    bits = 3;
    if temp < 4
        bits = temp;
    end
    N = power(4, bits);
    sum = 0;
    for i = (count(TicksIn) - N + 1):1:(count(TicksIn))
        sum = sum + TicksIn(i);
    end
    result = bitsrl(sum, bits);
    Cout = result * power(2, -bits);
end

% Korekcja wartości pojemności
function Cout = Correction(Cin)
    global A;
    Cout = A(1) * Cin^3 + A(2) * Cin^2 + A(3) * Cin + A(4);
end