# network.py

import numpy as np


class Network2D_18_3(object):
    def __init__(self,
            x1_step1_xoffset,
            x1_step1_gain,
            x1_step1_ymin,
            b1,
            IW1_1,
            LW1_2,
            b2,
            LW2_1,
            y1_step1_ymin,
            y1_step1_gain,
            y1_step1_xoffset,
            clear_output_row = -1):
        self.x1_step1_xoffset = x1_step1_xoffset
        self.x1_step1_gain = x1_step1_gain
        self.x1_step1_ymin = x1_step1_ymin
        self.b1 = b1
        self.IW1_1 = IW1_1
        self.LW1_2 = LW1_2
        self.b2 = b2
        self.LW2_1 = LW2_1
        self.y1_step1_ymin = y1_step1_ymin
        self.y1_step1_gain = y1_step1_gain
        self.y1_step1_xoffset = y1_step1_xoffset
        self.clear_output_row = clear_output_row

    '''
    imag_in:  NxTS numpy matrix containing imagined input contraction states.
    seed_in:  Nx3 numpy matrix containing input delay states.
      where N is the number of inputs the network was trained for.
    seed_out: Mx3 numpy matrix containing output delay states.
      where M is the number of outputs the network was trained for.

    pred_out: 18xTS numpy matrix containing predicted output coordinates.
    '''
    def predict(self, seed_in, seed_out, imag_in):

        # First, calculate ai2, the initial output layer feedback values.
        # These values are just the output layer weights applied to
        # the output seed values.
        self.ai2 = np.zeros([2,3])
        self.ai2[:,0:1] = mapminmax_apply(seed_out[1:,0:1], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)
        self.ai2[:,1:2] = mapminmax_apply(seed_out[1:,1:2], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)
        self.ai2[:,2:3] = mapminmax_apply(seed_out[1:,2:3], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)

        # Now we have what we need to perform the prediction.
        # Calculate number of timesteps based on input.
        TS = imag_in.shape[1]

        # Input layer delay states
        xd1 = np.zeros(seed_in.shape)
        for col in range(seed_in.shape[1]):
            xd1[:,col:col+1] = mapminmax_apply(seed_in[:,col:col+1], self.x1_step1_xoffset[:,0:1], self.x1_step1_gain[:,0:1], self.x1_step1_ymin)
        xd1 = np.hstack((xd1, np.zeros([18,1])))

        # Layer delay states
        ad2 = np.hstack((self.ai2, np.zeros([2,1])))

        # Output
        pred_out = np.zeros([3, TS])


        # Prediction time-loop
        for ts in range(1, TS+1):
            # Rotating delay state position
            xdts = np.mod(ts+2,4)+1
            adts = np.mod(ts+2,4)+1

            # Input layer
            xd1[:, xdts-1:xdts] = mapminmax_apply(imag_in[:,ts-1:ts], self.x1_step1_xoffset[:,0:1], self.x1_step1_gain[:,0:1], self.x1_step1_ymin)

            # Layer 1 (hidden layers)
            # (reshape in numpy follows 'C' order by default, this code is expecting Matlab 'Fortran' order scanning, hence order='F' and .copy().)
            tapdelay1 = xd1[:,np.mod(xdts-np.array([1,2,3])-1, 4)].reshape(54,1,order='F').copy()
            tapdelay2 = ad2[:,np.mod(adts-np.array([1,2,3])-1, 4)].reshape(6,1,order='F').copy()
            # OLD: APPLY TANSIG
            # a1 = tansig_apply(self.b1 + np.dot(self.IW1_1, tapdelay1) + np.dot(self.LW1_2, tapdelay2));
            a1 = self.b1 + np.dot(self.IW1_1, tapdelay1) + np.dot(self.LW1_2, tapdelay2);


            # Layer 2 (output layer in)
            ad2[:,adts-1:adts] = self.b2 + np.dot(self.LW2_1, a1);

            # Output layer (reverse-mapped out)
            temp = mapminmax_reverse(ad2[:,adts-1:adts], self.y1_step1_xoffset[:,0:1], self.y1_step1_gain[:,0:1], self.y1_step1_ymin)
            if (self.clear_output_row != -1):
                pred_out[:, ts-1:ts] = insert_row(self.clear_output_row, 0, temp)
            else:
                pred_out[:,ts-1:ts] = temp


        # Final delay states 
        final_timesteps = TS+np.arange(3)
        f_ats = np.mod(final_timesteps,4)
        cntu_seed_out = mapminmax_reverse(ad2[:, f_ats[0]:f_ats[2]], self.y1_step1_xoffset[:,0:2], self.y1_step1_gain[:,0:2], self.y1_step1_ymin)
        cntu_seed_in = imag_in[:,TS-3:]
        # ^ cntu_seed_in and cntu_seed_out could be
        # returned as well as pred_out to allow this function
        # to be called consecutively and predict even further
        # out. (But it will become less accurate over time.)

        return pred_out


