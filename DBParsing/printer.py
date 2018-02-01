import util

group = (
    util.CALL_NUM,
    util.AUTHOR,
    util.DIM_OR_DESC,
    util.GENRE,
    util.SUBJECT,
    util.SUMMARY
)

util.group_processor('data/data1.tsv', group, print)
