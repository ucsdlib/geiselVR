"""
Module for generating a test database, since the data used to develop
GeiselVR cannot be publicly distributable. Parameters are set in the "User
Values" section in the script, where they are also documented. The resulting
database will be compatible with the GeiselVR program.
"""
import random
import sqlite3

### User Values
db_file = 'testdb.db'       # data base file path
table = 'testing'           # name of table to populate
call_index = 'call_index'   # name for look up index
call_col = 'call'           # col to index for look up
size = 100000               # size of database
def_title = 'Sample Book Sample Book Sample Book'   # default assigned title
###

tableq = 'CREATE TABLE IF NOT EXISTS ' + table + '(' + call_col + ' text PRIMARY KEY, title text, width real);'
insertq = 'INSERT INTO testing VALUES (?, ?, ?)'
dropq = 'DROP INDEX IF EXISTS ' + call_index
indexq = 'CREATE INDEX ' + call_index + ' ON ' + table + '(' + call_col + ')'
deleteq = 'DELETE FROM testing'

alphabet = []
alphabet.extend("ABCDEFGHIJKLMNOPQRSTUVWXYZ")

conn = sqlite3.connect(db_file)
curs = conn.cursor()
curs.execute(tableq)
curs.execute(deleteq)
curs.execute(dropq)
curs.execute(indexq)

subCount = int(size / len(alphabet))
for topLetter in alphabet:
    for i in range(0, subCount):
        botLetter = random.choice(alphabet)
        num = str(random.randrange(0, 10000))
        subLetter = random.choice(alphabet)
        subNum = str(random.randrange(0, 100))

        call = topLetter + botLetter + num + ' .' + subLetter + subNum
        title = def_title
        width = 20 + random.randrange(-10, 10)

        print(call, title, width)
        curs.execute(insertq, (call, title, width))

conn.commit()
conn.close()