class Network_18_3(object):
    def __init__(self,
            x1_step1_xoffset,
            x1_step1_gain,
            x1_step1_ymin,
            b1,
            IW1_1,
            LW1_2,
            b2,
            LW2_1,
            y1_step1_ymin,
            y1_step1_gain,
            y1_step1_xoffset,
            clear_output_row = -1):
        self.x1_step1_xoffset = x1_step1_xoffset
        self.x1_step1_gain = x1_step1_gain
        self.x1_step1_ymin = x1_step1_ymin
        self.b1 = b1
        self.IW1_1 = IW1_1
        self.LW1_2 = LW1_2
        self.b2 = b2
        self.LW2_1 = LW2_1
        self.y1_step1_ymin = y1_step1_ymin
        self.y1_step1_gain = y1_step1_gain
        self.y1_step1_xoffset = y1_step1_xoffset
        self.clear_output_row = clear_output_row

    '''
    imag_in:  NxTS numpy matrix containing imagined input contraction states.
    seed_in:  Nx3 numpy matrix containing input delay states.
      where N is the number of inputs the network was trained for.
    seed_out: Mx3 numpy matrix containing output delay states.
      where M is the number of outputs the network was trained for.

    pred_out: 18xTS numpy matrix containing predicted output coordinates.
    '''
    def predict(self, seed_in, seed_out, imag_in):

        # First, calculate ai2, the initial output layer feedback values.
        # These values are just the output layer weights applied to
        # the output seed values.
        self.ai2 = np.zeros([3,3])
        self.ai2[:,0:1] = mapminmax_apply(seed_out[:,0:1], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)
        self.ai2[:,1:2] = mapminmax_apply(seed_out[:,1:2], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)
        self.ai2[:,2:3] = mapminmax_apply(seed_out[:,2:3], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)

        # Now we have what we need to perform the prediction.
        # Calculate number of timesteps based on input.
        TS = imag_in.shape[1]

        # Input layer delay states
        xd1 = np.zeros(seed_in.shape)
        for col in range(seed_in.shape[1]):
            xd1[:,col:col+1] = mapminmax_apply(seed_in[:,col:col+1], self.x1_step1_xoffset[:,0:1], self.x1_step1_gain[:,0:1], self.x1_step1_ymin)
        xd1 = np.hstack((xd1, np.zeros([18,1])))

        # Layer delay states
        ad2 = np.hstack((self.ai2, np.zeros([3,1])))

        # Output
        pred_out = np.zeros([3, TS])


        # Prediction time-loop
        for ts in range(1, TS+1):
            # Rotating delay state position
            xdts = np.mod(ts+2,4)+1
            adts = np.mod(ts+2,4)+1

            # Input layer
            xd1[:, xdts-1:xdts] = mapminmax_apply(imag_in[:,ts-1:ts], self.x1_step1_xoffset[:,0:1], self.x1_step1_gain[:,0:1], self.x1_step1_ymin)

            # Layer 1 (hidden layers)
            # (reshape in numpy follows 'C' order by default, this code is expecting Matlab 'Fortran' order scanning, hence order='F' and .copy().)
            tapdelay1 = xd1[:,np.mod(xdts-np.array([1,2,3])-1, 4)].reshape(54,1,order='F').copy()
            tapdelay2 = ad2[:,np.mod(adts-np.array([1,2,3])-1, 4)].reshape(9,1,order='F').copy()
            # OLD: APPLY TANSIG
            # a1 = tansig_apply(self.b1 + np.dot(self.IW1_1, tapdelay1) + np.dot(self.LW1_2, tapdelay2));
            a1 = self.b1 + np.dot(self.IW1_1, tapdelay1) + np.dot(self.LW1_2, tapdelay2);


            # Layer 2 (output layer in)
            ad2[:,adts-1:adts] = self.b2 + np.dot(self.LW2_1, a1);

            # Output layer (reverse-mapped out)
            temp = mapminmax_reverse(ad2[:,adts-1:adts], self.y1_step1_xoffset[:,0:1], self.y1_step1_gain[:,0:1], self.y1_step1_ymin)
            if (self.clear_output_row != -1):
                pred_out[:, ts-1:ts] = remove_row(self.clear_output_row, 0, temp)
            else:
                pred_out[:,ts-1:ts] = temp


        # Final delay states 
        final_timesteps = TS+np.arange(3)
        f_ats = np.mod(final_timesteps,4)
        cntu_seed_out = mapminmax_reverse(ad2[:, f_ats[0]:f_ats[2]], self.y1_step1_xoffset[:,0:2], self.y1_step1_gain[:,0:2], self.y1_step1_ymin)
        cntu_seed_in = imag_in[:,TS-3:]
        # ^ cntu_seed_in and cntu_seed_out could be
        # returned as well as pred_out to allow this function
        # to be called consecutively and predict even further
        # out. (But it will become less accurate over time.)

        return pred_out


