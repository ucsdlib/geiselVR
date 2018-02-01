import util

group = [
    util.CALL_NUM,
    util.AUTHOR,
    util.DIM_OR_DESC,
    util.GENRE,
    util.SUBJECT,
    util.SUMMARY
]

names = [
    'call',
    'author',
    'width',
    'genre',
    'subject',
    'summary'
]

table = 'testing'

print(util.query_create_table(table, group, names))
print(util.query_insert(table, len(group)))
print(util.query_drop_index('call_index'))
print(util.query_create_index(table, 'call_index', names[0]))
print(util.query_delete_table(table))
