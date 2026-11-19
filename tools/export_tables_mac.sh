#!/usr/bin/env bash
set -u

# 기획 원본 엑셀을 게임용 CSV 로 내보냅니다.
# 터미널에서 bash tools/export_tables_mac.sh 로 실행하면 가상환경 생성, openpyxl 설치, 변환, 결과 표시까지 진행합니다.
# 단계마다 종료 코드를 확인해서 안내를 보여 주므로 set -e 는 쓰지 않습니다.

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT" || exit 1

VENV_DIR="tools/.venv"
VENV_PY="$VENV_DIR/bin/python"

# 1. 파이썬 찾기
if ! python3 --version >/dev/null 2>&1; then
    echo "파이썬을 찾지 못했습니다. 사전 과제 안내의 파이썬 설치를 확인하세요."
    exit 1
fi

# 2. 파이썬 버전 확인 (3.9 이상)
if ! python3 -c 'import sys; sys.exit(0 if sys.version_info >= (3, 9) else 1)'; then
    echo "파이썬 3.9 이상이 필요합니다. 지금 버전: $(python3 --version 2>&1)"
    exit 1
fi

# 3. 가상환경이 없으면 만듭니다.
if [ ! -x "$VENV_PY" ]; then
    echo "가상환경을 만듭니다: $VENV_DIR"
    if ! python3 -m venv "$VENV_DIR" || [ ! -x "$VENV_PY" ]; then
        echo "가상환경을 만들지 못했습니다."
        exit 1
    fi
fi

# 4. openpyxl 이 없을 때만 가상환경의 pip 로 설치합니다.
if ! "$VENV_PY" -c 'import openpyxl' >/dev/null 2>&1; then
    echo "openpyxl 을 설치합니다."
    if ! "$VENV_PY" -m pip install --disable-pip-version-check -r tools/requirements.txt; then
        echo "openpyxl 설치에 실패했습니다. 인터넷 연결을 확인하세요."
        exit 1
    fi
fi

# 5. 변환 도구를 실행합니다. 이 파일에 넘긴 인자는 그대로 전달합니다.
"$VENV_PY" tools/export_tables.py "$@"
EXIT_CODE=$?

echo ""
if [ "$EXIT_CODE" -eq 0 ]; then
    echo "변환 완료. Unity로 돌아가 Play 해서 확인하세요."
else
    echo "변환 실패. 위 메시지의 엑셀 위치를 고친 뒤 다시 실행하세요."
fi
exit "$EXIT_CODE"