class Network_18_18(object):
    def __init__(self,
            x1_step1_xoffset,
            x1_step1_gain,
            x1_step1_ymin,
            b1,
            IW1_1,
            LW1_2,
            b2,
            LW2_1,
            y1_step1_ymin,
            y1_step1_gain,
            y1_step1_xoffset):
        self.x1_step1_xoffset = x1_step1_xoffset
        self.x1_step1_gain = x1_step1_gain
        self.x1_step1_ymin = x1_step1_ymin
        self.b1 = b1
        self.IW1_1 = IW1_1
        self.LW1_2 = LW1_2
        self.b2 = b2
        self.LW2_1 = LW2_1
        self.y1_step1_ymin = y1_step1_ymin
        self.y1_step1_gain = y1_step1_gain
        self.y1_step1_xoffset = y1_step1_xoffset

    '''
    imag_in:  NxTS numpy matrix containing imagined input contraction states.
    seed_in:  Nx3 numpy matrix containing input delay states.
      where N is the number of inputs the network was trained for.
    seed_out: Mx3 numpy matrix containing output delay states.
      where M is the number of outputs the network was trained for.

    pred_out: 18xTS numpy matrix containing predicted output coordinates.
    '''
    def predict(self, seed_in, seed_out, imag_in):

        # First, calculate ai2, the initial output layer feedback values.
        # These values are just the output layer weights applied to
        # the output seed values.
        self.ai2 = np.zeros([18,3])
        self.ai2[:,0:1] = mapminmax_apply(seed_out[:,0:1], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)
        self.ai2[:,1:2] = mapminmax_apply(seed_out[:,1:2], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)
        self.ai2[:,2:3] = mapminmax_apply(seed_out[:,2:3], self.y1_step1_xoffset, self.y1_step1_gain, self.y1_step1_ymin)

        # Now we have what we need to perform the prediction.
        # Calculate number of timesteps based on input.
        TS = imag_in.shape[1]

        # Input layer delay states
        xd1 = np.zeros(seed_in.shape)
        for col in range(seed_in.shape[1]):
            xd1[:,col:col+1] = mapminmax_apply(seed_in[:,col:col+1], self.x1_step1_xoffset[:,0:1], self.x1_step1_gain[:,0:1], self.x1_step1_ymin)
        xd1 = np.hstack((xd1, np.zeros([18,1])))

        # Layer delay states
        ad2 = np.hstack((self.ai2, np.zeros([18,1])))

        # Output
        pred_out = np.zeros([18, TS])


        # Prediction time-loop
        for ts in range(1, TS+1):
            # Rotating delay state position
            xdts = np.mod(ts+2,4)+1
            adts = np.mod(ts+2,4)+1

            # Input layer
            xd1[:, xdts-1:xdts] = mapminmax_apply(imag_in[:,ts-1:ts], self.x1_step1_xoffset[:,0:1], self.x1_step1_gain[:,0:1], self.x1_step1_ymin)

            # Layer 1 (hidden layers)
            # (reshape in numpy follows 'C' order by default, this code is expecting Matlab 'Fortran' order scanning, hence order='F' and .copy().)
            tapdelay1 = xd1[:,np.mod(xdts-np.array([1,2,3])-1, 4)].reshape(54,1,order='F').copy()
            tapdelay2 = ad2[:,np.mod(adts-np.array([1,2,3])-1, 4)].reshape(54,1,order='F').copy()
            # OLD: APPLY TANSIG
            # a1 = tansig_apply(self.b1 + np.dot(self.IW1_1, tapdelay1) + np.dot(self.LW1_2, tapdelay2));
            a1 = self.b1 + np.dot(self.IW1_1, tapdelay1) + np.dot(self.LW1_2, tapdelay2);


            # Layer 2 (output layer in)
            ad2[:,adts-1:adts] = self.b2 + np.dot(self.LW2_1, a1);

            # Output layer (reverse-mapped out)
            pred_out[:,ts-1:ts] = mapminmax_reverse(ad2[:,adts-1:adts], self.y1_step1_xoffset[:,0:1], self.y1_step1_gain[:,0:1], self.y1_step1_ymin)

        # Final delay states 
        final_timesteps = TS+np.arange(3)
        f_ats = np.mod(final_timesteps,4)
        cntu_seed_out = mapminmax_reverse(ad2[:, f_ats[0]:f_ats[2]], self.y1_step1_xoffset[:,0:2], self.y1_step1_gain[:,0:2], self.y1_step1_ymin)
        cntu_seed_in = imag_in[:,TS-3:]
        # ^ cntu_seed_in and cntu_seed_out could be
        # returned as well as pred_out to allow this function
        # to be called consecutively and predict even further
        # out. (But it will become less accurate over time.)

        return pred_out


