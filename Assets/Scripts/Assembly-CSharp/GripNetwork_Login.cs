using Gamespy.Authentication;
using Gamespy.Common;
using System;
using UnityEngine;

public class GripNetwork_Login : MonoBehaviour
{
    private static GripNetwork_Login instance;

    private RequestState loginUserState;

    private Account acctManager;

    private string gameName;

    private int gameId;

    private string secretKey;

    private string accessKey;

    private int partnerCode;

    private int namespaceId;

    private string mNickName;

    private string mPassword;

    private string mEmail;

    private static Action<GripNetwork.Result> mLoginCallback;

    private string stackTrace;

    public Action updateAction;

    public static GripNetwork_Login Instance
    {
        get
        {
            if (ApplicationUtilities.HasShutdown)
            {
                return null;
            }
            if (instance == null)
            {
                Reset();
            }
            return instance;
        }
    }

    public bool Ready
    {
        get
        {
            if (loginUserState == RequestState.Complete && acctManager != null && acctManager.Authenticate_Result == Account.AuthenticateResult.Success)
            {
                return true;
            }
            return false;
        }
    }

    public bool Busy
    {
        get
        {
            return acctManager != null && loginUserState != RequestState.Complete;
        }
    }

    public Account Manager
    {
        get
        {
            return acctManager;
        }
    }

    public GripNetwork_Login()
    {
        gameName = GeneralConfig.GameSpyName;
        gameId = GeneralConfig.GameSpyID;
        secretKey = GeneralConfig.GameSpySecretKey;
        accessKey = GeneralConfig.GameSpyAccessKey;
        partnerCode = GeneralConfig.GameSpyPartnerCode;
        namespaceId = GeneralConfig.GameSpyNamespaceID;
        LogOut();
    }

    public static void Reset()
    {
        GameObject gameObject = null;
        if (instance != null)
        {
            gameObject = instance.gameObject;
            ObjectUtils.DestroyImmediate(instance);
        }
        if (gameObject == null)
        {
            gameObject = new GameObject("GripNetworkLogin");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }
        instance = gameObject.AddComponent<GripNetwork_Login>();
    }

    public void Update()
    {
        if (updateAction != null)
        {
            updateAction();
        }
    }

    public void Login(string nickName, string password, Action<GripNetwork.Result> loginCallback)
    {
        mLoginCallback = loginCallback;

        mNickName = "something123";
        mPassword = "something1234";

        UnityEngine.Debug.Log("it worked????");
        WhenDone(GripNetwork.Result.Success);
    }

    public void Create(string nickName, string password, string email, Action<GripNetwork.Result> loginCallback)
    {
        mLoginCallback = loginCallback;
        stackTrace = GenericUtils.StackTrace();
        try
        {
            if (acctManager != null)
            {
                WhenDone(GripNetwork.Result.Failed);
                return;
            }
            acctManager = new Account();
            loginUserState = RequestState.Beginning;
            mNickName = nickName;
            mPassword = password;
            mEmail = email;
            acctManager.SetGameCredentials(gameName, gameId, secretKey, accessKey);
            updateAction = MyCreateUpdate;
        }
        catch (Exception)
        {
            WhenDone(GripNetwork.Result.Failed);
        }
    }

    public void LogOut()
    {
        acctManager = null;
        loginUserState = RequestState.Beginning;
        updateAction = null;
    }

    private void MyLoginUpdate()
    {
        try
        {
            loginUserState = acctManager.Authenticate(partnerCode, namespaceId, mNickName, mPassword);
            if (loginUserState == RequestState.Complete)
            {
                updateAction = null;
                if (acctManager.Authenticate_Result != 0)
                {
                    LogOut();
                    WhenDone(GripNetwork.Result.Failed);
                }
                else
                {
                    WhenDone(GripNetwork.Result.Success);
                }
            }
        }
        catch (Exception)
        {
            WhenDone(GripNetwork.Result.Failed);
        }
    }

    private void MyCreateUpdate()
    {
        try
        {
            loginUserState = acctManager.Create(partnerCode, namespaceId, mEmail, mNickName, mPassword);
            if (loginUserState == RequestState.Complete)
            {
                Account.CreateResult create_Result = acctManager.Create_Result;
                LogOut();
                switch (create_Result)
                {
                    case Account.CreateResult.UniqueNicknameAlreadyInUse:
                        WhenDone(GripNetwork.Result.InUse);
                        break;
                    default:
                        WhenDone(GripNetwork.Result.Failed);
                        break;
                    case Account.CreateResult.Success:
                        Login(mNickName, mPassword, mLoginCallback);
                        break;
                }
            }
        }
        catch (Exception)
        {
            WhenDone(GripNetwork.Result.Failed);
        }
    }

    private void WhenDone(GripNetwork.Result result)
    {
        Action<GripNetwork.Result> action = mLoginCallback;
        mLoginCallback = null;
        if (result != 0)
        {
        }
        if (action != null)
        {
            action(result);
        }
    }
}
