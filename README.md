

# Auto Folder&File System - SJW
<img width="500" height="100" alt="image" src="https://github.com/user-attachments/assets/77871762-71bb-491d-8dc7-37c38a64be46" />

*Auto Folder&File System 은 Unity 프로젝트 내에서 애셋 관리를 간소화하도록 설계된 직관적인 Unity 에디터 확장 도구입니다.

<img width="312" height="747" alt="image" src="https://github.com/user-attachments/assets/eca9b2dd-3834-4840-8efa-61e3e4c61164" />
<img width="317" height="755" alt="image" src="https://github.com/user-attachments/assets/b9fa458a-1ac2-4d4e-91cf-f77cfd9af145" />


## 주요 기능

* **통합 설정 관리:** 모든 도구 설정은 `ScriptableObject`를 통해 중앙에서 관리되므로, 지속적이고 쉽게 설정할 수 있습니다.
* **애셋 일괄 처리:** 지정된 폴더에 있는 애셋(텍스처, 모델, 오디오, 사용자 정의 타입)을 일괄 처리하여 최적화된 임포트 설정을 적용합니다.
    * `.png`, `.jpg` 파일은 **Sprite (2D and UI)**로, **Alpha Is Transparency** 활성화, **Mipmap** 비활성화로 설정됩니다.
    * `.fbx` 모델은 **Import Animation** 비활성화, **Mesh Compression**을 **Medium**으로 설정됩니다.
    * `.wav`, `.mp3`, `.ogg` 오디오는 **Load Type**을 **Streaming**, **Force To Mono** 활성화, **Compression Format**을 **Vorbis**, **Quality**를 0.5로 설정됩니다.
* **유연한 폴더 구성:** 애셋 처리를 위한 단일 대상 폴더를 쉽게 관리하며, 폴더 추가 또는 제거 옵션을 제공합니다.
* **스마트 파일 이동:** 특정 포함/제외 키워드를 기준으로 파일을 원본 폴더에서 대상 폴더로 일괄 이동하며, 선택적으로 하위 폴더를 생성할 수 있습니다.
* **안전한 파일 삭제:** 선택한 폴더에서 특정 기준과 일치하는 파일을 안전하게 삭제하며, 실수로 인한 데이터 손실을 방지하기 위한 강력한 경고 메시지를 제공합니다.
* **사용자 정의 확장자 지원:** 사용자 정의 파일 확장자를 추가하고 관리하여 애셋 처리 기능을 확장할 수 있습니다.
* **시스템 폴더 제외:** `Assets/AutoFolderSystem` 및 `Assets/Editor`와 같은 특정 시스템 폴더는 자동으로 제외되어 도구의 안정성을 보장합니다.

---

## 사용법 / 시작하기

1.  **도구 창 열기:** Unity 에디터에서 상단 메뉴의 `Tools > Asset Automation Tool`을 선택하여 도구 창을 엽니다.

2.  **"파일 일괄 정리" (Batch File Organization) 탭:**
    * **1. 애셋 확장명 선택:** 버튼을 클릭하여 처리할 애셋 유형(예: `.PNG`, `.FBX`, `.WAV` 또는 추가한 사용자 정의 확장자)을 선택합니다.
    * **2. 처리할 폴더 (관리 탭에서 수정):** 현재 애셋 처리를 위해 선택된 단일 폴더를 표시합니다. 폴더를 변경하거나 추가하려면 "폴더 관리" 탭을 이용해야 합니다.
    * **3. 프로젝트 내 기존 폴더 선택:** 프로젝트의 모든 하위 폴더 목록이 표시됩니다. 폴더 옆의 **"선택"** 버튼을 클릭하여 해당 폴더를 애셋 일괄 처리를 위한 **단일 대상 폴더**로 설정할 수 있습니다.
    * **"선택된 폴더의 [확장명] 애셋 일괄 처리 시작"** 버튼을 클릭하여 선택된 폴더 내의 모든 선택된 확장자 애셋에 미리 정의된 임포트 설정을 적용합니다.

3.  **"특정 파일 이동" (Batch File Movement) 탭:**
    * **원본 폴더 경로**와 **대상 폴더 경로**를 지정합니다.
    * **파일 이름 포함** 및 **파일 이름 제외** 키워드(쉼표로 구분)를 사용하여 이동할 파일을 필터링합니다.
    * **"하위 폴더 생성"** 토글: 활성화하면, 이동하는 각 파일에 대해 대상 폴더 안에 새 하위 폴더(원본 파일 이름 또는 지정된 접두사 포함)가 생성됩니다.
    * **"파일 일괄 이동 시작"** 버튼을 클릭하여 작업을 실행합니다.

