using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace windows_xp_like
{
    /// <summary>
    /// 앱 폼 안에 들어가는 폼 중 키보드를 쓰는 폼이 상속 받아 포커싱 활성화 제어를 구현하는 인터페이스
    /// </summary>
    public interface IAppFormChild
    {
        void SetFocusState(bool isActive);
    }
}