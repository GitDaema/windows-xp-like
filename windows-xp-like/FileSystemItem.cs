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
        /// 파일 또는 특수 폴더에 대해 열릴 폼
        /// </summary>
        public Control ActionControl { get; set; }

        /// <summary>
        /// 일반적인 폴더에 대해 탐색용 구분 키 
        /// </summary>
        public string ActionKey { get; set; }

        // 오버로딩을 이용해 폴더 생성과 파일 생성을 구분
        // 폴더는 탐색용 ActionKey, 파일 종류는 앱 폼에서 열 ActionControl을 가짐

        public FileSystemItem(string name, string navigationKey)
        {
            Name = name;
            IsFolder = true;
            ActionKey = navigationKey;
            ActionControl = null;
        }

        /// <summary>
        /// 실행 가능한 파일 또는 특수 폴더에 대한 생성자, 앱 폼안에서 열릴 컨트롤 필요
        /// </summary>
        /// <param name="name"></param>
        /// <param name="controlToLaunch"></param>
        public FileSystemItem(string name, Control controlToLaunch)
        {
            Name = name;
            IsFolder = false;
            ActionKey = null;
            ActionControl = controlToLaunch;
        }

        /// <summary>
        /// 아무 동작 없는 미구현 폴더나 파일을 위한 생성자
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isFolder"></param>
        public FileSystemItem(string name, bool isFolder = false)
        {
            Name = name;
            IsFolder = isFolder;
            ActionKey = null;
            ActionControl = null;
        }
    }
}