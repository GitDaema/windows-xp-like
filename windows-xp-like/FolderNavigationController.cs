using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace windows_xp_like
{
    public class FolderNavigationController
    {
        private FolderView _view;
        private AppForm _window;
        private Dictionary<string, FolderData> _fileSystemData;

        // 뒤로 가기 등을 지원하기 위해 디렉토리에서 어떤 경로로 탐색을 진행했는지를 저장하는 스택
        private Stack<string> _history = new Stack<string>();

        private string _currentKey; // 현재 폴더 키

        // 앱 실행을 데스크탑에 요청하기 위한 이벤트 신호
        public event Action<FileSystemItem, Image> AppLaunchRequested;

        public FolderNavigationController(FolderView view, AppForm window, Dictionary<string, FolderData> fileSystemData, string startKey)
        {
            _view = view;
            _window = window;
            _fileSystemData = fileSystemData;

            _currentKey = startKey; // 첫 폴더 키는 생성될 때 정하므로 전달받은 키로 초기화

            _view.ItemDoubleClicked += OnItemDoubleClicked;
            _view.BackClicked += OnBackClicked;
            _view.UpClicked += OnUpClicked;
            _view.CreateFolderRequested += OnCreateFolderRequested;
        }

        /// <summary>
        /// 데스크톱에서 준비가 끝났을 때 호출되는 폴더 창 초기화 메서드 
        /// </summary>
        public void Start()
        {
            // 첫 폴더 키를 이용해 첫 폴더 데이터 가져오기
            FolderData currentFolderData = _fileSystemData[_currentKey];

            _window.Text = currentFolderData.Name; // 창 이름 초기화

            _view.LoadItems(currentFolderData.Items); // 초기 아이템 항목 로딩

            // 처음 루트 폴더에 들어왔을 때는 뒤로 가기도, 위로 가기도 비활성화
            _view.SetBackEnabled(false);

            // 첫 폴더 데이터의 부모 키가 존재할 때만 위로 가기 버튼 활성화
            bool hasParent = false;
            if (currentFolderData.ParentKey != null)
                hasParent = true;

            _view.SetUpEnabled(hasParent);
        }

        /// <summary>
        /// 폴더에서 항목을 더블클릭했을 때 파일 실행 또는 내부 폴더 열기 동작을 시행하는 메서드
        /// </summary>
        /// <param name="item"></param>
        private void OnItemDoubleClicked(FileSystemItem item, Image icon)
        {
            if (item == null) return;

            // 폴더가 맞고, 탐색 키가 빈 문자열이 아니며, 그 탐색 키를 실제로 가지고 있으면
            if (item.IsFolder && !string.IsNullOrEmpty(item.ActionKey) && _fileSystemData.ContainsKey(item.ActionKey))
            {
                _history.Push(_currentKey); // 탐색 히스토리 스택에 현재 키
                _currentKey = item.ActionKey; // 현재 키 갱신

                _view.LoadItems(_fileSystemData[_currentKey].Items);
                _window.Text = item.Name;

                _view.SetBackEnabled(true); // 탐색이 한 단계 나아갔으므로 뒤로 가기 버튼 활성화
                _view.SetUpEnabled(true); // 이제 부모 폴더로 돌아갈 수 있도록 위로 가기 버튼 활성화
            }
            else // 폴더가 아니거나 탐색 키가 없다면 파일일 가능성이 있으므로 신호 보내기
            {
                AppLaunchRequested?.Invoke(item, icon); // 실제 파일 실행 가능 여부 등은 데스크탑에서 검사
            }
        }

        private void OnBackClicked()
        {
            if (_history.Count > 0)
            {
                _currentKey = _history.Pop(); // 뒤로 가기이므로 스택에서 팝해서 현재 키 갱신

                FolderData currentFolderData = _fileSystemData[_currentKey];

                if (currentFolderData == null)
                    return;

                _view.LoadItems(currentFolderData.Items); // 창에 보일 항목 초기화
                _window.Text = currentFolderData.Name;

                if (_history.Count > 0) // 아직 스택에 항목이 1개 이상 남아 있다면
                    _view.SetBackEnabled(true); // 더 뒤로 갈 수 있으니 버튼 활성화
                else // 스택이 비었다면
                    _view.SetBackEnabled(false); // 더 뒤로 갈 수 없으니 버튼 비활성화

                // 뒤로 갔을 때는 부모 또한 바뀌므로 위로 가기 버튼 활성화 여부 결정 필요
                string parentKey = currentFolderData.ParentKey;
                _view.SetUpEnabled(parentKey != null);
            }
        }

        private void OnUpClicked()
        {
            // 우선 부모 키 찾기
            string parentKey = _fileSystemData[_currentKey].ParentKey;

            // 부모 키를 찾았고, 그 부모 키가 실제로 파일 시스템 딕셔너리에 저장되어 있으면
            if (parentKey != null && _fileSystemData.ContainsKey(parentKey))
            {
                _history.Push(_currentKey); // 항상 모든 탐색은 뒤로 가기를 대비하기 위해 푸시 필요
                _currentKey = parentKey; // 부모로 이동하기 위한 현재 키 갱신

                FolderData currentFolderData = _fileSystemData[_currentKey];

                if (currentFolderData == null)
                    return;

                _view.LoadItems(currentFolderData.Items);
                _window.Text = currentFolderData.Name;

                _view.SetBackEnabled(true); // 여기에서 바로 뒤로 가기를 누르면 자식 폴더로 다시 이동

                // 부모 키를 가지고 있어야 위로 가기 버튼이 활성화
                bool hasParentKey = currentFolderData.ParentKey != null;
                _view.SetUpEnabled(hasParentKey);
            }
        }

        /// <summary>
        /// 현재 폴더가 가진 아이템 항목을 검사해서 새폴더 (1)처럼 고유한 폴더 이름이 생성되도록 하는 메서드
        /// </summary>
        private string GetUniqueFolderName(List<FileSystemItem> items, string baseName)
        {
            string finalName = baseName;
            int counter = 1; // 새 폴더가 이미 있으면 새 폴더 (1)부터 시작

            bool isDuplicate = false; // 이름 중복 체크용

            do // 고유한 이름을 찾을 때까지 반복하되, 일단 한 번은 검사해 봐야 하니 do while
            {
                isDuplicate = false; // 일단 중복 아닌 상태로 초기화

                foreach (FileSystemItem item in items)
                {
                    // 대소문자 구분 없이 이름 비교해서 만약 같으면 중복
                    if (item.Name.Equals(finalName, StringComparison.OrdinalIgnoreCase))
                    {
                        isDuplicate = true;
                        finalName = $"{baseName} ({counter})";
                        counter++;
                        break;
                    }
                }

            } while (isDuplicate); // 위에서 중복이 발견되었다면 계속 반복되도록

            return finalName;
        }

        /// <summary>
        /// 폴더 뷰로부터 새 폴더 생성 요청을 받아 처리하는 메서드
        /// </summary>
        private void OnCreateFolderRequested()
        {
            List<FileSystemItem> currentItems = _fileSystemData[_currentKey].Items;

            // 이름 중복 검사 후 고유한 이름을 받아와 저장
            string newFolderName = GetUniqueFolderName(currentItems, "새 폴더");

            // 새 폴더에 사용할 고유 키 생성
            // Guid는 겹치지 않는 랜덤한 ID를 생성하는 기술로, 16진수 128비트로 표현
            // 이러면 이름이 바뀌더라도 중복되지 않고 변하지 않는 키 생성 가능 
            string newFolderKey = "NAVIGATE_NEWFOLDER_" + Guid.NewGuid().ToString();

            // 키와 이름을 바탕으로 새 파일 시스템 아이템 객체 생성
            FileSystemItem newItem = new FileSystemItem(newFolderName, newFolderKey);

            // 현재 위치에서 폴더를 새로 만들었다면, 새로 만든 폴더의 부모는 곧 현재 폴더
            _fileSystemData.Add(newFolderKey, new FolderData(newFolderName, _currentKey, new List<FileSystemItem>()));

            // 현재 위치 키의 실제 데이터 목록에 새 폴더 추가
            // 딕셔너리가 참조 타입이라서 바로 자동 반영
            _fileSystemData[_currentKey].Items.Add(newItem);

            // 목록이 변경되었으니 폴더 뷰에 갱신 요청
            _view.LoadItems(_fileSystemData[_currentKey].Items);
        }
    }
}