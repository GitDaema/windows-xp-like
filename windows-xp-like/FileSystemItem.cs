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

        /*
        원래는 앱 폼 창을 유지한 채로 껐다 켰다 하는 구조로 계획
        그러나 Dispose로 완전히 사라졌는데 다시 불러오려고 하면 오류 발생  
        따라서 시작하자마자 앱 폼을 만드는 것이 아닌, 앱 폼을 만드는 함수만 만들기
        아이콘을 클릭하면 그때 어떤 컨트롤을 넣어야 할지 정보를 받아와서 만들기
        창을 닫으면 Dispose, 아이콘을 다시 클릭할 때마다 이전 창이 아닌 새로운 창
         */

        /// <summary>
        /// 파일의 열릴 폼을 생성하는 폼 재사용 시 발생하는 오류 해결용 함수
        /// </summary>
        public Func<Control> ActionControlFactory { get; set; }

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
            ActionControlFactory = null;
        }

        /// <summary>
        /// 실행 가능한 파일 또는 특수 폴더에 대한 생성자, 앱 폼안에서 열릴 컨트롤 전달 함수 필요
        /// </summary>
        /// <param name="name"></param>
        /// <param name="controlToLaunch"></param>
        public FileSystemItem(string name, Func<Control> controlToLaunch)
        {
            Name = name;
            IsFolder = false;
            ActionKey = null;
            ActionControlFactory = controlToLaunch;
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
            ActionControlFactory = null;
        }
    }
}