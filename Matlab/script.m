clear; clc; close all;

syms s t T Vout_ J_ C_ R_ Uc_0_;

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% deklaracja zmiennych:
global V1 V0 J C R H_THR L_THR;
Vcc = 5.028;            % napięcie zasilania
V1 = 4.95;              % napięcie stanu logicznego '1' na wyjściu bufora
V0 = 0.00;              % napięcie stanu logicznego '0' na wyjściu bufora
J = -242E-9;            % prąd Ibias komparatorów
C = 1E-12;              % badana pojemność
R = 200E3;              % rezystancja pomiarowa

H_THR = 2/3*Vcc;        % próg napięcia referencyjnego (H)
L_THR = 1/3*Vcc;        % próg napięcia referencyjnego (L)

time = 200E-6;           % czas wyświetlania wyników obliczeń


F_CPU = 16E6;           % zegar taktujący MK.
max_resolution = 2048;  % maksymalna wartość, do której może zliczyć timer
min_C = 100E-12;        % minimalna pojemność do zmierzenia
max_C = 300E-12;        % maksymalna pojemność do zmierzenia

tick = 1/F_CPU;         % rozdzielczość procesora

% maksymalny czas zliczania - 5% błędu
max_charging_time = max_resolution * tick * 0.95; 

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% układ rozwiązany metodą potencjałów węzłowych:
% funkcja zmiennej zespolonej 's'
VLC(Vout_, J_, C_, R_, Uc_0_, s)=(C_*Uc_0_+Vout_/R_/s-J_/s)/(1/R_ + s*C_);
% przejście do domeny czasu 't' (odwrotna transformata Laplaca)
Vcap = matlabFunction(ilaplace(VLC));
pretty(ilaplace(VLC))

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Wyznaczenie przebiegów czasowych: 
t = 0:1E-9:time;
[VCAP, STHR, SIGNAL, t_charging, t_discharging] = f(t, Vcap);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Zmierzone czasy opadania i narastania napięcia na badanej pojemności:
disp("avg time charging: " + mean(t_charging));
disp("avg time discharging: " + mean(t_discharging));
disp("calc capacity [charging]: " + Cx(t_charging, 'C'));
disp("calc capacity [discharging]: " + Cx(t_discharging, 'D'));

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Wykres przebiegu napięcia na badanej pojemności:
figure
subplot(3, 1, 1);
plot(t, VCAP);
xlabel("Time t [s]");
ylabel("Capacitor Voltage [V]");
title("Przebieg napięcia na badanej pojemności");
axis([0 time L_THR H_THR]);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Przebieg przekroczenia progów napięcia: 
subplot(3, 1, 2);
plot(t, STHR);
xlabel("Time t [s]");
ylabel("Voltage [V]");
title("Przebieg STHR (moment przekroczenia progów napięcia)");
axis([0 time 0 5]);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% przebieg sygnału z MK: 
subplot(3, 1, 3);
plot(t, SIGNAL);
xlabel("Time t [s]");
ylabel("Voltage [V]");
title("Sygnał podawany z MK.");
axis([0 time 0 5]);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% dobór rezystancji pomiarowej:
Res = 10000:5000:550000;
max_time = [];
for x = Res
    max_time(end + 1) = max_charging_time;
end

figure;
subplot(2, 1, 1);
plot(Res*1E-3, charging_time(Res, max_C)*1E6, Res*1E-3, max_time*1E6);
xlabel("Resistance [kOhm]");
ylabel("Charging time [us]");
title("Wykres czasu ładowania wzgl. rezystancji w układzie pomiarowym");
legend('charging time', 'max measure time');

