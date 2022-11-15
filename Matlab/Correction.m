close all; clear; clc;

format short;
syms x;
% wartości zmierzone
Meas = [
    %126.1832  147.6006 180.2216 196.8221 208.4288 231.3545 250.6058 270.1945 283.4323 307.1494 322.3885
    126.1832  147.6006 180.2216 196.8221 208.4288 231.3545 250.6058 270.1945 283.4323 307.1494 322.3885
] * 1E-12;

% wartości rzeczywiste
Real = [
     95.4870 116.6560 149.5600 165.7383 177.2800 199.3710 220.5100 240.7840 252.6100 276.2010 293.1200
] * 1E-12;


J = -400E-12;          % prąd Ibias komparatorów
R = 741200;           % rezystancja pomiarowa
 
H_THR = 3.3500;       % próg napięcia referencyjnego (H)
L_THR = 1.6770;       % próg napięcia referencyjnego (L)
 
H_VOUT = 5.0230;      % napięcie stanu logicznego '1' na wyjściu bufora
L_VOUT = 0.0000;      % napięcie stanu logicznego '0' na wyjściu bufora
%% wyznaczenie wzoru korekcyjnego za pomocą regresji liniowej
[a, b, Regression] = LinearRegression(Meas, Real);

%% wyznaczenie wzoru korekcyjnego za pomocą interpolacji Lagrange'a
INTERPOLATION_FULL_RANGE = 1;
IntN = [2, 3, 6, 11];
%IntN = [2, 5, 8, 10];
IntX = [Meas(IntN(1)), Meas(IntN(2)), Meas(IntN(3)), Meas(IntN(4))];
IntY = [Real(IntN(1)), Real(IntN(2)), Real(IntN(3)), Real(IntN(4))];
Poly = LagrangeInterpolation(IntX, IntY);
start = find(Meas==IntX(1)); stop = find(Meas==IntX(end));
Interpolation = polyval(Poly, Meas(start:stop));
%% wyniki pomiarowe
fprintf('Linear Regression result:\n');
fprintf('  y = %3.4e * x + %3.4e \n \n', a, b);

fprintf('Lagrange Interpolation: \n');
fprintf('  y = %3.4e x^3 + %3.4e x^2 + %3.4e x + %3.4e \n', ...
    Poly(1), Poly(2), Poly(3), Poly(4));
fprintf('  poly: [%3.4e, %3.4e, %3.4e, %3.4e] \n \n', ...
    Poly(1), Poly(2), Poly(3), Poly(4));

%figure('Name', 'Capacitance Correction');
Real = Real * 1E12; Meas = Meas * 1E12;
Regression = Regression * 1E12;

if INTERPOLATION_FULL_RANGE == 0
    %% wyznaczenie interpolacji po wskazanych punktach
    [Ierr, Ierr_abs] = Error(polyval(Poly, Meas(start:stop)* 1E-12), Real(start:stop)* 1E-12);
    [Int, Ierr, Ierr_abs] = FillVectors(0, Interpolation * 1E12, Ierr, Ierr_abs, start, stop, size(Meas, 2));
    figure('Name', 'Corrections');
    %subplot(3, 1, 1);
    %plot(Meas, Real);
    %subplot(3, 1, 2);
    %plot(Meas, Regression);
    %subplot(3, 1, 3);
    %plot(IntX * 1E12, polyval(Poly, IntX) * 1E12);
    p = plot(Meas, Real, Meas, Regression, IntX * 1E12, polyval(Poly, IntX) * 1E12 - 30);
else
    %% wyznaczenie interpolacji w całym mierzonym zakresie
    Int = polyval(Poly, Meas * 1E-12) * 1E12;
    [Ierr, Ierr_abs] = Error(Int, Real);
    Ierr = Ierr * 1E-12;
    figure('Name', 'Corrections');
    subplot(2, 1, 1);
    plot(Regression,Real);
    xlim([90 300]);
    ylabel('pojemność po korekcji regresją liniową [pF]'); xlabel('rzeczywista pojemność C [pF]');
    subplot(2, 1, 2);
    plot(Int, Real);
    xlim([90 300]);
    ylabel("pojemność po korekcji interpolacją Lagrange'a [pF]"); xlabel('rzeczywista pojemność C [pF]');
    %p = plot(Meas, Real, Meas, Regression, Meas, Int);
end
%p(1).Marker = 'o';
%p(1).MarkerSize = 5;
%p(1).LineWidth = 1.25;
%p(2).LineWidth = 1.25;
%p(3).LineWidth = 2;

