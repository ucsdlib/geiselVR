import csv
import sys
import sqlite3
import util


# Useful queries
'''SELECT * FROM main
WHERE call >= ? and call <= ?
ORDER BY call'''


def process_line(line):
    if len(line) != util.SIZE:
        return None

    width = util.extract_dim(line[util.DIM_OR_DESC])

    return line[util.CALL_NUM], line[util.TITLE], width


def store_data(data: tuple, db: sqlite3.Cursor):
    db.execute('INSERT INTO testing VALUES (?,?,?)', data)


def main(argv):
    with open(argv[1], newline='') as csvfile:
        csvfile.readline()  # skip header
        reader = csv.reader(csvfile, delimiter='\t')
        conn = sqlite3.connect('call-only.db')
        cursor = conn.cursor()
        cursor.execute('DELETE FROM testing')

        # Populate dat
        count = 0
        for row in reader:
            data_tuple = process_line(row)
            store_data(data_tuple, cursor)
            if count % 100000 == 0:
                print(count)
            count += 1
        conn.commit()
        conn.close()


if __name__ == '__main__':
    main(sys.argv)
