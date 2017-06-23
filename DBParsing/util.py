import csv
import sqlite3

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

dim_regex = '(\d+(?:\s+\d+\/\d+)?)\s?'  # get dimension


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
