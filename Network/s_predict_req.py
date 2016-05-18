#! /usr/bin/python3
#s_predict_req.py

from pythonosc import udp_client as osc_client
from pythonosc import osc_message_builder
from pythonosc import osc_bundle_builder
import numpy as np
import numpyosc as nposc
import random
from numpy import genfromtxt

def build_ready_msg():
    msg = osc_message_builder.OscMessageBuilder(address = "/ready")
    msg.add_arg(1)
    return msg.build()

client = osc_client.UDPClient("127.0.0.1", 1236)
def main():

    seed_in = genfromtxt('seed_in.csv', delimiter=',')
    seed_out = genfromtxt('seed_out.csv', delimiter=',')
    imag_in = genfromtxt('imag_in_small.csv', delimiter=',')

    ready_msg = build_ready_msg()
    seed_in_msg = nposc.osc_msg_from_np_arr('/predict', seed_in)
    seed_out_msg = nposc.osc_msg_from_np_arr('/predict', seed_out)
    imag_in_msg = nposc.osc_msg_from_np_arr('/predict', imag_in)

    client.send(ready_msg)
    client.send(seed_in_msg)
    client.send(seed_out_msg)
    client.send(imag_in_msg)

    #bundle = osc_bundle_builder.OscBundleBuilder(osc_bundle_builder.IMMEDIATELY)
    #bundle.add_content(ready_msg)
    #bundle.add_content(seed_in_msg)
    #bundle.add_content(seed_out_msg)
    #bundle.add_content(imag_in_msg)
#
    #client.send(bundle.build())

    print("Sent bundle:\n" + str(bundle) + ".")

if __name__ == "__main__":
    main()