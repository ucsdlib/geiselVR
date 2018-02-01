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


def query_create_table(table, columns, names):
    """
    Generate table creation query
    :param table: Name of table
    :param columns: IDs of columns. Defined in util.py
    :param names: SQL col names. Matches 1:1 with columns list
    """
    q = 'CREATE TABLE IF NOT EXISTS ' + table + '('

    first = True
    for col, name in zip(columns, names):
        if first:
            if columns[0] == DIM_OR_DESC:
                q += names[0] + " real PRIMARY KEY"
            else:
                q += names[0] + " text PRIMARY KEY"
            first = False
        else:
                if col == DIM_OR_DESC:
                    q += ',' + name + " real"
                else:
                    q += ',' + name + " text"
    q += ')'
    return q


def query_insert(table, count):
    """Generate insert query into table for count many items"""
    q = 'INSERT INTO ' + table + ' VALUES (?'
    for i in range(0, count - 1):
        q += ',?'
    q += ')'
    return q


def query_drop_index(index):
    """Generate drop query for index with name 'index'"""
    return 'DROP INDEX IF EXISTS ' + index


def query_create_index(table, index, col_name):
    """
    Generate query to create index with name 'index' on column
    'col_name' in table 'table'
    """
    return 'CREATE INDEX ' + index + ' ON ' + table + '(' + col_name + ')'


def query_delete_table(table):
    """Generate table delete query for table with name 'table'"""
    return 'DELETE FROM ' + table
