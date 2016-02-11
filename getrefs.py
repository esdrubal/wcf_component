import json
import sys

if len(sys.argv) < 2:
  sys.exit("Not enough args")
json_file = str(sys.argv[1])
ignore_refs = sys.argv[2:]

json_data = open(json_file)
data = json.load(json_data)
refs = ""
for key in data['dependencies'].keys():
    if not key in ignore_refs:
      refs += "-r:" + key + " "

if 'frameworks' in data and 'dnxcore50' in data['frameworks'] and 'dependencies' in data['frameworks']['dnxcore50']:
    for key in data['frameworks']['dnxcore50']['dependencies'].keys():
        if not key in ignore_refs:
          refs += "-r:" + key + " "

print refs