subplot(2, 1, 2);
plot(Res*1E-3, discharging_time(Res, max_C)*1E6, Res*1E-3, max_time*1E6);
xlabel("Resistance [kOhm]");
ylabel("Discharging time [us]");
title("Wykres czasu rozładowania wzg. rezystancji w układzie pomiarowym");
legend('discharging time', 'max measure time');

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Wykresy zmiany czasu ładowania / rozładowania wzgl. badanej pojemności
meas_C = min_C:0.5E-12:max_C;
figure;
subplot(2, 1, 1);
plot(meas_C*1E12, discharging_time(R, meas_C)*1E6, meas_C*1E12, charging_time(R, meas_C)*1E6);
xlabel("Capacity [pF]");
ylabel("Charging / discharging  time [us]");
title("Wykres czasu ładowania / rozładowania wzg. badanej pojemności");
legend("discharging", "charging");

HS1101 = 161E-12:0.5E-12:193E-12;
subplot(2, 1, 2);
plot(HS1101*1E12, discharging_time(R, HS1101)*1E6, HS1101*1E12, charging_time(R, HS1101)*1E6);
xlabel("HS1101 capacity [pF]");
ylabel("Charging / discharging  time [us]");
title("Wykres czasu rozładowania wzg. badanej pojemności");
xlim([161 193]);
legend("discharging", "charging");

disp("HS1101:")
hs_min_c = charging_time(R, HS1101(1));
hs_min_d = discharging_time(R, HS1101(1));
hs_max_c = charging_time(R, HS1101(end));
hs_max_d = discharging_time(R, HS1101(end));
hs_res_c = 100 / floor((hs_max_c - hs_min_c) / tick);
hs_res_d = 100 / floor((hs_max_d - hs_min_d) / tick);

%disp("min charging / discharging time [us]: " + hs_min_c*1E6 + " " + hs_min_d*1E6);
%disp("max charging / discharging time [us]: " + hs_max_c*1E6 + " " + hs_max_d*1E6);
disp("theoretical resolution: " + hs_res_c + " / " + hs_res_d + " % RH");


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Funkcja wyznaczająca przebiegi czasowe:
function [VCAP, STHR, SIGNAL, t_charging, t_discharging] = f(time, Vcap_t)
    global V1 V0 J C R H_THR L_THR;
    VCAP = [];
    STHR = [];
    SIGNAL = [];
    t_charging = [];
    t_discharging = [];
    TIME = 0;
    V = 0;
    state = 0;
    for t = time
        if state == 0
            % charging
            V = Vcap_t(V1, J, C, R, L_THR, t - TIME);
            VCAP(end + 1) = V;
            SIGNAL(end + 1) = V1;
            STHR(end + 1) = V0;
            if V > H_THR
                t_charging(end + 1) = t - TIME;
                TIME = t;
                state = 1;
                STHR(end) = V1;
            end 
        else
            % discharging
            V = Vcap_t(V0, J, C, R, H_THR, t - TIME);
            VCAP(end + 1) = V;
            SIGNAL(end + 1) = V0;
            STHR(end + 1) = V0;
            if V < L_THR
                t_discharging(end + 1) = t - TIME;
                TIME = t;
                state = 0;
                STHR(end) = V1;
            end 
        end
    end 
end

function capacity = Cx(time, char)
    global J R V1 V0 L_THR H_THR;

    T = mean(time);
    if char == 'D' 
        capacity = -T/R/log(1 - (H_THR - L_THR)/(H_THR - V0 + J*R));
    elseif char == 'C'
        capacity = -T/R/log(1 + (H_THR - L_THR)/(L_THR - V1 + J*R));
    else
        capacity = -1;
    end
end

function time = charging_time(Res, Cap)
    global J V1 L_THR H_THR;

    time = [];
    for r = Res
        for c = Cap
            time(end + 1) = -c*r*log(1 + (H_THR - L_THR)/(L_THR - V1 + J*r));
        end
    end 
end 

function time = discharging_time(Res, Cap)
    global J V0 L_THR H_THR;

    time = [];
    for r = Res
        for c = Cap
            time(end + 1) = -c*r*log(1 - (H_THR - L_THR)/(H_THR - V0 + J*r));
        end
    end 
end 