%% wyznaczenie maksymalnych błędów korekcji
[Rerr, Rerr_abs] = Error(Regression * 1E-12, Real * 1E-12);
fprintf(' ( Regression ) max error: %3.1f pF  (%3.1f %%) \n', ...
    max(abs(Rerr) * 1E12), max(abs(Rerr_abs)));
fprintf(' ( Interpolation ) max error: %3.1f pF  (%3.1f %%) \n\n', ...
    max(abs(Ierr) * 1E12), max(abs(Ierr_abs)));

err = [Rerr * 1E12; Rerr_abs; Ierr * 1E12; Ierr_abs];

% figure('Name', 'discharging random error');
% subplot(2, 1, 1);
% plot(Real, LosD * 1E6);
% xlim([90 300]);
% xlabel('pojemność rzeczywista [pF]');
% ylabel('losowy błąd bezwzględny [us]');
% subplot(2, 1, 2);
% plot(Real, LosDP * 1E6);
% xlim([90 300]);
% xlabel('pojemność rzeczywista [pF]');
% ylabel('losowu błąd względny [%]');
% figure('Name', 'charging random error');
% subplot(2, 1, 1);
% plot(Real, LosC * 1E6);
% xlim([90 300]);
% xlabel('pojemność rzeczywista [pF]');
% ylabel('losowy błąd bezwzględny [us]');
% subplot(2, 1, 2);
% plot(Real, LosCP * 1E6);
% xlim([90 300]);
% xlabel('pojemność rzeczywista [pF]');
% ylabel('losowu błąd względny [%]');
DisplayErrors(Real, Rerr, Rerr_abs, 'regresion');
DisplayErrors(Real, Ierr,Ierr_abs,'interpol');
%% wyznaczenie tablic pomiarowych
fprintf(' [real value]\t[measurement]\t[regression]\t[interpolation]\t[REG rel err]\t[INT rel err]\t[REG abs err]\t[INT abs err]\n');
for i = 1:1:size(Meas, 2)
    fprintf('   %3.4f\t \t %3.3f\t  %3.4f \t\t   %3.4f\t\t  %3.4f \t  %3.4f \t  %3.4f \t  %3.4f\n', ...
        Real(i), Meas(i), Regression(i), Int(i), err(1, i), err(3, i), err(2, i), err(4,i));
end
Int'
err(3,:)'
err(4,:)'

% Funkcja Regresji liniowej
function [a, b, Y] = LinearRegression(x, y)
    x_avg = mean(x);
    y_avg = mean(y);
    a = sum(y .* (x - x_avg)) / sum((x - x_avg).^2);
    b = y_avg - x_avg * a;
    Y = a * x + b;
end

	function Poly = LagrangeInterpolation(IntX, IntY)
		syms x;
		Int(x) = ...
			IntY(1) * (x - IntX(2))*(x - IntX(3))*(x - IntX(4)) / ...
				((IntX(1) - IntX(2))*(IntX(1) - IntX(3))*(IntX(1) - IntX(4))) + ...
			IntY(2) * (x - IntX(1))*(x - IntX(3))*(x - IntX(4)) / ...
				((IntX(2) - IntX(1))*(IntX(2) - IntX(3))*(IntX(2) - IntX(4))) + ...
			IntY(3) * (x - IntX(1))*(x - IntX(2))*(x - IntX(4)) / ...
				((IntX(3) - IntX(1))*(IntX(3) - IntX(2))*(IntX(3) - IntX(4))) + ...
			IntY(4) * (x - IntX(1))*(x - IntX(2))*(x - IntX(3)) / ...
				((IntX(4) - IntX(1))*(IntX(4) - IntX(2))*(IntX(4) - IntX(3)));
		Poly = sym2poly(Int);
	end
% Funkcja wyznaczająca błędy pomiarowe
function [relative, absolute] = Error(Meas, Real)
    relative = Meas - Real;
    absolute = relative ./ Real * 100;
end

function [RetA, RetB, RetC] = FillVectors(Value, A, B, C, start, stop, maxSize)
    RetA = A; RetB = B; RetC = C;
    for i = 1:1:start - 1
        RetA = [Value RetA];
        RetB = [Value RetB];
        RetC = [Value RetC];
    end
    for i = stop:1:maxSize - 1
        RetA = [RetA Value];
        RetB = [RetB Value];
        RetC = [RetC Value];
    end
end

function DisplayErrors(X, rel, abs, title)
    figure('Name', title);
    subplot(2, 1, 1);
    plot(X, rel * 1E12);
    xlim([90 300]);
    xlabel('pojemność wzorcowa [pF]');
    ylabel('błąd bezwzględny [pF]');
    subplot(2, 1, 2);
    plot(X, abs);
    xlim([90 300]);
    xlabel('pojemność rzeczywista [pF]');
    ylabel('błąd względny [%]');
end

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