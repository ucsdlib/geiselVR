import re

from util import *

PATH = 'data.tsv'

for data in process_col_iter(PATH, DIM_OR_DESC):
    print(data)
