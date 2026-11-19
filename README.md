# 원정대 캠프 (Expedition Camp)

Unity 엔진 교육 2단계 「게임 프로젝트를 받아서 제작단 작업 연습」에서 쓰는 저장소입니다.
기획 데이터 · 씬 · 프리팹을 바꿔 보고, GitHub Desktop 으로 바뀐 파일을 확인하고 되돌리는 연습을 합니다.

## 1. 저장소 구성

| 경로 | 내용 |
|---|---|
| `UnityProject/` | Unity 프로젝트 (Unity 6000.3.20f1 · URP · Input System) |
| `design_data/` | 기획 원본 엑셀 7개 (Monster · Item · Stage · Common · String · Shop · Field) |
| `tools/export_tables_windows.bat` · `tools/export_tables_mac.sh` | 변환 도구 실행 파일 (가상환경 생성 · openpyxl 설치 · 변환) |
| `tools/export_tables.py` | 기획 원본 엑셀을 게임용 CSV 로 내보내는 변환 도구 |
| `.gitignore` | Library · Temp · Logs · obj · Builds · UserSettings 를 제외합니다. |

## 2. 준비물과 프로젝트 열기

| 준비물 | 내용 |
|---|---|
| Unity Hub · Unity 6000.3.20f1 | 다른 버전으로 열면 프로젝트 파일이 바뀌어 Changes 목록에 나타납니다. |
| GitHub Desktop | 저장소 받기 · 브랜치 전환 · 변경 확인 · Discard |
| 파이썬 3.9 이상 | 변환 도구 실행 (STEP 7) |
| 엑셀 | 기획 원본 열기 · 수정 |

- Unity Hub 에서 Add > Add project from disk 를 선택하고 `UnityProject` 폴더를 고릅니다.
- 버전 칸에 6000.3.20f1 이 표시되는지 확인하고 프로젝트를 엽니다.
- 첫 실행 때 Library 폴더를 만드는 동안 창을 닫지 않습니다.
- `Assets/Scenes/Boot.unity` 를 열고 Play 하면 Lobby 로 이동합니다.

게임 씬은 Boot · Lobby · Field_01 · Field_02 입니다.

## 3. 게임 진행

- 로비 오른쪽 위의 「초원으로 떠나기」 카드나 출정 게시판 NPC 를 눌러 스테이지를 고릅니다.
- 로비 왼쪽 아래 버튼과 NPC 로 가방 · 도감 · 상점을 엽니다.
- 필드에서는 WASD 로 이동하고, 가장 가까운 몬스터를 자동으로 공격합니다.
- 자동 전투를 켜면 플레이어가 가장 가까운 몬스터에게 이동합니다.
- 웨이브를 모두 끝내면 결과 창이 나오고, 확인 버튼을 누르면 보상을 저장하고 로비로 돌아갑니다.
- 1챕터 1 ~ 10스테이지만 플레이할 수 있습니다.

## 4. 브랜치

| 브랜치 | 내용 |
|---|---|
| `main` | 정상 상태 |
| `break/01` ~ `break/08` | 실수 재현 브랜치 (STEP 9 수업 · 과제에서 안내에 따라 사용) |

- 브랜치를 바꾸기 전에 Unity 를 닫습니다.
Unity 가 열린 상태에서 바꾸면 씬을 다시 불러오라는 창이 나오고, 저장하지 않은 씬이 있으면 덮어쓸지 묻습니다.
- 브랜치를 바꾼 뒤 Changes 목록에 파일이 생기면, 다른 브랜치로 바꾸기 전에 Discard 합니다.

## 5. 기획 원본을 CSV 로 내보내기 (STEP 7)

실행 파일 하나로 가상환경 생성 · openpyxl 설치 · 변환 · 결과 표시까지 진행합니다.
처음 실행할 때만 `tools/.venv` 가상환경을 만들고 openpyxl 을 설치하므로 인터넷 연결이 필요합니다.

| OS | 실행 방법 |
|---|---|
| Windows | 탐색기에서 `tools\export_tables_windows.bat` 를 더블클릭합니다. |
| macOS | 터미널에서 `bash tools/export_tables_mac.sh` 를 실행합니다. |

