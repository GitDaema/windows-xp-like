using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace windows_xp_like
{
    public partial class FolderView : UserControl
    {
        // 폴더 뷰의 아이템이 더블 클릭되었을 때 
        public event Action<FileSystemItem> ItemDoubleClicked;

        public FolderView()
        {
            InitializeComponent();
        }

        private void FolderView_Load(object sender, EventArgs e)
        {
            List<FileSystemItem> testDataList = new List<FileSystemItem>
            {
                new FileSystemItem("내 문서", true),
                new FileSystemItem("Game.exe", false, new GameForm()),
                new FileSystemItem("메모장.txt", false)
            };

            LoadItems(testDataList);
        }

        /// <summary>
        /// FileSystemItem 리스트를 받아서 그 항목들을 리스트 뷰에 아이템으로 넣는 메서드
        /// </summary>
        /// <param name="items"></param>
        public void LoadItems(List<FileSystemItem> items)
        {
            listView1.Items.Clear(); // 혹시 모르니 아이템 항목 초기화 초기화

            foreach (FileSystemItem item in items) // 리스트의 모든 아이템 순회
            {
                // 아이템이 폴더라면 0번(폴더) 아이콘, 아니면 1번(파일) 아이콘
                int iconIndex;
                if (item.IsFolder)
                    iconIndex = 0;
                else
                    iconIndex = 1;

                // 리스트 뷰에 들어갈 수 있는 형태 자리를 생성한 후
                ListViewItem lvi = new ListViewItem(item.Name, iconIndex);
                lvi.Tag = item; // 태그를 이용해 어떤 파일 아이템인지 전달

                listView1.Items.Add(lvi); // 리스트 뷰에 실제 추가
            }
        }

        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                // 선택된 아이템들 중 0번 인덱스 항목의 태그를 이용해서 파일 아이템 가져오기
                FileSystemItem selectedItem = listView1.SelectedItems[0].Tag as FileSystemItem;

                if (selectedItem != null)
                {
                    // 데스크톱에게 이벤트를 보내서 선택된 아이템에 맞는 실행 역할 넘기기
                    // 이벤트 방식 안 쓰면 public 메서드 직접 호출 방식이라 너무 의존
                    ItemDoubleClicked?.Invoke(selectedItem);
                }
            }
        }

        private void ListView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // 레이블이 null이면 사용자가 esc 키 등을 눌러서 취소한 것이니 미반영
            if (string.IsNullOrWhiteSpace(e.Label))
            {
                e.CancelEdit = true;
                return;
            }

            // 이름이 바뀌었으니 태그에 저장된 원본 아이템의 이름 속성도 함께 변경
            FileSystemItem item = listView1.Items[e.Item].Tag as FileSystemItem;
            if (item != null)
            {
                item.Name = e.Label;
            }
        }

        private void 폴더ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 파일 아이템 형식으로 새 폴더 생성, 아직은 미구현된
            FileSystemItem newItem = new FileSystemItem("새 폴더", true);

            // 리스트 뷰에 추가 후 태그 연결
            ListViewItem lvi = new ListViewItem(newItem.Name, 0);
            lvi.Tag = newItem;

            listView1.Items.Add(lvi);

            // 새로 폴더 생성하면 무조건 이름 바꾸기 상태로 진입
            lvi.BeginEdit();
        }

        private void 삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 리스트 뷰의 기본 기능을 이용해 여러 아이템을 선택할 수도 있으므로 전부 순회
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                item.Remove(); // 실제 삭제 기능은 아니지만 우선 리스트 뷰에서는 제거
            }
        }
    }
}