#!/usr/bin/python
# matlab_to_numpy.py

import sys, getopt, re

def numpy_from_matlab_network_constants(in_path, out_path):
    text = ""
    with open(in_path, 'r') as in_file:
        text = in_file.read()
        # rule 1: replace " = " with " = np.array(["
        text = text.replace(" = ", " = np.array([")
        # rule 2: replace ";\n" with "])\n"
        text = text.replace(";\n", "])\n")
        # rule 3 (must follow 2): replace ";" with "],["
        text = text.replace(";", "],[")
        # rule 4: replace "[0-9] [\-0-9]" with ", " (requires index of match + 1)
        matches_iter = re.finditer("\d [\d-]", text)
        for match in matches_iter:
            text = text[:match.start()+1] + "," + text[match.start()+2:]
        # rule 5: replace "%" with "#"
        text = text.replace("%", "#")
        # rule 6: find "[a-zA-Z]* = " prepend "self."
        matches_iter = re.finditer("\n[a-zA-Z0-9_]+ = ", text)
        offset = 0
        for match in matches_iter:
            text = text[:match.start()+1+offset] + "self." + text[match.start()+1+offset:]
            offset += 5

    if (len(text) == 0):
        print("ERROR: empty text in.")
    
    with open(out_path, 'w') as out_file:
        out_file.write(text)


def main(argv):
    in_file = ""
    out_file = ""
    try:
        opts, args = getopt.getopt(argv, "hi:o:",["ifile=", "ofile="])
    except getopt.GetoptError:
        print('netconvert.py -i <in_file> -o <out_file>')
        sys.exit(2)
    for opt, arg in opts:
        if opt == '-h':
            print('netconvert.py -i <in_file> -o <out_file>')
            sys.exit()
        elif opt in ("-i", "--ifile"):
            in_file = arg
        elif opt in ("-o", "--ofile"):
            out_file = arg
    numpy_from_matlab_network_constants(in_file, out_file)


if __name__ == "__main__":
    main(sys.argv[1:])