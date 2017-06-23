import re
import util

from util import process_col

PATH = 'data.tsv'

process_col(PATH, util.DIM_OR_DESC, print)
