using System.Threading.Tasks;
using Unity.Services.Authentication;

namespace Scripts.Networking
{
    public enum UGSAuthState
    {
        NotAuthenticated,  //인증되지 않음
        Authenticating,  //인증 중
        Authenticated,  //인증 됨
        Error,  //비상
        TimeOut,  //시간 초과
        
    }
    public class UGSAuthWrapper
    {
        public static UGSAuthState AuthState {get; private set; } = UGSAuthState.NotAuthenticated;

        public static async Task<UGSAuthState> DoAuthAsync(int maxTryCount = 5)
        {
            if (AuthState == UGSAuthState.Authenticated)
            {
                return AuthState;
            }
            
            AuthState = UGSAuthState.Authenticating;

            int tryCount = 0;

            while (AuthState == UGSAuthState.Authenticating && tryCount < maxTryCount)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = UGSAuthState.Authenticated;
                    break;
                }
                
                tryCount++;
                await Task.Delay(1000);  //1초 대기후 재시도
            }
            
            //지금은 에러처리 없음
            return AuthState;
        }
        
    }
}