# Static Network Functions #

def mapminmax_apply(x, xoffset, gain, ymin):
    y = x - xoffset
    y = y * gain
    y = y + ymin
    return y

def mapminmax_reverse(y, xoffset, gain, ymin):
    x = y - ymin
    x = x / gain
    x = x + xoffset
    return x

def remove_row(row_num, clear_value, orig_matrix):
    return np.vstack((orig_matrix[0:row_num,:],
                      np.ones([1, orig_matrix.shape[1]]) * clear_value,
                      orig_matrix[row_num+1:,:]))

def insert_row(row_num, default_value, orig_matrix):
    return np.vstack((orig_matrix[0:row_num,:],
                      np.ones([1, orig_matrix.shape[1]]) * default_value,
                      orig_matrix[row_num:,:]))

def tansig_apply(n): # unused: switched to purelin (linear, no function applied)
    return 2. / (1 + np.exp(-2*n)) - 1

import v6LN_3_3_net
def main():
    # Test code using v6_3_3_net.py network variables.
    seed_in = np.array([[ 0.91715664,0.91771704,0.9182415],[
            2.16766787, 2.16793728, 2.16818953],[
            1.39355671, 1.39298391, 1.39244688],[
            1.39481807, 1.39424837, 1.39371502],[
            2.16928649, 2.16955996, 2.16981626],[
            0.91793245, 0.91849262, 0.9190169],[
            2.27610826, 2.27611065, 2.27611399],[
            1.76563048, 1.76563346, 1.76563811],[
            1.22911417, 1.22911561, 1.22911906],[
            1.22420692, 1.22420394, 1.22420347],[
            1.84451878, 1.84451473, 1.84451234],[
            2.27138567, 2.27138162, 2.27137995],[
            2.71608853, 2.71608973, 2.71609354],[
            2.21054053, 2.21054339, 2.21054959],[
            1.74631333, 1.74631429, 1.74631464],[
            1.74284887, 1.74284792, 1.74284816],[
            2.20434737, 2.20434165, 2.2043383],[
            2.71334267, 2.71334004, 2.71333814 ]])
        
    seed_out = np.array([[ -8.73565674e-04, -8.97407532e-04, -9.20295715e-04],[
            -2.09312773e+00, -2.09686136e+00, -2.10035706e+00],[
             2.76373863e+00, 2.76322937e+00, 2.76275349e+00 ]])


    imag_in = np.array([[
             0.9182415  , 0.91771704 , 0.91715664 , 0.91656005 , 0.91592753 , 0.91525984 ,  0.91455746  ,  0.91382086 ,  0.91305023 ,  0.91224569 ,  0.91140759 ,  0.9105373  , 0.90963501  , 0.90870231  , 0.90773958  , 0.90674782  , 0.90572774  , 0.90468055  , 0.90360749  , 0.90250957],[
             2.16818953 , 2.16793728 , 2.16766787 , 2.16737962 , 2.16707373 , 2.16674995 ,  2.1664083   ,  2.16604996 ,  2.16567492 ,  2.16528225 ,  2.16487241 ,  2.16444802 ,  2.16400838 ,  2.16355419 ,  2.16308713 ,  2.16260695 ,  2.16211462 ,  2.16161036 ,  2.16109538 ,  2.16056895],[
             1.39244688 , 1.39298391 , 1.39355671 , 1.39416444 , 1.39480793 , 1.39548612 ,  1.3961997   ,  1.39694798 ,  1.39773059 ,  1.39854765 ,  1.39939666 ,  1.40028024 ,  1.40119529 ,  1.40214431 ,  1.40312493 ,  1.40413642 ,  1.40517795 ,  1.40624917 ,  1.40734923 ,  1.40847576],[
             1.39371502 , 1.39424837 , 1.39481807 , 1.39542401 , 1.39606595 , 1.39674497 ,  1.39746022  ,  1.3982116  ,  1.39899886 ,  1.39982176 ,  1.40067697 ,  1.40156639 ,  1.40248787 ,  1.40344179 ,  1.4044261  ,  1.40543962 ,  1.40648174 ,  1.40755153 ,  1.40864885 ,  1.40977073],[
             2.16981626 , 2.16955996 , 2.16928649 , 2.1689961  , 2.1686883  , 2.16836476 ,  2.16802549  ,  2.16767192 ,  2.16730189 ,  2.16691685 ,  2.16651559 ,  2.16609907 ,  2.16566753 ,  2.1652205  ,  2.16475868 ,  2.16428137 ,  2.16378927 ,  2.16328335 ,  2.16276383 ,  2.16223192],[
             0.9190169  , 0.91849262 , 0.91793245 , 0.91733629 , 0.91670471 , 0.9160381  ,  0.9153372   , 0.91460234  , 0.91383374  , 0.91303217  , 0.91219646  , 0.91132879  , 0.91042894  , 0.90949863  , 0.90853798  , 0.90754807  , 0.90652901  , 0.90548283  , 0.90440977  , 0.90331203],[
             2.27611399 , 2.27611065 , 2.27610826 , 2.27610564 , 2.27610373 , 2.27610278 ,  2.27610183  ,  2.2761004  ,  2.27610111 ,  2.27610183 ,  2.27610421 ,  2.27610612 ,  2.27610779 ,  2.2761097  ,  2.27611041 ,  2.2761116  ,  2.27611232 ,  2.27611279 ,  2.27611208 ,  2.27611232],[
             1.76563811 , 1.76563346 , 1.76563048 , 1.76562643 , 1.76562202 , 1.76561713 ,  1.76561153  ,  1.7656045  ,  1.76559937 ,  1.76559162 ,  1.76558685 ,  1.76558053 ,  1.76557565 ,  1.76557064 ,  1.76556623 ,  1.76556385 ,  1.76556289 ,  1.76556253 ,  1.76556289 ,  1.76556563],[
             1.22911906 , 1.22911561 , 1.22911417 , 1.22911155 , 1.22910857 , 1.22910452 ,  1.22909915  ,  1.22909176 ,  1.22908628 ,  1.22907841 ,  1.22907245 ,  1.22906506 ,  1.22905874 ,  1.22905278 ,  1.22904813 ,  1.22904468 ,  1.22904325 ,  1.22904313 ,  1.22904384 ,  1.2290467 ],[
             1.22420347 , 1.22420394 , 1.22420692 , 1.22421062 , 1.2242142  , 1.22421837 ,  1.22422349  ,  1.22422838 ,  1.22423565 ,  1.22424221 ,  1.22425044 ,  1.22425759 ,  1.22426462 ,  1.22427022 ,  1.22427428 ,  1.22427762 ,  1.22428012 ,  1.2242806  , 1.22427881  , 1.2242775 ],[
             1.84451234 , 1.84451473 , 1.84451878 , 1.84452319 , 1.84452772 , 1.84453225 ,  1.84453738  ,  1.84454191 ,  1.84454918 ,  1.84455383 ,  1.8445611  ,  1.84456682 ,  1.84457242 ,  1.84457636 ,  1.84457934 ,  1.8445822  ,  1.84458411 ,  1.84458435 ,  1.84458303 ,  1.84458208],[
             2.27137995 , 2.27138162 , 2.27138567 , 2.27138782 , 2.27139044 , 2.27139211 ,  2.27139306  ,  2.27139187 ,  2.27139211 ,  2.27139115 ,  2.27139068 ,  2.27138901 ,  2.27138686 ,  2.27138591 ,  2.271384   ,  2.27138305 ,  2.27138281 ,  2.27138329 ,  2.27138329 ,  2.271384  ],[
             2.71609354 , 2.71608973 , 2.71608853 , 2.71608639 , 2.71608686 , 2.71608829 ,  2.71609235  ,  2.71609712 ,  2.71610403 ,  2.71611261 ,  2.71612215 ,  2.71613097 ,  2.71614027 ,  2.71614909 ,  2.71615481 ,  2.7161603  ,  2.71616268 ,  2.71616316 ,  2.71616149 ,  2.71615934],[
             2.21054959 , 2.21054339 , 2.21054053 , 2.21053767 , 2.21053529 , 2.21053505 ,  2.21053672  ,  2.21053863 ,  2.21054387 ,  2.21054959 ,  2.21055746 ,  2.21056461 ,  2.210572   ,  2.21058011 ,  2.21058416 ,  2.21058822 ,  2.21059036 ,  2.21059132 ,  2.21058989 ,  2.21058798],[
             1.74631464 , 1.74631429 , 1.74631333 , 1.74631155 , 1.74630737 , 1.74630153 ,  1.74629307  ,  1.74628222 ,  1.7462697  ,  1.74625552 ,  1.74624097 ,  1.74622607 ,  1.74621212 ,  1.74619985 ,  1.74618959 ,  1.74618232 ,  1.74617863 ,  1.74617791 ,  1.74617982 ,  1.74618459],[
             1.74284816 , 1.74284792 , 1.74284887 , 1.74285138 , 1.74285519 , 1.74286115 ,  1.74286962  ,  1.74287999 ,  1.74289274 ,  1.74290669 ,  1.74292195 ,  1.74293673 ,  1.74295092 ,  1.74296331 ,  1.74297309 ,  1.74298048 ,  1.74298465 ,  1.74298513 ,  1.74298298 ,  1.74297857],[
             2.20433831 , 2.20434165 , 2.20434737 , 2.20434976 , 2.20435262 , 2.2043519  ,  2.20435119  , 2.2043469   , 2.20434284  , 2.20433688  , 2.20432997  , 2.20432258  , 2.20431519  , 2.20430994  , 2.2043035   , 2.20430017  , 2.20429778  , 2.20429754  , 2.20429826  , 2.20430136],[
             2.71333814 , 2.71334004 , 2.71334267 , 2.71334434 , 2.71334434 , 2.71334267 ,  2.71333885  ,  2.71333265 ,  2.71332598 ,  2.71331787 ,  2.71330929 ,  2.71329951 ,  2.71329093 ,  2.71328378 ,  2.71327615 ,  2.71327186 ,  2.71326947 ,  2.71326876 ,  2.71327019 ,  2.71327376 ]])


    # seed_in = np.array([[4, 4, 4],
    #                     [0, 0, 0],
    #                     [1, 1, 1],
    #                     [0, 0, 0],
    #                     [6, 6, 6],
    #                     [3, 3, 3],
    #                     [4, 4, 4],
    #                     [6, 6, 6],
    #                     [5, 5, 5],
    #                     [8, 8, 8],
    #                     [6, 6, 6],
    #                     [3, 3, 3],
    #                     [1, 1, 1],
    #                     [6, 6, 6],
    #                     [1, 1, 1],
    #                     [6, 6, 6],
    #                     [4, 4, 4],
    #                     [3, 3, 3]])
    # seed_out = np.array([[ 2.0739,  2.0739,  2.048 ],
    #                      [-5.3445, -5.3445, -5.3612],
    #                      [ 1.0748,  1.0748,  1.0718]])
    # imag_in = np.repeat(np.array([
    #                [10],
    #                 [0],
    #                 [0],
    #                 [0],
    #                 [0],
    #                [10],
    #                [10],
    #                 [0],
    #                 [0],
    #                 [0],
    #                 [0],
    #                [10],
    #                 [0],
    #                 [0],
    #                 [0],
    #                 [0],
    #                 [0],
    #                 [0]]), 100, axis=1)
    network = Network_18_3(*v6_3_3_net.Network().get_network_vars())
    print(network.predict(seed_in, seed_out, imag_in))
    return network.predict(seed_in, seed_out, imag_in)
if __name__ == "__main__":
    main()