using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace windows_xp_like
{
    public partial class FolderView : UserControl
    {
        // [핵심] 6단계에서 DesktopForm이 이 신호를 받아서 처리합니다.
        public event Action<FileSystemItem> ItemDoubleClicked;

        public FolderView()
        {
            InitializeComponent();

            // 디자이너에서 설정했지만, 코드로도 확인
            this.listView1.DoubleClick += ListView1_DoubleClick;
            this.listView1.AfterLabelEdit += ListView1_AfterLabelEdit;
        }

        // Form_Load와 동일한 역할
        private void FolderView_Load(object sender, EventArgs e)
        {
            // 컨트롤이 처음 로드될 때 테스트용 아이템들을 채워 넣습니다.
            LoadItems(GetMockData());
        }

        // 테스트용 '가짜' 데이터를 만듭니다.
        private List<FileSystemItem> GetMockData()
        {
            return new List<FileSystemItem>
            {
                new FileSystemItem("내 문서", true),
                new FileSystemItem("Game.exe", false, "LAUNCH_APP_A"),
                new FileSystemItem("메모장.txt", false)
            };
        }

        // FileSystemItem 리스트를 받아서 ListView에 아이템을 채웁니다.
        public void LoadItems(List<FileSystemItem> items)
        {
            listView1.Items.Clear(); // 기존 아이템 삭제

            foreach (var item in items)
            {
                // IsFolder가 true면 0번(폴더) 아이콘, 아니면 1번(파일) 아이콘
                int iconIndex = item.IsFolder ? 0 : 1;

                ListViewItem lvi = new ListViewItem(item.Name, iconIndex);
                lvi.Tag = item; // [중요] ListViewItem에 원본 데이터(FileSystemItem)를 저장

                listView1.Items.Add(lvi);
            }
        }

        // ListView에서 아이템을 더블클릭했을 때
        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                // Tag에 저장했던 원본 FileSystemItem 데이터를 가져옵니다.
                FileSystemItem selectedItem = listView1.SelectedItems[0].Tag as FileSystemItem;

                if (selectedItem != null)
                {
                    // "나는 모르겠다. 주인에게 이 아이템을 열어달라고 신호(Event)를 보낸다."
                    ItemDoubleClicked?.Invoke(selectedItem);
                }
            }
        }

        // 아이템 이름 바꾸기가 끝났을 때
        private void ListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // e.Label이 null이면 사용자가 취소(ESC)한 것
            if (string.IsNullOrWhiteSpace(e.Label))
            {
                e.CancelEdit = true; // 이름 바꾸기 취소
                return;
            }

            // Tag에 저장된 원본 데이터의 Name 속성도 같이 변경
            FileSystemItem item = listView1.Items[e.Item].Tag as FileSystemItem;
            if (item != null)
            {
                item.Name = e.Label;
            }
        }


        // [디자이너]에서 "새로 만들기 > 폴더" 메뉴를 더블클릭하면 이 함수가 생성됩니다.
        // (만약 함수 이름이 다르면 디자이너의 이벤트 속성에서 직접 연결해야 합니다.)
        private void 폴더ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. 새 가짜 데이터 생성
            FileSystemItem newItem = new FileSystemItem("새 폴더", true);

            // 2. ListView에 아이템 추가 (0번 = 폴더 아이콘)
            ListViewItem lvi = new ListViewItem(newItem.Name, 0);
            lvi.Tag = newItem; // 데이터 연결

            listView1.Items.Add(lvi);

            // 3. 방금 만든 아이템의 이름 바꾸기 모드 시작
            lvi.BeginEdit();
        }

        // [디자이너]에서 "삭제" 메뉴를 더블클릭하면 이 함수가 생성됩니다.
        private void 삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 선택된 모든 아이템을 순회하며 삭제
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                // (실제 파일 삭제 로직은 없으므로)
                item.Remove(); // ListView에서만 제거
            }
        }
    }
}