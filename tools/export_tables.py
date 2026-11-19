"""design_data 폴더의 기획 원본 엑셀을 게임용 CSV로 내보냅니다.

사용 방법
    python3 tools/export_tables.py
    python3 tools/export_tables.py --out-dir 다른폴더   (게임 폴더 대신 다른 폴더에 내보내기)

내보내기 대상이 C 또는 CS인 열만 CSV에 들어갑니다.
첫 칸이 # 으로 시작하는 행은 내보내지 않습니다.
2행의 타입과 맞지 않는 값이 있으면 위치를 출력하고 종료 코드 1로 중단합니다.
"""

import argparse
import csv
import os
import re
import sys

from openpyxl import load_workbook

HEADER_ROW_COUNT = 4
FIRST_DATA_ROW = HEADER_ROW_COUNT + 1

WORKBOOK_SHEETS = [
    ("Monster.xlsx", ["monster", "skill"]),
    ("Item.xlsx", ["item", "drop_group"]),
    ("Stage.xlsx", ["stage", "stage_wave"]),
    ("Common.xlsx", ["level_exp", "const", "enum"]),
    ("String.xlsx", ["string"]),
    ("Shop.xlsx", ["shop"]),
    ("Field.xlsx", ["field_object"]),
]

EXPORT_TARGETS = ("C", "CS")
BOOL_VALUES = ("TRUE", "FALSE")
ENUM_VALUE_PATTERN = re.compile(r"^[A-Z][A-Z0-9_]*$")


def project_root():
    tools_dir = os.path.dirname(os.path.abspath(__file__))
    return os.path.dirname(tools_dir)


def cell_to_text(value):
    """칸의 값을 문자열로 바꿉니다. 기획자가 입력한 공백은 그대로 둡니다."""
    if value is None:
        return ""
    if isinstance(value, bool):
        return "TRUE" if value else "FALSE"
    if isinstance(value, float) and value.is_integer():
        return str(int(value))
    return str(value)


def header_text(value):
    """머리 4행의 값을 문자열로 바꿉니다."""
    return cell_to_text(value).strip()


def read_header(worksheet):
    names = []
    types = []
    targets = []
    for column_index in range(1, worksheet.max_column + 1):
        name = header_text(worksheet.cell(row=1, column=column_index).value)
        if name == "":
            break
        names.append(name)
        types.append(header_text(worksheet.cell(row=2, column=column_index).value))
        targets.append(header_text(worksheet.cell(row=3, column=column_index).value))
    return names, types, targets


def check_value(value_type, text):
    """타입에 맞지 않으면 이유를 담은 문자열을 반환하고, 맞으면 None을 반환합니다."""
    if value_type == "int":
        if text == "":
            return "정수 칸이 비어 있습니다"
        try:
            int(text)
        except ValueError:
            return "정수로 변환할 수 없습니다"
        return None
    if value_type == "bool":
        if text not in BOOL_VALUES:
            return "TRUE 또는 FALSE가 아닙니다"
        return None
    if value_type.startswith("enum:"):
        if text == "":
            return "허용 값 칸이 비어 있습니다"
        if ENUM_VALUE_PATTERN.match(text) is None:
            return "허용 값 형식(대문자와 밑줄)이 아닙니다"
        return None
    return None


def export_sheet(file_name, worksheet, out_dir):
    names, types, targets = read_header(worksheet)

    export_indexes = []
    for column_index, target in enumerate(targets):
        if target in EXPORT_TARGETS:
            export_indexes.append(column_index)

    errors = []
    data_rows = []
    for excel_row in range(FIRST_DATA_ROW, worksheet.max_row + 1):
        first_text = cell_to_text(worksheet.cell(row=excel_row, column=1).value).strip()
        if first_text == "":
            continue
        if first_text.startswith("#"):
            continue

        row_values = []
        for column_index in range(len(names)):
            text = cell_to_text(worksheet.cell(row=excel_row, column=column_index + 1).value)
            if column_index in export_indexes:
                reason = check_value(types[column_index], text)
                if reason is not None:
                    errors.append(
                        "%s · %s · 엑셀 %d행 · %s · 값 %r · %s"
                        % (file_name, worksheet.title, excel_row, names[column_index], text, reason)
                    )
            row_values.append(text)

        exported = []
        for column_index in export_indexes:
            exported.append(row_values[column_index])
        data_rows.append(exported)

    if errors:
        return None, errors

    header = []
    for column_index in export_indexes:
        header.append(names[column_index])

    out_path = os.path.join(out_dir, "%s.csv" % worksheet.title)
    with open(out_path, "w", encoding="utf-8", newline="") as out_file:
        writer = csv.writer(out_file, quoting=csv.QUOTE_MINIMAL, lineterminator="\n")
        writer.writerow(header)
        for row in data_rows:
            writer.writerow(row)

    return len(data_rows), []


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--out-dir", default="")
    options = parser.parse_args()

    root = project_root()
    design_dir = os.path.join(root, "design_data")
    out_dir = os.path.join(root, "UnityProject", "Assets", "StreamingAssets", "data")
    if options.out_dir != "":
        out_dir = os.path.abspath(options.out_dir)
    os.makedirs(out_dir, exist_ok=True)

    all_errors = []
    counts = []

    for file_name, sheet_names in WORKBOOK_SHEETS:
        path = os.path.join(design_dir, file_name)
        if not os.path.exists(path):
            print("기획 원본 파일을 찾을 수 없습니다: %s" % path)
            sys.exit(1)
        workbook = load_workbook(path, data_only=True)
        for sheet_name in sheet_names:
            if sheet_name not in workbook.sheetnames:
                print("%s 파일에 %s 시트가 없습니다" % (file_name, sheet_name))
                sys.exit(1)
            count, errors = export_sheet(file_name, workbook[sheet_name], out_dir)
            if errors:
                all_errors.extend(errors)
            else:
                counts.append((sheet_name, count))
        workbook.close()

    if all_errors:
        print("타입이 맞지 않는 값 %d건을 찾았습니다." % len(all_errors))
        for message in all_errors:
            print("  %s" % message)
        print("CSV를 만들지 않고 중단합니다.")
        sys.exit(1)

    print("내보낸 CSV %d개" % len(counts))
    for sheet_name, count in counts:
        print("  %-12s %4d행" % (sheet_name, count))


if __name__ == "__main__":
    main()
