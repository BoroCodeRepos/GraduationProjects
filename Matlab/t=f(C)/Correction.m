close all; clear; clc;

format short;
syms x;

% wartości zmierzone
Meas = [
    126.1832 147.6006 180.2216 196.8221 208.4288 231.3545 250.6058 270.1945 283.4323 310.1494 322.3885
] * 1E-12;

% wartości rzeczywiste
Real = [
     98.4870 119.6560 149.5600 164.7383 177.2800 199.3710 221.8100 240.7840 252.6100 274.7010 293.1200
] * 1E-12;

%% wyznaczenie wzoru korekcyjnego za pomocą regresji liniowej
[a, b, Regression] = LinearRegression(Meas, Real);

%% wyznaczenie wzoru korekcyjnego za pomocą interpolacji Lagrange'a
INTERPOLATION_FULL_RANGE = 0;
IntX = [Meas(2), Meas(3), Meas(7), Meas(end-2)];
IntY = [Real(2), Real(3), Real(7), Real(end-2)];
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


figure('Name', 'Capacitance Correction');
Real = Real * 1E12; Meas = Meas * 1E12;
Regression = Regression * 1E12;

if INTERPOLATION_FULL_RANGE == 0
    %% wyznaczenie interpolacji po wskazanych punktach
    [Ierr, Ierr_abs] = Error(polyval(Poly, Meas(start:stop)* 1E-12), Real(start:stop)* 1E-12);
    [Int, Ierr, Ierr_abs] = FillVectors(0, Interpolation * 1E12, Ierr, Ierr_abs, start, stop, size(Meas, 2));
    p = plot(Meas, Real, Meas, Regression, IntX * 1E12, polyval(Poly, IntX) * 1E12);
else
    %% wyznaczenie interpolacji w całym mierzonym zakresie
    Int = polyval(Poly, Meas * 1E-12) * 1E12;
    [Ierr, Ierr_abs] = Error(Int, Real);
    p = plot(Meas, Real, Meas, Regression, Meas, Int);
end
p(1).Marker = 'o';
p(1).MarkerSize = 5;
p(1).LineWidth = 1.25;
p(2).LineWidth = 1.25;
p(3).LineWidth = 2;
title('Capacitance Correction');
xlabel('Measured capacity [pF]'); ylabel('Real capacity [pF]');
legend('Real capacity', 'Linear correction', 'Interpolation')

%% wyznaczenie maksymalnych błędów korekcji
[Rerr, Rerr_abs] = Error(Regression * 1E-12, Real * 1E-12);
fprintf(' ( Regression ) max error: %3.1f pF  (%3.1f %%) \n', ...
    max(Rerr * 1E12), max(Rerr_abs));
fprintf(' ( Interpolation ) max error: %3.1f pF  (%3.1f %%) \n\n', ...
    max(Ierr * 1E12), max(Ierr_abs));

err = [Rerr * 1E12; Rerr_abs; Ierr * 1E12; Ierr_abs];

%% wyznaczenie tablic pomiarowych
fprintf(' [real value]\t[measurement]\t[regression]\t[interpolation]\t[REG rel err]\t[INT rel err]\t[REG abs err]\t[INT abs err]\n');
for i = 1:1:size(Meas, 2)
    fprintf('   %3.4f\t \t  %3.4f \t\t  %3.4f \t\t   %3.4f\t\t  %3.4e \t  %3.4e \t  %3.4e \t  %3.4e\n', ...
        Real(i), Meas(i), Regression(i), Int(i), err(1, i), err(3, i), err(2, i), err(4,i));
end

% Funkcja Regresji liniowej
function [a, b, Y] = LinearRegression(x, y)
    x_avg = mean(x);
    y_avg = mean(y);
    a = sum(y .* (x - x_avg)) / sum((x - x_avg).^2);
    b = y_avg - x_avg * a;
    Y = a * x + b;
end
% Funkcja Interpolacji Lagrange'a
function Poly = LagrangeInterpolation(IntX, IntY)
    syms x;
    Int(x) = ...
        IntY(1) * (x - IntX(2))*(x - IntX(3))*(x - IntX(4)) / ((IntX(1) - IntX(2))*(IntX(1) - IntX(3))*(IntX(1) - IntX(4))) + ...
        IntY(2) * (x - IntX(1))*(x - IntX(3))*(x - IntX(4)) / ((IntX(2) - IntX(1))*(IntX(2) - IntX(3))*(IntX(2) - IntX(4))) + ...
        IntY(3) * (x - IntX(1))*(x - IntX(2))*(x - IntX(4)) / ((IntX(3) - IntX(1))*(IntX(3) - IntX(2))*(IntX(3) - IntX(4))) + ...
        IntY(4) * (x - IntX(1))*(x - IntX(2))*(x - IntX(3)) / ((IntX(4) - IntX(1))*(IntX(4) - IntX(2))*(IntX(4) - IntX(3)));
    Poly = sym2poly(Int);
end
% Funkcja wyznaczająca błędy pomiarowe
function [relative, absolute] = Error(Meas, Real)
    relative = abs(Meas - Real);
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