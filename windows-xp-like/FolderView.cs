using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace windows_xp_like
{
    public partial class FolderView : UserControl
    {
        public event Action BackClicked; // 디렉토리에서 뒤로 가기 신호
        public event Action UpClicked; // 위로 가기 신호
        public event Action CreateFolderRequested; // 새 폴더 생성 신호

        // 폴더 뷰의 아이템이 더블 클릭되었을 때의 이벤트 신호
        public event Action<FileSystemItem, Image> ItemDoubleClicked;

        // 검색 기능을 위해서 현재 폴더가 가지고 있는 아이템들을 저장하는 리스트
        private List<FileSystemItem> _currentItems = new List<FileSystemItem>();

        public FolderView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 이번에 포커스된 폴더가 가진 파일들의 리스트를 받아서 그 항목들을 리스트 뷰에 아이템으로 넣는 메서드
        /// </summary>
        /// <param name="items"></param>
        public void LoadItems(List<FileSystemItem> items)
        {
            if(items != null) 
            {
                _currentItems = items;
            } 
            else
            {
                _currentItems = new List<FileSystemItem>();
            }

            // 검색창에 텍스트가 있는지 확인하고 필터링해야만 제대로 폴더 항목 표시 가능 
            FilterItems();
        }

        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                // 사용 중인 이미지 리스트의 이미지를 가져오기 위해 리스트 뷰 자체를 가져오기
                ListViewItem lvi = listView1.SelectedItems[0];

                // 선택된 아이템들 중 0번 인덱스 항목의 태그를 이용해서 파일 아이템 가져오기
                FileSystemItem selectedItem = listView1.SelectedItems[0].Tag as FileSystemItem;

                if (selectedItem != null)
                {
                    // 이 아이템이 listView1에서 사용 중인 ImageList의 이미지를 가져옵니다.
                    Image icon = null;
                    // LargeImageList가 설정되어 있고, ImageIndex가 유효한 범위일 때
                    if (listView1.LargeImageList != null && lvi.ImageIndex >= 0)
                    {
                        icon = listView1.LargeImageList.Images[lvi.ImageIndex];
                    }

                    // 데스크톱에게 이벤트를 보내서 선택된 아이템에 맞는 실행 역할 넘기기
                    // 이벤트 방식 안 쓰면 public 메서드 직접 호출 방식이라 너무 의존
                    ItemDoubleClicked?.Invoke(selectedItem, icon);
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
            CreateFolderRequested?.Invoke();
        }

        private void 삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 리스트 뷰의 기본 기능을 이용해 여러 아이템을 선택할 수도 있으므로 전부 순회
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                item.Remove(); // 실제 삭제 기능은 아니지만 우선 리스트 뷰에서는 제거
            }
        }

        private void tsbBack_Click(object sender, EventArgs e)
        {
            // 뒤로 가기 버튼 자체는 자기가 뭘 해야 할지 모르며, 일부러 연결하면 오히려 과한 의존성 발생
            // 따라서 뒤로 가달라는 신호만 보내 이를 받는 데스크탑에서 실제 뒤로 가기 행동 수행
            BackClicked?.Invoke();
        }

        /// <summary>
        /// 뒤로 가기 버튼의 활성화 여부를 바꾸는 메서드
        /// </summary>
        /// <param name="enabled">true면 뒤로 가기 버튼 활성화</param>
        public void SetBackEnabled(bool enabled)
        {
            if (tsbBack != null)
            {
                tsbBack.Enabled = enabled;
            }
        }

        /// <summary>
        /// 위(부모 폴더)로 가기 버튼의 활성화 여부를 바꾸는 메서드
        /// </summary>
        /// <param name="enabled"></param>
        public void SetUpEnabled(bool enabled)
        {
            if (tsbUp != null)
            {
                tsbUp.Enabled = enabled;
            }
        }

        private void tsbUp_Click(object sender, EventArgs e)
        {
            UpClicked?.Invoke(); // 마찬가지로 위로 가달라는 신호를 데스크탑에 전달
        }

        private void 큰아이콘LToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setView(View.LargeIcon);
        }

        private void 자세히DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setView(View.Details);
        }

        private void 간단히SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setView(View.SmallIcon);
        }

        /// <summary>
        /// 리스트 뷰의 보기 모드를 변경하는 메서드
        /// </summary>
        /// <param name="viewMode"></param>
        private void setView(View viewMode)
        {
            listView1.View = viewMode;
        }

        private void tstSearch_TextChanged(object sender, EventArgs e)
        {
            FilterItems();
        }

        private void FilterItems()
        {
            // 우선은 대소문자 구별하지 않고 필터링
            string filter = tstSearch.Text.Trim().ToLower();

            listView1.Items.Clear(); // 리스트 뷰 비우고 시작

            // 검색어가 없으면 모든 항목을 표시해야 하므로 원본 리스트 그대로 표시
            List<FileSystemItem> itemsToShow;
            if(string.IsNullOrEmpty(filter))
            {
                itemsToShow = _currentItems;
            }
            else // 있으면 필터링
            {
                // Where에서 끝나면 리스트 형식이 아니므로 오류, 반드시 ToList()를 이용해 새 리스트 형식으로 붙이기
                itemsToShow = _currentItems
                    .Where(item => item.Name.ToLower().Contains(filter))
                    .ToList();
            }


            // 필터링된 아이템들로 리스트 뷰를 다시 채우기 위해 순회
            foreach (var item in itemsToShow)
            {
                int iconIndex = 0;
                string itemType;

                // 아이템이 폴더라면 0번(폴더) 아이콘, 아니면 1번(파일) 아이콘
                // 추가적으로 자세히 보기 모드를 위한 서브 아이템 유형 정보 추가
                if (item.IsFolder)
                {
                    iconIndex = 0;
                    itemType = "파일 폴더";
                }
                else
                {
                    iconIndex = 1;

                    // 확장자 정보는 따로 없어서 뒤에 붙은 확장자 텍스트로 구분
                    if (item.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        itemType = "응용 프로그램";
                    }
                    else if (item.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        itemType = "텍스트 문서";
                    }
                    else
                    {
                        itemType = "알 수 없는 파일"; // 혹시 모를 기본값
                    }
                }

                // 리스트 뷰에 들어갈 수 있는 형태 자리를 생성한 후
                ListViewItem lvi = new ListViewItem(item.Name, iconIndex);
                lvi.Tag = item; // 태그를 이용해 어떤 파일 아이템인지 전달

                // 두 번째 열에 표시될 텍스트를 SubItem으로 추가
                lvi.SubItems.Add(itemType);

                listView1.Items.Add(lvi); // 리스트 뷰에 실제 추가
            }
        }
    }
}