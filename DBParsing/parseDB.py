import csv
import sys
import sqlite3
import util

### User values
output_file = 'data/database.db'  # output database path
input_file = 'data/data1.tsv'  # input tsv file
columns = [  # ID of columns to add
    util.ITEM_NUM,
    util.CALL_NUM,
    util.AUTHOR,
    util.DIM_OR_DESC,
    util.GENRE,
    util.SUBJECT,
    util.SUMMARY
]
names = [  # column names for each ID in 'columns'
    'id',
    'call',
    'author',
    'width',
    'genre',
    'subject',
    'summary'
]
table = 'main'  # table name
index_col = names[1]  # name of column to index
index_name = names[1] + "_index"  # name of index itself
###

# Useful queries
'''SELECT * FROM main
WHERE call >= ? and call <= ?
ORDER BY call'''

count = 0


def store_data(data, cursor):
    global count
    count += 1
    if count % 50000 == 0:
        print("Records processed:", count)
    cursor.execute(
        util.query_insert(table, len(names)),
        data
    )


def process_line(line):
    if len(line) != util.SIZE:
        return None

    width = util.extract_dim(line[util.DIM_OR_DESC])

    return line[util.CALL_NUM], line[util.TITLE], width


def main(argv):
    conn = sqlite3.connect(output_file)
    cursor = conn.cursor()

    # set up table
    cursor.execute(util.query_delete_table(table))
    cursor.execute(util.query_create_table(table, columns, names))
    cursor.execute(util.query_drop_index(index_name))
    cursor.execute(util.query_create_index(table, index_name, index_col))

    # populate table
    util.group_processor(input_file, columns, store_data, cursor)

    conn.commit()
    conn.close()


if __name__ == '__main__':
    main(sys.argv)
