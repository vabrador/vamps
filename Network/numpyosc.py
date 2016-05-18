#! usr/bin/python
# numpyosc.py

from pythonosc import osc_message_builder, osc_message
from pythonosc.parsing import osc_types
import numpy as np
import struct

''' Optimization. Removed try/except from corresponding pythonosc.osc_types function '''
def unsafe_write_string(val):
    dgram = val.encode('utf8')
    diff = 4 - (len(dgram) % 4)
    dgram += (b'\x00' * diff)
    return dgram
def unsafe_write_int(val):
    return struct.pack('>i', val)
''' Optimization. Removed try/except from corresponding pythonosc.osc_types function '''
def unsafe_write_float(val):
    return struct.pack('>f', val)
''' Optimization. Write many floats to 32-bit at once (major increase!); also removed try/except. '''
def unsafe_write_floats(floats, num_floats):
    return struct.pack('>'+str(num_floats)+'f', *floats)

''' Optimization. Removed parsing datagram from pythonosc.osc_message.OscMessage '''
class UnsafeOscMessage(object):
    def __init__(self, dgram):
        self._dgram = dgram

    @property
    def dgram(self):
        return self._dgram

def osc_msg_from_np_arr(address, np_arr, id):
    msg_length = 3 + (np_arr.shape[0] * np_arr.shape[1])
    dgram = b''
    if len(address[0:1]) == 1: dgram += osc_types.write_string(address)
    arg_types = 'i' + ''.join(['f' for i in range(msg_length)])
    dgram += unsafe_write_string(',' + arg_types)
    dgram += unsafe_write_int(id)
    dgram += unsafe_write_float(2)
    dgram += unsafe_write_float(np_arr.shape[0])
    dgram += unsafe_write_float(np_arr.shape[1])
    dgram += unsafe_write_floats(np_arr.flat, (np_arr.shape[0] * np_arr.shape[1]))
    return UnsafeOscMessage(dgram)

''' MUCH slower, but builds a full OscMessage object, with .params accessible. '''
def safe_osc_msg_from_np_arr(address, np_arr):
    msg = osc_message_builder.OscMessageBuilder(address = address)
    shape = np_arr.shape
    n_dimensions = len(shape)
    msg.add_arg(n_dimensions)
    for dimension_length in shape:
        msg.add_arg(dimension_length)
    for f in np_arr.ravel(order='C'):
        msg.add_arg(float(f), arg_type="f")
    msg = msg.build()
    return msg

def np_array_from_osc_data(osc_data):
    cur = 0
    n_dimensions = int(osc_data[cur])
    cur += 1
    shape = np.zeros(n_dimensions)
    for i in range(n_dimensions):
        shape[cur-1] = int(osc_data[cur])
        cur += 1
    init = True
    rows = []
    for row_i in range(int(shape[0])):
        row = []
        for col_i in range(int(shape[1])):
            row.append(osc_data[cur])
            cur += 1
        rows.append(row)
    nparr = np.vstack(tuple(rows))
    return nparr