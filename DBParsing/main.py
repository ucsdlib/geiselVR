import re

from util import *

PATH = 'data.tsv'

count = 0
for data in col_iterator(PATH, ISBN):
    print(count, data)
    count += 1
