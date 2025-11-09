using System;
using System.Windows.Forms;

namespace windows_xp_like
{
    /// <summary>
    /// FolderView에서 생성할 아이템의 정보를 저장하는 클래스
    /// </summary>
    public class FileSystemItem
    {
        /// <summary>
        /// "지뢰찾기.exe" 같이 화면에 표시될 이름 문자열 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// true면 폴더, false면 파일
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// 파일 또는 특수 폴더를 열었을 때 열릴 폼
        /// </summary>
        public Control ActionControl { get; set; }

        public FileSystemItem(string name, bool isFolder = false, Form actionControl = null)
        {
            Name = name;
            IsFolder = isFolder;
            ActionControl = actionControl;
        }
    }
}