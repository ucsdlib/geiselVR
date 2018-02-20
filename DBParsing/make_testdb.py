"""
Module for generating a test database, since the data used to develop
GeiselVR cannot be publicly distributable. Parameters are set in the "User
Values" section in the script, where they are also documented. The resulting
database will be compatible with the GeiselVR program.
"""
import random
import sqlite3
import util
import coldata

### User Values
# Output database file path
db_file = 'testdb.db'
# Desired SQLite table name
table = 'testing'
# Name of index for searching column. Typically this is the call number column
call_index = 'call_index'
# Name of column used for searching
call_col = 'call'
# Size of the database
size = 100000
# Title of every book in database
def_title = 'Sample Book Sample Book Sample Book'
# Padding for values not generated
padding = "N/A"
###

alphabet = []
alphabet.extend("ABCDEFGHIJKLMNOPQRSTUVWXYZ")

conn = sqlite3.connect(db_file)
curs = conn.cursor()
curs.execute(util.query_delete_table(table))
curs.execute(util.query_create_table(table, coldata.ids, coldata.names))
curs.execute(util.query_drop_index(call_index))
curs.execute(util.query_create_index(table, call_index, call_col))

subCount = int(size / len(alphabet))
unique_id = 0
for topLetter in alphabet:
    for i in range(0, subCount):
        botLetter = random.choice(alphabet)
        num = str(random.randrange(0, 10000))
        subLetter = random.choice(alphabet)
        subNum = str(random.randrange(0, 100))

        call = topLetter + botLetter + num + ' .' + subLetter + subNum
        title = def_title
        width = 20 + random.randrange(-10, 10)
        # TODO what if these are not included in coldata?

        # pad insertion to appropiate size
        data = [padding] * len(coldata.ids)
        data[coldata.table[util.ITEM_NUM]] = unique_id
        data[coldata.table[util.CALL_NUM]] = call
        data[coldata.table[util.TITLE]] = title
        data[coldata.table[util.DIM_OR_DESC]] = width

        curs.execute(
            util.query_insert(table, len(coldata.ids)),
            data
        )
        unique_id += 1

conn.commit()
conn.close()
