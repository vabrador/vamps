#! usr/bin/python3
# OSCpacktimer.py

import timeit
import numpy as np
import v4L_3_2_net as net
import numpyosc as nposc

def setup_network_data():
    seed_in = np.array([[4, 4],
                        [0, 0],
                        [1, 1],
                        [0, 0],
                        [6, 6],
                        [3, 3],
                        [4, 4],
                        [6, 6],
                        [5, 5],
                        [8, 8],
                        [6, 6],
                        [3, 3],
                        [1, 1],
                        [6, 6],
                        [1, 1],
                        [6, 6],
                        [4, 4],
                        [3, 3]], dtype=np.float32)
    seed_out = np.array([[ 2.0739,  2.048 ],
                         [-5.3445, -5.3612],
                         [ 1.0748,  1.0718]], dtype=np.float32)
    imag_in = np.repeat(np.array([
                   [10],
                    [0],
                    [0],
                    [0],
                    [0],
                   [10],
                   [10],
                    [0],
                    [0],
                    [0],
                    [0],
                   [10],
                    [0],
                    [0],
                    [0],
                    [0],
                    [0],
                    [0]], dtype=np.float32), 90, axis=1)
    return (seed_in, seed_out, imag_in)

def setup_osc_msgs(safe=False):
    (seed_in, seed_out, imag_in) = setup_network_data()
    if (safe):
        msg = nposc.safe_osc_msg_from_np_arr('/predict', seed_in)
        msg2 = nposc.safe_osc_msg_from_np_arr('/predict', seed_out)
        msg3 = nposc.safe_osc_msg_from_np_arr('/predict', imag_in)
    else:
        msg = nposc.osc_msg_from_np_arr('/predict', seed_in)
        msg2 = nposc.osc_msg_from_np_arr('/predict', seed_out)
        msg3 = nposc.osc_msg_from_np_arr('/predict', imag_in)
    return msg, msg2, msg3

def do_timer():
    t = timeit.Timer('msg = nposc.osc_msg_from_np_arr(\'/predict\', seed_in); msg2 = nposc.osc_msg_from_np_arr(\'/predict\', seed_out); msg3 = nposc.osc_msg_from_np_arr(\'/predict\', imag_in)', setup='from __main__ import setup_network_data; import numpyosc as nposc; seed_in, seed_out, imag_in = setup_network_data()')
    t2 = timeit.Timer('seed_in = nposc.np_array_from_osc_data(data1); seed_out = nposc.np_array_from_osc_data(data2); imag_in = nposc.np_array_from_osc_data(data3)', setup='from __main__ import setup_osc_msgs; import numpyosc as nposc; data1, data2, data3 = map(lambda x: x.params, setup_osc_msgs(safe=True))')

    print(t.timeit(number=1))
    print(t2.timeit(number=1))


# ==================================== #


# profiling instead of function timing
import cProfile, pstats, io

def do_profile():
    pr = cProfile.Profile()
    msg1, msg2, msg3 = setup_osc_msgs(safe=True)
    pr.enable()
    data1, data2, data3 = map(lambda x: x.params, (msg1, msg2, msg3))
    seed_in = nposc.np_array_from_osc_data(data1)
    seed_out = nposc.np_array_from_osc_data(data2)
    imag_in = nposc.np_array_from_osc_data(data3)
    pr.disable()
    s = io.StringIO()
    sortby = 'cumulative'
    ps = pstats.Stats(pr, stream=s).sort_stats(sortby)
    ps.print_stats()
    print(s.getvalue())

# ==================================== #


if __name__ == "__main__":
    do_timer()