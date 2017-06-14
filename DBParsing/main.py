import re
import util

PATH = 'data.tsv'

a = [
    '23 cm',
    '43 1/2 cm',
    '20 cm x 30 cm',
    '',
    '25cm'
]

# m = re.search('^(\d{2})\s?(x\s?(\d{2}))?(\d+/\d+)?$', exp)

for exp in a:
    print(exp, end=': ')
    m = re.search('(\d{2})', exp)
    print(m)
