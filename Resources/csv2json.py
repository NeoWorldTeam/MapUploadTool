import csv
import json

def csv_to_json(csv_file_path, json_file_path):
    data = []
    with open(csv_file_path, 'r', encoding='utf-8-sig') as csv_file:
        csv_reader = csv.DictReader(csv_file)
        
        for row in csv_reader:
            if row['tagid'] != '' and row['left'] != '':
                data.append(row)

    with open(json_file_path, 'w', encoding='utf-8') as json_file:
        json.dump({"data": data}, json_file, ensure_ascii=False, indent=4)

if __name__ == "__main__":
    csv_file_path = '/Users/fuhao/Downloads/conimgs.csv'
    json_file_path = '/Users/fuhao/Downloads/conimgs_output.json'
    csv_to_json(csv_file_path, json_file_path)
    print("CSV文件已成功转换为JSON格式")