- 어느 폴더에서 실행해도 결과가 같습니다.
- 마지막에 「변환 완료」 또는 「변환 실패」 안내가 표시됩니다.
  실패하면 위에 출력된 시트 · 행 · 열 위치의 엑셀 값을 고친 뒤 다시 실행합니다.
- 다른 폴더에 내보내려면 `--out-dir` 뒤에 전체 경로를 적습니다 (예: `bash tools/export_tables_mac.sh --out-dir /tmp/csv_check`).

결과 CSV 는 `UnityProject/Assets/StreamingAssets/data/` 에 저장됩니다.
CSV 를 엑셀이나 메모장에서 직접 저장하지 않고, 엑셀 원본을 고친 뒤 변환 도구를 실행합니다.

### 직접 실행

실행 파일을 쓰지 않을 때는 처음 1번만 openpyxl 을 설치합니다.

Windows

```
py -m pip install openpyxl
```

macOS

```
python3 -m pip install openpyxl
```

저장소 최상위 폴더에서 변환 도구를 실행합니다.

Windows

```
py tools\export_tables.py
```

macOS

```
python3 tools/export_tables.py
```

## 6. 테스트 기능 (STEP 4)

| 기능 | 사용 방법 |
|---|---|
| 치트 패널 | Play 중 F1 (노트북은 fn + F1) |
| 디버그 표시 | Play 중 F2 (노트북은 fn + F2) |
| 고정 난수 | `Assets/Settings/QAConfig.asset` 의 Random Seed, 실행 인자 `-qaSeed 1234` (실행 인자가 우선) |

- 이 기능들은 스크립팅 정의 `QA_BRIDGE` 가 있을 때만 들어갑니다.
에디터의 Project Settings > Player > Scripting Define Symbols 에는 `QA_BRIDGE` 가 들어 있습니다.
- 출시 빌드에는 `QA_BRIDGE` 가 빠지므로 테스트 기능이 들어가지 않습니다.

## 7. Console 메시지

- 빨간 메시지(에러)가 나오면 보고 대상입니다.
첫 줄 · 개수 · 재현 순서 · 화면을 함께 적습니다.
- 노란 메시지(경고)는 내용을 읽고 보고할지 판단합니다.

## 8. 로그 · 세이브 위치

| 파일 | Windows | macOS |
|---|---|---|
| 빌드 로그 | `%USERPROFILE%\AppData\LocalLow\302lab\ExpeditionCamp\Player.log` | `~/Library/Logs/302lab/ExpeditionCamp/Player.log` |
| 세이브 | `%USERPROFILE%\AppData\LocalLow\302lab\ExpeditionCamp\save.json` | `~/Library/Application Support/302lab/ExpeditionCamp/save.json` |

세이브가 손상되면 같은 폴더에 `save.bak.json` 을 만들고 새 세이브로 시작합니다.

## 9. 리소스 출처

| 리소스 | 위치 | 출처 · 라이선스 |
|---|---|---|
| 몬스터 일러스트 40장 · 아이템 아이콘 60장 · 속성 아이콘 5장 | `Assets/Resources/Icons/` | 15챕터 몬스터 도감 저장소에서 제작한 리소스 (자체 제작) |
| 해독제 아이콘 `item_202016.png` | `Assets/Resources/Icons/Item/` | `item_202001.png` 의 색을 바꾼 파일 |
| NPC 이미지 3장 | `Assets/Sprites/NPC/` | 몬스터 일러스트 mon_100009 · mon_100015 · mon_100030 복사본 |
| 플레이어 이미지 | `Assets/Resources/Icons/Monster/mon_100001.png` | 몬스터 일러스트 100001 (불꽃여우) |
| 그림자 · 원 · 체력바 막대 이미지 | `Assets/Art/` | 자체 제작 (파이썬으로 생성) |
| Kenney UI Pack 2.0 | `Assets/ThirdParty/Kenney/` | Kenney (www.kenney.nl) · CC0 · `License.txt` 포함 |
| Noto Sans KR | `Assets/Fonts/` | Google Fonts · SIL Open Font License 1.1 · `Assets/ThirdParty/NotoSansKR/OFL.txt` 포함 |
| TextMesh Pro 기본 리소스 | `Assets/TextMesh Pro/` | Unity uGUI 패키지 |
| 바닥 · 바위 · 나무 · 다리 | 씬 안 | Unity 기본 도형 + URP Lit 재질 |
