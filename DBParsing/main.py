import re

from util import *

PATH = 'data.tsv'

count = 0
for data in process_col_iter(PATH, ISBN):
    print(count, data)
    count += 1
