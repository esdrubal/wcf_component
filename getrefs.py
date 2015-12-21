import json
import sys

if len(sys.argv) != 2:
  sys.exit("Not enough args")
json_file = str(sys.argv[1])

json_data = open(json_file)
data = json.load(json_data)
refs = ""
for key in data['dependencies'].keys():
    refs += "-r:" + key + " "

if data['frameworks'] and data['frameworks']['dnxcore50'] and data['frameworks']['dnxcore50']['dependencies']:
    for key in data['frameworks']['dnxcore50']['dependencies'].keys():
        refs += "-r:" + key + " "

print refs
