using Silmoon.Extension;
using Silmoon.Models;
using Silmoon.Models.Identities;
using Silmoon.Models.Identities.Enums;
using Silmoon.Web.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Silmoon.Web
{
    public abstract class UserSessionManager<TUser> : System.Web.SessionState.IRequiresSessionState where TUser : IDefaultUserIdentity
    {
        RSACryptoServiceProvider rsa = null;
        string cookieDomain = null;
        DateTime cookieExpires = default;
        int sessionTimeout = 30;

        public int SessionTimeout
        {
            get
            {
                if (HttpContext.Current.Session != null)
                    sessionTimeout = HttpContext.Current.Session.Timeout;
                return sessionTimeout;
            }
            set
            {
                sessionTimeout = value;
                if (HttpContext.Current.Session != null)
                    HttpContext.Current.Session.Timeout = value;
            }
        }
        public event EventHandler UserLogin;
        public event EventHandler UserLogout;
        public event UserSessionHanlder OnRequestRefreshUserSession;
        public event UserTokenHanlder OnRequestUserToken;

        public string Username
        {
            get
            {
                return User?.Username;
            }
        }
        public IdentityRole? Role
        {
            get
            {
                return User?.Role;
            }
        }
        [Obsolete]
        public LoginState State
        {
            get
            {
                if (HttpContext.Current.Session["___silmoon_state"] != null)
                {
                    int.TryParse(HttpContext.Current.Session["___silmoon_state"].ToString(), out int result);
                    return (LoginState)result;
                }
                else return LoginState.None;
            }
            private set
            {
                HttpContext.Current.Session["___silmoon_state"] = (int)value;
            }
        }
        public TUser User
        {
            get
            {
                if (HttpContext.Current.Session["___silmoon_user"] != null)
                    return (TUser)HttpContext.Current.Session["___silmoon_user"];
                else return default;
            }
            private set { HttpContext.Current.Session["___silmoon_user"] = value; }
        }
        public bool IsSignin
        {
            get
            {
                return State == LoginState.Login;
            }
        }

        public RSACryptoServiceProvider RSACookiesCrypto
        {
            get { return rsa; }
            set { rsa = value; }
        }
        public string CookieDomain
        {
            get { return cookieDomain; }
            set { cookieDomain = value; }
        }
        public DateTime CookieExpires
        {
            get { return cookieExpires; }
            set { cookieExpires = value; }
        }

        public UserSessionManager() : this(null) { }
        public UserSessionManager(string cookieDomain)
        {
            this.cookieDomain = cookieDomain;
        }

        public bool GteRole(IdentityRole role)
        {
            var srule = Role;
            if (srule.HasValue)
                return srule >= role;
            else return false;
        }


        public object ReadSession(string key)
        {
            return HttpContext.Current.Session[key];
        }
        /// <summary>
        /// ���UserSession�ĵ�¼״̬��
        /// ����û���¼���Ὣʵ����ֵ��ViewBag.UserSession���û������ݸ�ֵ��ViewBag.User��
        /// </summary>
        /// <param name="controller">controller����null���������Զ�ת���������ڵ�¼״̬�²��ᣬ���û��Ựʵ����ֵ��ViewBag.UserSession���û������ݲ��ḳֵ��ViewBag.User��</param>
        /// <param name="IsRole">�������IsRule��ͬʱ�ж��û���ɫ</param>
        /// <param name="requestRefreshUserSession">������True�������OnRequestRefreshUserSession�¼����������»�ȡ�û�ʵ��</param>
        /// <param name="isAppApiRequest">�ж��Ƿ���Api����������Api����᷵��Redirect����ΪApi���󣬽�����Json</param>
        /// <param name="signInUrl">������controller����ȡ�û�UserToken�Ȳ������������û�е�¼��ʹ��ת����������ָ����URL��</param>
        /// <returns></returns>
        public ActionResult MvcSessionChecking(Controller controller, IdentityRole? IsRole, bool requestRefreshUserSession = false, bool isAppApiRequest = false, string signInUrl = "~/User/Signin?url=$SigninUrl")
        {
            controller.ViewBag.UserSession = this;
            signInUrl = signInUrl?.Replace("$SigninUrl", controller.Server.UrlEncode(controller.Request.RawUrl));
            var username = controller.Request.QueryString["Username"];
            var userToken = controller.Request.QueryString["UserToken"] ?? controller.Request.QueryString["AppUserToken"];
            var tokenNoSession = controller.Request.QueryString["TokenNoSession"].ToBool(false, false);
            var ignoreUserToken = controller.Request.QueryString["ignoreUserToken"].ToBool(false, false);

            if (!IsSignin || (!userToken.IsNullOrEmpty() && !ignoreUserToken))
            {
                if (userToken.IsNullOrEmpty())
                {
                    ///���ǵ�¼״̬������û���ṩAppUserToken������¡�
                    if (controller.Request.IsAjaxRequest())
                        return new JsonResult { Data = StateFlag.Create(false, -9999, "no signin."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    else return new RedirectResult(signInUrl);
                }
                else
                {
                    ///�ṩ��AppUserToken�������
                    if (userToken.ToLower() == "null")
                    {
                        ///�ṩ��AppUserToken���ַ�����null��
                        if (controller.Request.IsAjaxRequest() || isAppApiRequest)
                            return new JsonResult { Data = StateFlag.Create(false, -9999, "usertoken is \"null\"."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        else return new RedirectResult(signInUrl);
                    }
                    else
                    {
                        ///����UserToken��¼��֤������̣���ȡ�û�ʵ�塣
                        var userInfo = OnRequestUserToken(username, userToken);
                        if (userInfo != null)
                        {
                            ///���AppUserToken��֤���̷������û�ʵ�塣
                            User = userInfo;
                            if (!tokenNoSession) Signin(User);

                            ///ʹ��UserToken��¼����
                            if (IsRole.HasValue && Role < IsRole)
                            {
                                if (controller.Request.IsAjaxRequest() || isAppApiRequest)
                                    return new JsonResult { Data = StateFlag.Create(false, -9999, "access denied."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                else return new ContentResult() { Content = "access denied", ContentType = "text/plain" };
                            }
                            controller.ViewBag.User = User;
                            return null;
                        }
                        else
                        {
                            if (controller.Request.IsAjaxRequest() || isAppApiRequest)
                                return new JsonResult { Data = StateFlag.Create(false, -9999, "OnRequestUserToken return null."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                            else
                            {
                                ///�������һ����ͻ�������ǰ�ǵ�¼״̬������ʹ��AppUserToken��¼��AppUserToken��¼ʧ�ܣ���ת������¼ҳ�棬�����������ڵ�¼״̬�����ٴ����ص�ǰҳ�棬�������ѭ����
                                return new RedirectResult(signInUrl);
                            }
                        }
                    }
                }
            }
            else
            {
                if (requestRefreshUserSession)
                {
                    var userInfo = onRequestRefreshUserSession();
                    if (userInfo != null)
                        User = userInfo;
                    else
                    {
                        if (controller.Request.IsAjaxRequest() || isAppApiRequest)
                            return new JsonResult { Data = StateFlag.Create(false, -9999, "onRequestRefreshUserSession return null."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        else return new RedirectResult(signInUrl);
                    }
                }

                if (IsRole.HasValue)
                {
                    if (Role < IsRole)
                    {
                        if (controller.Request.IsAjaxRequest() || isAppApiRequest)
                            return new JsonResult { Data = StateFlag.Create(false, -9999, "access denied."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        else return new ContentResult() { Content = "access denied", ContentType = "text/plain" };
                    }
                }

                controller.ViewBag.User = User;
                return null;
            }
        }
        public ActionResult ActionFilterChecking(HttpContextBase httpContext, IdentityRole? IsRole, bool requestRefreshUserSession = false, bool isAppApiRequest = false, string signInUrl = "~/User/Signin?url=$SigninUrl")
        {
            signInUrl = signInUrl?.Replace("$SigninUrl", httpContext.Server.UrlEncode(httpContext.Request.RawUrl));
            var username = httpContext.Request.QueryString["Username"];
            var userToken = httpContext.Request.QueryString["UserToken"] ?? httpContext.Request.QueryString["AppUserToken"];
            var tokenNoSession = httpContext.Request.QueryString["TokenNoSession"].ToBool(false, false);
            var ignoreUserToken = httpContext.Request.QueryString["ignoreUserToken"].ToBool(false, false);

            if (!IsSignin || (!userToken.IsNullOrEmpty() && !ignoreUserToken))
            {
                if (userToken.IsNullOrEmpty())
                {
                    ///���ǵ�¼״̬������û���ṩAppUserToken������¡�
                    if (httpContext.Request.IsAjaxRequest())
                        return new JsonResult { Data = StateFlag.Create(false, -9999, "no signin."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    else return new RedirectResult(signInUrl);
                }
                else
                {
                    ///�ṩ��AppUserToken�������
                    if (userToken.ToLower() == "null")
                    {
                        ///�ṩ��AppUserToken���ַ�����null��
                        if (httpContext.Request.IsAjaxRequest() || isAppApiRequest)
                            return new JsonResult { Data = StateFlag.Create(false, -9999, "usertoken is \"null\"."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        else return new RedirectResult(signInUrl);
                    }
                    else
                    {
                        ///����UserToken��¼��֤������̣���ȡ�û�ʵ�塣
                        var userInfo = OnRequestUserToken(username, userToken);
                        if (userInfo != null)
                        {
                            ///���AppUserToken��֤���̷������û�ʵ�塣
                            User = userInfo;
                            if (!tokenNoSession) Signin(User);

                            ///ʹ��UserToken��¼����
                            if (IsRole.HasValue && Role < IsRole)
                            {
                                if (httpContext.Request.IsAjaxRequest() || isAppApiRequest)
                                    return new JsonResult { Data = StateFlag.Create(false, -9999, "access denied."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                                else return new ContentResult() { Content = "access denied", ContentType = "text/plain" };
                            }
                            return null;
                        }
                        else
                        {
                            if (httpContext.Request.IsAjaxRequest() || isAppApiRequest)
                                return new JsonResult { Data = StateFlag.Create(false, -9999, "OnRequestUserToken return null."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                            else
                            {
                                ///�������һ����ͻ�������ǰ�ǵ�¼״̬������ʹ��AppUserToken��¼��AppUserToken��¼ʧ�ܣ���ת������¼ҳ�棬�����������ڵ�¼״̬�����ٴ����ص�ǰҳ�棬�������ѭ����
                                return new RedirectResult(signInUrl);
                            }
                        }
                    }
                }
            }
            else
            {
                if (requestRefreshUserSession)
                {
                    var userInfo = onRequestRefreshUserSession();
                    if (userInfo != null)
                        User = userInfo;
                    else
                    {
                        if (httpContext.Request.IsAjaxRequest() || isAppApiRequest)
                            return new JsonResult { Data = StateFlag.Create(false, -9999, "onRequestRefreshUserSession return null."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        else return new RedirectResult(signInUrl);
                    }
                }

                if (IsRole.HasValue)
                {
                    if (Role < IsRole)
                    {
                        if (httpContext.Request.IsAjaxRequest() || isAppApiRequest)
                            return new JsonResult { Data = StateFlag.Create(false, -9999, "access denied."), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        else return new ContentResult() { Content = "access denied", ContentType = "text/plain" };
                    }
                }
                return null;
            }
        }

        public void WriteSession(string key, string value)
        {
            HttpContext.Current.Session.Timeout = sessionTimeout;
            HttpContext.Current.Session[key] = value;
        }

        public bool LoginFromCookie()
        {
            if (rsa == null) return false;
            if (HttpContext.Current.Request.Cookies["___silmoon_user_session"] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["___silmoon_user_session"].Value))
            {
                byte[] data = Convert.FromBase64String(HttpContext.Current.Request.Cookies["___silmoon_user_session"].Value);
                data = rsa.Decrypt(data, true);
                string username = Encoding.Default.GetString(data, 2, BitConverter.ToInt16(data, 0));
                string password = Encoding.Default.GetString(data, BitConverter.ToInt16(data, 0) + 4, data[BitConverter.ToInt16(data, 0) + 2]);
                return CookieRelogin(username, password);
            }
            else
            {
                return false;
            }
        }
        public bool LoginCrossLoginCookie()
        {
            if (rsa == null) return false;
            if (HttpContext.Current.Request.Cookies["___silmoon_cross_session"] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["___silmoon_cross_session"].Value))
            {
                byte[] data = Convert.FromBase64String(HttpContext.Current.Request.Cookies["___silmoon_cross_session"].Value);
                data = rsa.Decrypt(data, true);
                string username = Encoding.Default.GetString(data, 2, BitConverter.ToInt16(data, 0));
                string password = Encoding.Default.GetString(data, BitConverter.ToInt16(data, 0) + 4, data[BitConverter.ToInt16(data, 0) + 2]);
                bool result = CrossLogin(username, password);
                HttpContext.Current.Response.Cookies.Remove("___silmoon_cross_session");
                return result;
            }
            else
            {
                return false;
            }
        }
        public bool LoginFormToken(string token)
        {
            if (rsa == null) return false;
            byte[] data = Convert.FromBase64String(token);
            data = rsa.Decrypt(data, true);
            string username = Encoding.Default.GetString(data, 2, BitConverter.ToInt16(data, 0));
            string password = Encoding.Default.GetString(data, BitConverter.ToInt16(data, 0) + 4, data[BitConverter.ToInt16(data, 0) + 2]);
            bool result = TokenLogin(username, password);
            return result;
        }

        public virtual bool CookieRelogin(string username, string password)
        {
            return false;
        }
        public virtual bool CrossLogin(string username, string password)
        {
            return false;
        }
        public virtual bool TokenLogin(string username, string password)
        {
            return false;
        }

        public string GetUserToken()
        {
            if (rsa == null) return "";

            byte[] usernameData = Encoding.Default.GetBytes(User.Username);
            byte[] passwordData = Encoding.Default.GetBytes(User.Password);
            byte[] data = new byte[4 + usernameData.Length + passwordData.Length];

            Array.Copy(BitConverter.GetBytes((short)usernameData.Length), 0, data, 0, 2);
            Array.Copy(usernameData, 0, data, 2, usernameData.Length);
            Array.Copy(BitConverter.GetBytes((short)passwordData.Length), 0, data, usernameData.Length + 2, 2);
            Array.Copy(passwordData, 0, data, usernameData.Length + 4, passwordData.Length);

            data = rsa.Encrypt(data, true);
            return Convert.ToBase64String(data);
        }
        /// <summary>
        /// д��һ����ָ��ʱ����ڵġ�___silmoon_user_session����Cookie���Ա��´��Զ���¼��
        /// </summary>
        /// <param name="Expires">����ʱ��</param>
        public void WriteCookie(DateTime Expires = default(DateTime))
        {
            if (rsa != null)
            {
                if (Expires != default) cookieExpires = Expires;

                if (cookieDomain != null)
                    HttpContext.Current.Response.Cookies["___silmoon_user_session"].Domain = cookieDomain;

                if (CookieExpires != default(DateTime))
                    HttpContext.Current.Response.Cookies["___silmoon_user_session"].Expires = cookieExpires;

                HttpContext.Current.Response.Cookies["___silmoon_user_session"].Value = GetUserToken();
            }
        }
        public void WriteCrossLoginCookie(string domain = null)
        {
            if (rsa != null)
            {
                HttpContext.Current.Response.Cookies["___silmoon_cross_session"].Value = GetUserToken();
                if (!string.IsNullOrEmpty(domain))
                    HttpContext.Current.Response.Cookies["___silmoon_cross_session"].Domain = domain;
                HttpContext.Current.Response.Cookies["___silmoon_cross_session"].Expires = DateTime.Now.AddDays(1);
            }
        }

        [Obsolete]
        public virtual void DoLogin(TUser user)
        {
            DoLogin(user.Username, user.Password, user.Role, user);
        }
        [Obsolete]
        public virtual void DoLogin(string username, string password, TUser user)
        {
            DoLogin(username, password, user.Role, user);
        }
        [Obsolete]
        public virtual void DoLogin(string username, string password, IdentityRole role, TUser user = default)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException($"��{nameof(username)}������Ϊ null ��ա�", nameof(username));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException($"��{nameof(password)}������Ϊ null ��ա�", nameof(password));
            }

            HttpContext.Current.Session.Timeout = sessionTimeout;
            User = user;
            State = LoginState.Login;
            UserLogin?.Invoke(this, EventArgs.Empty);
        }
        public virtual void Signin(TUser user)
        {
            Signin(user.Username, user.Role, user);
        }
        public virtual void Signin(string username, TUser user)
        {
            Signin(username, user.Role, user);
        }
        public virtual void Signin(string username, IdentityRole role, TUser user = default)
        {
            HttpContext.Current.Session.Timeout = sessionTimeout;
            User = user;
            State = LoginState.Login;
            UserLogin?.Invoke(this, EventArgs.Empty);
        }
        public virtual void Signout()
        {
            State = LoginState.Logout;
            HttpContext.Current.Session.Remove("___silmoon_username");
            HttpContext.Current.Session.Remove("___silmoon_password");
            HttpContext.Current.Session.Remove("___silmoon_level");
            HttpContext.Current.Session.Remove("___silmoon_userflag");
            HttpContext.Current.Session.Remove("___silmoon_object");
            HttpContext.Current.Session.Remove("___silmoon_user");

            if (UserLogout != null) UserLogout(this, EventArgs.Empty);
        }

        public void ClearCrossCookie()
        {
            if (HttpContext.Current.Request.Cookies["___silmoon_cross_session"] != null)
            {
                if (cookieDomain != null)
                    HttpContext.Current.Response.Cookies["___silmoon_cross_session"].Domain = cookieDomain;
                HttpContext.Current.Response.Cookies["___silmoon_cross_session"].Expires = DateTime.Now.AddYears(-10);
            }
        }
        public void ClearUserCookie()
        {
            if (cookieDomain != null)
                HttpContext.Current.Response.Cookies["___silmoon_user_session"].Domain = cookieDomain;
            HttpContext.Current.Response.Cookies["___silmoon_user_session"].Expires = DateTime.Now.AddYears(-10);
        }
        public virtual void Clearup()
        {
            ClearUserCookie();
            ClearCrossCookie();
            Signout();
        }

        TUser onRequestRefreshUserSession()
        {
            if (OnRequestRefreshUserSession == null)
                return default(TUser);
            else
            {
                return OnRequestRefreshUserSession(User);
            }
        }
        public delegate TUser UserSessionHanlder(TUser User);
        public delegate TUser UserTokenHanlder(string Username, string UserToken);
    }


    [Serializable]
    public enum LoginState
    {
        None = 0,
        Login = 1,
        Logout = -1,
    }
}