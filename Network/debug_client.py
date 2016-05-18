#! /usr/bin/python3
#debug_client.py

from pythonosc import udp_client as osc_client
from pythonosc import osc_message_builder
import numpy as np
import numpyosc as nposc
import random
from numpy import genfromtxt

client = osc_client.UDPClient("127.0.0.1", 1236)

def main():

    msg = osc_message_builder.OscMessageBuilder(address = "/debug");

    client.send(msg.build())
    

    print("Sent:\n" + str(msg) + ".")


if __name__ == "__main__":
    main()