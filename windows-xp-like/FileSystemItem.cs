using System;

namespace windows_xp_like
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public bool IsFolder { get; set; }

        // [추가] 이 아이템이 수행할 액션을 구분하는 '꼬리표'
        // 예: "LAUNCH_APP_A", "LAUNCH_APP_B", "OPEN_FOLDER"
        public string ActionKey { get; set; }

        // [수정] 생성자에서 ActionKey도 받을 수 있게 (선택적)
        public FileSystemItem(string name, bool isFolder = false, string actionKey = null)
        {
            this.Name = name;
            this.IsFolder = isFolder;

            // IsFolder가 true면 기본 ActionKey를 "OPEN_FOLDER"로 설정
            // actionKey가 null일 때만 (외부에서 지정 안 했을 때만)
            if (actionKey == null)
            {
                this.ActionKey = isFolder ? "OPEN_FOLDER" : "OPEN_FILE"; // 기본값
            }
            else
            {
                this.ActionKey = actionKey; // 외부에서 지정한 값 사용
            }
        }
    }
}