import csv
import sys

# Positions in CSV file
ITEM_NUM = 0
BIB_NUM = 1
TITLE = 2
AUTHOR = 3
O_AUTHOR = 4
PUBLISHER = 5
YEAR = 6
ISBN = 7
EXT_OR_DESC = 8
DIM_OR_DESC = 9
SUBJECT = 10
GENRE = 11
SUMMARY = 12
TOC = 13
CALL_NUM = 14
COPY_NUM = 15
VOL = 16
LOCATION = 17

SIZE = 18  # number of entries per line


def process_line(line):
    if (len(line) != SIZE):
        return None





def main(argv):
    with open(argv[1], newline='') as csvfile:
        csvfile.readline()  # skip header
        reader = csv.reader(csvfile, delimiter='\t')
        for row in reader:
            process_line(row)


if __name__ == '__main__':
    main(sys.argv)
