import csv
import re
import sqlite3

from fractions import Fraction

# Positions in CSV file
ITEM_NUM = 0        # item number
BIB_NUM = 1         # library number
TITLE = 2           # title
AUTHOR = 3          # author
O_AUTHOR = 4        # other author
PUBLISHER = 5       # publisher
YEAR = 6            # years
ISBN = 7            # isbn
EXT_OR_DESC = 8     # extent / description
DIM_OR_DESC = 9     # dimension / description
SUBJECT = 10        # subject
GENRE = 11          # genre
SUMMARY = 12        # summary
TOC = 13            # TOC
CALL_NUM = 14       # call number
COPY_NUM = 15       # copy number
VOL = 16            # volume
LOCATION = 17       # location

SIZE = 18  # number of entries per line

dim_regex = re.compile('(\d+(?:\s+\d+\/\d+)?)\s?')  # get dimension


def process_col(path, col, processor, *args):
    with open(path) as file:
        reader = csv.reader(file, delimiter='\t')
        for row in reader:
            processor(row[col], *args)


def process_col_iter(path, col):
    with open(path) as file:
        reader = csv.reader(file, delimiter='\t')
        for row in reader:
            yield row[col]


def extract_dim(data):
    dim_match = dim_regex.match(data)
    if dim_match is not None:
        dim_parts = dim_match.group(0).strip().split()
        return float(sum(Fraction(s) for s in dim_parts))
    else:
        return 0

