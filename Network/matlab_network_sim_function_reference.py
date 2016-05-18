


% ==== CUSTOM SET-UP (Ai from seed_out) =====
% Expects seed_out to be a 1x2 (numdelays=2) cell of 3x1 (output state)
% matrices.
if isempty(ai2)
    ai2 = zeros(3,2);
    ai2(:,1) = mapminmax_apply(seed_out(:,1), y1_step1_gain, y1_step1_xoffset, y1_step1_ymin);
    ai2(:,2) = mapminmax_apply(seed_out(:,2), y1_step1_gain, y1_step1_xoffset, y1_step1_ymin);
end

% ===== SIMULATION ========

% Dimensions
TS = size(imag_in,2); % timesteps

% Input 1 Delay States
xd1 = mapminmax_apply(seed_in,x1_step1_gain,x1_step1_xoffset,x1_step1_ymin);
xd1 = [xd1 zeros(18,1)];

% Layer Delay States
ad2 = [ai2 zeros(3,1)];

% Allocate Outputs
pred_out = zeros(3,TS);

% Time loop
for ts=1:TS

    % Rotating delay state position
    xdts = mod(ts+1,3)+1;
    adts = mod(ts+1,3)+1;
    
    % Input 1
    xd1(:,xdts) = mapminmax_apply(imag_in(:,ts),x1_step1_gain,x1_step1_xoffset,x1_step1_ymin);
    
    % Layer 1
    tapdelay1 = reshape(xd1(:,mod(xdts-[1 2]-1,3)+1),36,1);
    tapdelay2 = reshape(ad2(:,mod(adts-[1 2]-1,3)+1),6,1);
    a1 = tansig_apply(b1 + IW1_1*tapdelay1 + LW1_2*tapdelay2);
    
    % Layer 2
    ad2(:,adts) = b2 + LW2_1*a1;
    
    % Output 1
    pred_out(:,ts) = mapminmax_reverse(ad2(:,adts),y1_step1_gain,y1_step1_xoffset,y1_step1_ymin);
end

% Final delay states
finalxts = TS+(1: 2);
xits = finalxts(finalxts<=2);
xts = finalxts(finalxts>2)-2;
finalats = TS+(1:2);
ats = mod(finalats-1,3)+1;
xf1 = [seed_in(:,xits) imag_in(:,xts)];
af2 = ad2(:,ats);
end

% ===== MODULE FUNCTIONS ========

% Map Minimum and Maximum Input Processing Function
function y = mapminmax_apply(x,settings_gain,settings_xoffset,settings_ymin)
y = bsxfun(@minus,x,settings_xoffset);
y = bsxfun(@times,y,settings_gain);
y = bsxfun(@plus,y,settings_ymin);
end

% Sigmoid Symmetric Transfer Function
function a = tansig_apply(n)
a = 2 ./ (1 + exp(-2*n)) - 1;
end

% Map Minimum and Maximum Output Reverse-Processing Function
function x = mapminmax_reverse(y,settings_gain,settings_xoffset,settings_ymin)
x = bsxfun(@minus,y,settings_ymin);
x = bsxfun(@rdivide,x,settings_gain);
x = bsxfun(@plus,x,settings_xoffset);
end
