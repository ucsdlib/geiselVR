"""
Defines the columns which GeiselVR is currently expecting.
"""
import util

ids = [
    util.ITEM_NUM,
    util.CALL_NUM,
    util.TITLE,
    util.AUTHOR,
    util.DIM_OR_DESC,
    util.GENRE,
    util.SUBJECT,
    util.SUMMARY
]

names = [
    'id',
    'call',
    'title',
    'author',
    'width',
    'genre',
    'subject',
    'summary'
]

table = {}
for i in range(0, len(ids)):
    table[ids[i]] = i
