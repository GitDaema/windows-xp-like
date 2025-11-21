using System;
using System.Drawing;
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

        /// <summary>
        /// 실행 파일이 열려 앱 폼이 생성됐을 때의 초기 위치
        /// </summary>
        public Point InitialLocation { get; set; }
        /// <summary>
        /// 실행 파일이 열려 앱 폼이 생성됐을 때의 초기 사이즈
        /// </summary>
        public Size InitialSize { get; set; }
        /// <summary>
        /// 실행 파일이 열려 앱 폼이 생성됐을 때 그 앱 폼의 비율 고정 여부, true면 비율 고정
        /// </summary>
        public bool KeepAspectRatio { get; set; }
        /// <summary>
        /// 리스트 뷰에서 사용할 아이콘 인덱스 번호
        /// </summary>
        public int IconIndex { get; set; }

        /// <summary>
        /// 단순 폴더로, 탐색에 필요한 네비게이션 키만 갖는 생성자
        /// </summary>
        /// <param name="name"></param>
        /// <param name="navigationKey"></param>
        public FileSystemItem(string name, string navigationKey, int iconIndex = -1)
        {
            Name = name;
            IsFolder = true;
            ActionKey = navigationKey;
            ActionControlFactory = null;

            if (iconIndex == -1)
                IconIndex = 0;
            else
                IconIndex = iconIndex;
        }

        /// <summary>
        /// 앱 폼이 열리는 실행 파일 중 내부 컨트롤 생성용 함수를 포함한 창 초기 위치, 크기, 비율 고정 여부 등을 명시적으로 초기화하는 생성자
        /// </summary>
        /// <param name="name"></param>
        /// <param name="controlToLaunch"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="keepAspect"></param>
        public FileSystemItem(string name, Func<Control> controlToLaunch, Point location, Size size, bool keepAspect, int iconIndex = -1)
        {
            Name = name;
            IsFolder = false;
            ActionKey = null;
            ActionControlFactory = controlToLaunch;

            InitialLocation = location;
            InitialSize = size;
            KeepAspectRatio = keepAspect;

            if (iconIndex == -1)
                IconIndex = 1;
            else
                IconIndex = iconIndex;
        }

        /// <summary>
        /// 아무 동작 없는 미구현 폴더나 파일을 위한 생성자
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isFolder"></param>
        public FileSystemItem(string name, bool isFolder = false, int iconIndex = -1)
        {
            Name = name;
            IsFolder = isFolder;
            ActionKey = null;
            ActionControlFactory = null;

            if (iconIndex == -1)
            {
                if (isFolder)
                    IconIndex = 0;
                else
                    IconIndex = 1;
            }
            else
                IconIndex = iconIndex;
        }
    }
}