# network_timer.py

import timeit
import numpy as np
import v4L_3_2_net as net

def setup_network():
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
                        [3, 3]])
    seed_out = np.array([[ 2.0739,  2.048 ],
                         [-5.3445, -5.3612],
                         [ 1.0748,  1.0718]])
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
                    [0]]), 90, axis=1)
    network = net.Network()
    return (network, seed_in, seed_out, imag_in)

t = timeit.Timer('network.predict(seed_in, seed_out, imag_in)', setup='from __main__ import setup_network; network, seed_in, seed_out, imag_in = setup_network()')

print t.timeit(number=1)