4.  **"확장자 관리" (Extension Management) 탭:**
    * 이 탭을 사용하여 `.tga`, `.exr`와 같은 **사용자 정의 파일 확장자**를 추가하거나 제거할 수 있습니다. 추가된 확장자는 "파일 일괄 정리" 탭에서 선택 옵션으로 나타납니다.

5.  **"파일 일괄 제거" (Batch File Deletion) 탭:**
    * 파일을 삭제할 **대상 폴더 경로**를 지정합니다.
    * **"하위 폴더 포함"**을 토글하여 하위 디렉토리의 파일도 삭제할지 결정합니다.
    * **파일 이름 포함** 및 **파일 이름 제외** 키워드(쉼표로 구분)를 사용하여 삭제할 파일을 필터링합니다.
    * **"🚨 파일 일괄 제거 시작 🚨"** 버튼을 클릭합니다. 영구적인 삭제 전에 **매우 중요한 경고 메시지**가 표시됩니다.

6.  **"폴더 관리" (Folder Organization) 탭:**
    * 이 탭은 "파일 일괄 정리" 탭에서 애셋을 처리할 대상을 관리하는 곳입니다.
    * **"새 폴더 경로"** 입력 필드와 **"폴더 선택"** 버튼을 사용하여 새로운 폴더를 생성하거나 기존 폴더를 추가할 수 있습니다.
    * **"현재 선택된 폴더"** 목록에서, 현재 처리 대상으로 지정된 폴더가 표시됩니다. 여기서 폴더를 선택하거나 이름을 변경할 수 있습니다.
    * **폴더 이름 변경:** 목록에서 폴더를 선택하고 **"이름 변경"** 버튼을 클릭하면, 해당 폴더의 새 이름을 입력할 수 있습니다. 변경 사항은 즉시 프로젝트에 반영됩니다.
    * **"프로젝트 내 기존 폴더"** 목록에서 프로젝트의 모든 하위 폴더를 보고, **"선택"** 또는 **"삭제"** 버튼을 사용하여 관리할 수 있습니다. **"삭제"** 버튼은 해당 폴더와 그 내용을 프로젝트에서 **영구적으로 삭제**합니다. 여러 차례 경고 메시지가 표시됩니다.

7.  **"제외 폴더" (Excluded Folders) 탭:**
    * 프로젝트 폴더 목록에서 제외할 폴더를 추가하거나 제거할 수 있습니다. 제외된 폴더는 도구의 기능에서 무시됩니다.

8.  **"모든 설정 초기화" 버튼:** 도구 창 하단의 이 버튼을 클릭하면 저장된 모든 설정이 기본값으로 초기화됩니다. 이 작업에 대한 확인 대화 상자가 표시됩니다.

---

## 제한 사항 및 중요 사항

* **되돌릴 수 없는 작업:** 파일 삭제 작업은 **영구적이며 되돌릴 수 없습니다**. 일괄 삭제를 수행하기 전에 항상 프로젝트를 백업하거나 버전 관리를 사용하십시오.
* **단일 대상 폴더:** "파일 일괄 정리" 탭의 애셋 처리 기능은 현재 **단일 지정 대상 폴더**만 처리합니다. "폴더 관리" 탭의 "선택" 버튼을 사용하면 항상 단일 폴더가 설정됩니다.
* **에디터 스크립트 위치:** 설치 시 명시된 바와 같이, 도구가 Unity 에디터 내에서 올바르게 작동하려면 모든 스크립트가 `Editor` 폴더(예: `Assets/Editor/AssetAutomation/`) 내에 **반드시** 있어야 합니다.
* **제외 폴더:** `AutoFolderSystem` 폴더(도구 스크립트가 위치해야 하는 곳)는 프로젝트 폴더 목록에서 **명시적으로 제외**되며, 의도치 않은 자체 삭제를 방지하기 위해 도구의 대상으로 지정될 수 없습니다.

---

## 📄 라이선스

이 소프트웨어는 명시적 또는 묵시적인 어떠한 종류의 보증도 없이 "있는 그대로" 제공됩니다. 상품성, 특정 목적에의 적합성 및 비침해에 대한 보증을 포함하되 이에 국한되지 않습니다. 어떠한 경우에도 작성자나 저작권 보유자는 소프트웨어 또는 소프트웨어의 사용 또는 기타 거래와 관련하여 발생하는 모든 청구, 손해 또는 기타 책임에 대해 계약, 불법 행위 또는 기타 방식으로 책임을 지지 않습니다.

**Copyright (c) Sonjongwook**

문의 사항이나 문제가 있는 경우 개발자에게 문의하십시오.
