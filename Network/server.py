# server.py
# An OSC server/client for Balancer.

from pythonosc import udp_client as osc_client
from pythonosc import osc_message_builder
from pythonosc import dispatcher
from pythonosc import osc_server
import numpyosc as nposc

import v5d1_3_3_net # directly from contraction to nose
import v6LN_3_3_net # v6LN
import v6CL_6_3_net # v6CL

import v7CL_6_3_net # 2D nets
import v7LN_3_3_net # 2D nets

import network


# This is the network the server will use for prediction.
CLnet = network.Network_18_18(*v6CL_6_3_net.Network().get_network_vars())
LNnet = network.Network_18_3 (*v6LN_3_3_net.Network().get_network_vars())   # 3D
#CLnet = network.Network_18_18(*v7CL_6_3_net.Network().get_network_vars())
#LNnet = network.Network2D_18_3(*v7LN_3_3_net.Network().get_network_vars())  # 2D

oldCNnet = network.Network_18_3(*v5d1_3_3_net.Network().get_network_vars())


def create_blank_request():
   return [None, None, None, None, None]
# def create_blank_request():
#     return [None, None, None] # old 3 input network for CN-style

current_prediction_request = create_blank_request()
receiving_input_index = 0
def ready(unused_addr, *data):
    print("Ready! ID: " + str(data[0]))
    global current_prediction_request, receiving_input_index
    current_prediction_request = create_blank_request()
    current_prediction_request[0] = data[0]
    receiving_input_index = 1

responder = osc_client.UDPClient("127.0.0.1", 1237)
def predict_and_respond(unused_addr, *data):
    global current_prediction_request, receiving_input_index
    #print(data)
    nparr = nposc.np_array_from_osc_data(data)
    current_prediction_request[receiving_input_index] = nparr
    #print(nparr)
    receiving_input_index += 1
    if (receiving_input_index == 5):
    # if (receiving_input_index == 3):
        #print("Ready to do prediction!")
        #print("ctr_seed_in: " + str(current_prediction_request[0]))
        #print("len_seed_out: " + str(current_prediction_request[1]))
        #print("ctr_imag_in: " + str(current_prediction_request[2]))
        #print("len_seed_in: identical to len_seed_out, will use len_seed_out.")
        #print("nose_seed_out: " + str(current_prediction_request[3]))
        print("Getting network response...")

        len_response =      CLnet.predict(
                          current_prediction_request[1], # ctr_seed_in
                          current_prediction_request[2], # len_seed_out
                          current_prediction_request[3]) # ctr_imag_in

        nose_pos_response = LNnet.predict(
                          current_prediction_request[2], # len_seed_in (len_seed_out above)
                          current_prediction_request[4], # nose_seed_out
                          len_response) # len_imag_in = len_response

        # nose_pos_response = oldCNnet.predict(
        #                     current_prediction_request[1],
        #                     current_prediction_request[4],
        #                     current_prediction_request[3])

        #print(nose_pos_response)
        responder.send(nposc.osc_msg_from_np_arr("", nose_pos_response, current_prediction_request[0]))

        print("Sent response back.")
        receiving_input_index = 0

def print_msg(unused_addr, *data):
    print('[debug]')
    print(data)
    response = 99.0
    msg = osc_message_builder.OscMessageBuilder(address = "/debug")
    msg.add_arg(response)
    responder.send(msg.build())
    print('[debug] Responded with ' + str(response))

def main():
    router = dispatcher.Dispatcher()
    router.map("/debug", print_msg)
    router.map("/ready", ready)
    router.map("/predict", predict_and_respond)
    server = osc_server.BlockingOSCUDPServer(("127.0.0.1", 1236), router)
    print("Serving on {}".format(server.server_address))
    server.serve_forever()

if __name__ == "__main__":
    main()