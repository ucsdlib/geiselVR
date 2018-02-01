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
    """
    Applies a method to every item of a column
    :param path: Location of data
    :param col: Column identifier. These are defined in util.py
    :param processor: Function to apply to every element. Takes in element string
    :param args: Any arguments to forward to processor
    """
    with open(path) as file:
        reader = csv.reader(file, delimiter='\t')
        for row in reader:
            processor(row[col], *args)


def col_iterator(path, col):
    """
    Provides an iterator for each element in a column.
    :param path: Location of data
    :param col: Column Defined. Defined in util.py
    """
    with open(path) as file:
        reader = csv.reader(file, delimiter='\t')
        for row in reader:
            yield row[col]


def extract_dim(data):
    """
    Parse dimension field to obtain a float value
    """
    dim_match = dim_regex.match(data)
    if dim_match is not None:
        dim_parts = dim_match.group(0).strip().split()
        return float(sum(Fraction(s) for s in dim_parts))
    else:
        return 0


def group_iterator(path, columns):
    """
    Provides an iterator for all elements over a group of columns
    :param path: Location of data
    :param columns: Collection of columns. Defined in util.py
    """
    with open(path) as file:
        reader = csv.reader(file, delimiter='\t')
        for row in reader:
            l = []
            for col in columns:
                l.append(row[col])
            yield l


def group_processor(path, columns, processor, *args):
    """
    Convenience method for group_iterator. Applies processor to
    each group returned by group_iterator. Args are forwarded to processor.
    Processor must take group as first argument
    """
    for group in group_iterator(path, columns):
        processor(group, *args)
