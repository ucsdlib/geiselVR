import random
import sqlite3

### User Values
db_file = 'testdb.db'       # data base file path
table = 'testing'           # name of table to populate
call_index = 'call_index'   # name for look up index
call_col = 'call'           # col to index for look up
size = 100000               # size of database
def_title = 'Sample'        # default assigned title
###

insertq = 'INSERT INTO testing VALUES (?, ?, ?)'
dropq = 'DROP INDEX IF EXISTS ' + call_index
indexq = 'CREATE INDEX ' + call_index + ' ON ' + table + '(' + call_col + ')'
deleteq = 'DELETE FROM testing'

alphabet = []
alphabet.extend("ABCDEFGHIJKLMNOPQRSTUVWXYZ")

conn = sqlite3.connect(db_file)
curs = conn.cursor()
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

