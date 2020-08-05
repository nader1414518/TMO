using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google;
using Facebook.Unity;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using Proyecto26;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using SimpleFileBrowser;
using Unity.Notifications.Android;
using PubNubAPI;
using ArabicSupport;
using agora_gaming_rtc;
using UnityEngine.Android;

public static class Globals
{
    public static string username;
    public static string email;
    public static string userId;
    public static bool isLoggedIn = false;
    public static bool loggedInWithGoogle = false;
    public static bool loggedInWithFacebook = false;
    public static bool loggedInWithEmail = false;

    public static bool logOutBtnClicked = false;

    public static bool showLoadingPanel = false;

    public static bool friendSuggestionsLoaded = false;
    public static bool friendsLoaded = false;

    public static bool visionAcuityTestSelected = false;
    public static bool astigmatismTestSelected = false;
    public static bool lightSensitivityTestSelected = false;
    public static bool nearVisionTestSelected = false;
    public static bool colorVisionTestSelected = false;
    public static bool visionTestStarted = false;
    public static int visionTestCounter = 0;
    public static int lightSensitivityTestCounter = 0;
    public static int colorVisionTestCounter = 0;
    public static bool showVisionTestResults = false;
    public static string astigmatismTestResult = "";
    public static string nearVisionTestResult = "";

    public static Color userColorNormal = new Color(0.0f, 0.273576f, 0.4716981f, 1.0f);
    public static Color userColorDark = new Color(0.2035867f, 0.2224064f, 0.2358491f, 1.0f);

    public static User currentUser = new User();

    public static List<string> visionTestResults = new List<string>();

    public static Texture2D snapshotPhoto;
    public static string currentSnapshotPath;
    public static List<string> certsPath = new List<string>();
    public static List<Friend> friendSuggestions = new List<Friend>();

    public static string agoraAppId = "706aa24039334ad8bb4649c1be61f311";
}

public class JSONInformation
{
    public string username;
    public string text;
}

public enum UserType
{
    ThisUser,
    OtherUser
}

public class UIMan : MonoBehaviour
{
    public static string webClientId = "710120277560-4mj7olb7o4f618r1op4t1ujovsmfnuf8.apps.googleusercontent.com";
    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;
    public static bool loadDashboard = false;
    bool signedUpSuccessfully = false;

    public Text infoTxt;

    float infoTxtTimer = 0;

    /* Facebook AppSecret: a24e214bd6f60dd79dcc7458587afb7e      Facebook APPID: 888409854989467   */
    // Authentication Area
    #region AuthenticationFunctionality
    #region Auxilaries
    public static void ViewMessageOnScreen(string msg)
    {
        MessageBoxMan.Open(msg);
    }
    public static void ViewTerminatingMessageOnScreen(string msg)
    {
        MessageBoxMan.Open(msg, true);
    }
    public static void CheckInternet()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ViewMessageOnScreen("No Internet Connection ... ");
        }
    }
    #endregion
    #region GoogleAndFacebookSignInFunctionality
    void Start()
    {
        /* Google Initializations */
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        //CheckFirebaseDependencies();        // Don't add this when you are running the game in the editor 
        /* Facebook Initializations */
        if (!(FB.IsInitialized))
        {
            // Initiliaze the facebook sdk
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation event
            FB.ActivateApp();
        }

        Debug.Log(FB.Android.KeyHash);
        // Register notification channel 
        var c = new AndroidNotificationChannel()
        {
            Id = "710120277560-4mj7olb7o4f618r1op4t1ujovsmfnuf8.apps.googleusercontent.com",
            Name = "TMO Main Channel",
            Importance = Importance.High,
            Description = "Generic Notifications"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c);
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation 
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to initialize the Facebook SDK ");
            ViewMessageOnScreen("Failed to initialize the Facebook SDK ");
        }
    }
    private void OnHideUnity(bool isGameShown)
    {
        if (!(isGameShown))
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again 
            Time.timeScale = 1;
        }
    }
    private void SignInWithFacebook()
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }
    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details 
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Prints current access token's User ID
            Debug.Log("Access token: " + aToken.UserId.ToString());
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
                //Debug.Log("Access Token String: " + aToken.TokenString);
            }
            Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(aToken.TokenString);
            FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Facebook Login Canceled ... ");
                    ViewMessageOnScreen("Facebook Login Canceled ... ");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("Facebook Login error: " + task.Exception);
                    ViewMessageOnScreen("Facebook Login error: " + task.Exception);
                    return;
                }

                Debug.Log("Signed In Successfully ... ");
                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.Log("Welcome: " + newUser.DisplayName);
                Debug.Log("Email: " + newUser.Email);
                Globals.username = newUser.DisplayName.ToString();
                Globals.email = newUser.DisplayName.ToString();
                Globals.userId = newUser.UserId.ToString();
                loadDashboard = true;
                Globals.isLoggedIn = true;
                Globals.loggedInWithFacebook = true;
            });
        }
        else
        {
            Debug.Log("User cancelled login ... ");
            ViewMessageOnScreen("User Canceled Login ... ");
        }
    }
    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                }
                else
                {
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
                    ViewMessageOnScreen("Could not resolve all Firebase dependencies: " + task.Result.ToString());
                }
            }
        });
    }
    public void SignInWithGoogle() { OnSignIn(); }
    private void SignOutFromGoogle() { OnSignOut(); }
    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling sign in ... ");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
    private void OnSignOut()
    {
        AddToInformation("Calling SignOut ... ");
        GoogleSignIn.DefaultInstance.SignOut();
    }
    private void OnDisconnect()
    {
        AddToInformation("Calling Disconnect ... ");
        GoogleSignIn.DefaultInstance.Disconnect();
    }
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        loadDashboard = false;
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);
                    ViewMessageOnScreen("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToInformation("Got Unexpected Exception " + task.Exception);
                    ViewMessageOnScreen("Got Unexpected Exception " + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled ... ");
            ViewMessageOnScreen("Canceled ... ");
        }
        else
        {
            AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            AddToInformation("Email: " + task.Result.Email);
            loadDashboard = true;
            Globals.username = task.Result.DisplayName;
            Globals.email = task.Result.Email;
            Globals.userId = task.Result.UserId;
            Globals.isLoggedIn = true;
            Globals.loggedInWithGoogle = true;
            //AddToInformation("Google Id Token: " + task.Result.IdToken);
            //AddToInformation("Google Id Token: " + task.Result.IdToken);
            SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }
    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                {
                    AddToInformation("\nError code: " + inner.ErrorCode + " Message: " + inner.Message);
                    ViewMessageOnScreen("\nError code: " + inner.ErrorCode + " Message: " + inner.Message);
                }
                else
                {
                    loadDashboard = true;
                    Globals.username = task.Result.DisplayName;
                    Globals.email = task.Result.Email;
                    Globals.userId = task.Result.UserId;
                    Globals.isLoggedIn = true;
                    Globals.loggedInWithGoogle = true;
                    AddToInformation("SignIn Successful ... ");
                }
            }
        });
    }
    private void SignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling Sign In Silently ... ");
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }
    private void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;
        AddToInformation("Calling Games Sign IN ");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
    private void AddToInformation(string str) { if (infoTxt) { infoTxt.text = str; infoTxtTimer = 5.0f; } }
    #endregion 

    #region EmailSignInFunctionality
    static bool CheckPassStrength(string password)
    {
        string[] chars = { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "-", "+", "=", "~", "`", "/", "\\", ">", "<" };
        int length = password.Length;
        bool isAlpha = false;
        for (int i = 0; i < chars.Length; i++)
        {
            if (password.Contains(chars[i]))
            {
                isAlpha = true;
                break;
            }
        }
        if (length >= 8 && isAlpha == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    static void GetErrorMessage(AuthError errorCode)
    {
        string msg = "";
        msg = errorCode.ToString();
        Debug.Log(msg);
    }
    bool EmailLogin(string username, string password)
    {
        bool res = false;
        if (username == "" || password == "")
        {
            // Empty fields 
            Debug.Log("Empty Field");
            ViewMessageOnScreen("Empty Field");
            res = false;
        }
        else
        {
            FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(username, password).ContinueWith((task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    ViewMessageOnScreen(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    ViewMessageOnScreen(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsCompleted)
                {
                    Globals.username = username;
                    Globals.email = username;
                    Globals.userId = task.Result.UserId;
                    Globals.isLoggedIn = true;
                    Globals.loggedInWithEmail = true;
                    loadDashboard = true;
                    Debug.Log("Welcome back " + Globals.email);
                    res = true;
                }
            }));
        }
        return res;
    }
    bool RegisterUser(string email, string password)
    {
        bool res = false;
        if (password == "" || email == "")
        {
            // Empty field 
            res = false;
            Debug.Log("Some fields are empty ... ");
        }
        else if (!(CheckPassStrength(password)))
        {
            // Weak pass
            res = false;
            Debug.Log("Weak password ... ");
            ViewMessageOnScreen("Weak password ... ");

        }
        else
        {
            FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith((task =>
            {
                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    ViewMessageOnScreen(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                    GetErrorMessage((AuthError)e.ErrorCode);
                    ViewMessageOnScreen(((AuthError)e.ErrorCode).ToString());
                    res = false;
                    return;
                }
                if (task.IsCompleted)
                {
                    Debug.Log("Signed Up Successfully ... ");
                    signedUpSuccessfully = true;
                    ViewMessageOnScreen("Signed Up Successfully ... ");
                    //HideAllPanels();
                    //HelpersMan.ShowPanel(loginPanel);
                    res = true;
                }
            }));
        }
        return res;
    }
    #endregion

    public static void LogOut()
    {
        if (Globals.loggedInWithEmail)
        {
            Globals.isLoggedIn = false;
            Globals.loggedInWithEmail = false;
            Globals.username = "";
            Globals.email = "";
            Globals.userId = "";
            loadDashboard = false;
            FirebaseAuth.DefaultInstance.SignOut();
        }
        else if (Globals.loggedInWithGoogle)
        {
            Globals.isLoggedIn = false;
            Globals.loggedInWithGoogle = false;
            Globals.username = "";
            Globals.email = "";
            Globals.userId = "";
            loadDashboard = false;
            GoogleSignIn.DefaultInstance.SignOut();
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("Signed out from google ... ");
        }
        else if (Globals.loggedInWithFacebook)
        {
            Globals.isLoggedIn = false;
            Globals.loggedInWithFacebook = false;
            Globals.username = "";
            Globals.email = "";
            Globals.userId = "";
            loadDashboard = false;

            Debug.Log("Signed out from facebook ... ");
            FirebaseAuth.DefaultInstance.SignOut();
        }
    }
    #endregion

    #region UIVariables
    // Test Vars
    bool redirectToProfilePanel = false;
    public RawImage testImg;
    // Canvas reference 
    public Canvas canvas;
    // Social Platform UI variables
    public GameObject friendSlotPrefab;
    public GameObject friendSuggestionSlotPrefab;
    public List<GameObject> friendSuggestionsList;
    public List<GameObject> friendsList;
     // File browser object 
    public GameObject fileBrowserPrefab;
    // Doctor Certificates 
    public List<CertBtn> certBtns = new List<CertBtn>();
    // Webcam texture 
    WebCamTexture deviceCamera;
    // Main Panels
    public GameObject loadingPanel;
    public GameObject signUpPanel;
    public GameObject loginPanel;
    public GameObject dashboardPanel;
    public GameObject notificationsPanel;
    public GameObject alarmsPanel;
    public GameObject profilePanel;
    public GameObject visionTestPanel;
    public GameObject startExaminationPanel;
    public GameObject contactADoctorPanel;
    public GameObject communityPanel;
    public GameObject cameraStreamPanel;
    // Vision Panels
    public GameObject visionAcuityTestPanel;
    public GameObject astigmatismTestPanel;
    public GameObject lightSensitivityTestPanel;
    public GameObject nearVisionTestPanel;
    public GameObject colorVisionTestPanel;
    // Inner Items
    public GameObject notificationsContainerPanel;
    public Image visionTestSymbol;
    public Image lightSensitivitySymbol;
    public Image colorVisionSymbol;
    public InputField colorVisionTestNumberIF;
    public GameObject startVisionTestPanel;
    public GameObject cameraShotPreviewPanel;
    public GameObject cameraShotPreviewImage;
    public Button startTestBtn;
    public Text startTestPanelText;
    public Text startTestPanelInstructionsText;
    public Button astigmatismYesBtn;
    public Button astigmatismNoBtn;
    public GameObject contactUsPanel;
    public GameObject friendSuggestionsPanel;
    public GameObject friendsPanel;
    public GameObject friendSuggestionsPanelContainer;
    public GameObject friendsPanelContainer;
    // Color Vision Icons 
    public List<ColorVisionSymbol> colorVisionSymbols = new List<ColorVisionSymbol>();
    // Header Btns Containers 
    public GameObject visionTestBtnContainer;
    public GameObject startExaminationBtnContainer;
    public GameObject profileBtnContainer;
    public GameObject contactUsBtnContainer;
    public GameObject contactADoctorBtnContainer;
    public GameObject communityBtnContainer;
    public GameObject notificationsBtnContainer;
    public GameObject alarmsBtnContainer;
    // Vision Tests Btns Containers
    public GameObject visionAcuityTestBtnContainer;
    public GameObject astigmatismTestBtnContainer;
    public GameObject lightSensitvityTestBtnContainer;
    public GameObject nearVisionTestBtnContainer;
    public GameObject colorVisionTestBtnContainer;
    // User Login Variables
    public InputField loginEmailIF;
    public InputField loginPasswordIF;
    public InputField signupEmailIF;
    public InputField signupPasswordIF;
    public InputField signupPasswordConfirmIF;
    // Different Users Area
    public GameObject patientInfoPanel;
    public GameObject doctorInfoPanel;
    // Certifications Buttons 
    public List<GameObject> addCertBtns = new List<GameObject>();
    // User Info Area
    public Text profileUsernameTxt;
    public Text profileEmailTxt;
    public Toggle darkModeToggle;
    public InputField phoneNumberIF;
    public InputField addressIF;
    public Dropdown userTypeDD;
    // User History Area
    public InputField weightIF;
    public InputField previousDiseasesIF;
    public InputField currentDiseasesIF;
    public InputField currentDrugsIF;
    public InputField previousOperationsIF;
    public Toggle wearingGlassesToggle;
    // Family History Area
    public InputField familyMembersChronicDiseasesIF;
    // Social History Area
    public Toggle smokingToggle;
    public Toggle alcoholToggle;
    // Prefabs 
    GameObject informationNotificationPrefab;

    #endregion

    #region UIAuxilaries
    IEnumerator ShowLoadItemImageCoroutine(CertBtn btn)
    {
        /* FileBrowser initializations*/
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png", ".jpeg"));     // Set the show all files flag to true and show items of type (.png, .jpg)
        FileBrowser.SetDefaultFilter(".png");           // Default files to be displayed are of type (.png)
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".exe");
        yield return FileBrowser.WaitForLoadDialog(false, false, "Load Image", "Load");
        if (FileBrowser.Success)
        {
            Debug.Log("Image: " + FileBrowser.Result[0]);
            btn.certPath = FileBrowser.Result[0];
            btn.tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
            byte[] content = File.ReadAllBytes(btn.certPath);
            btn.tex.LoadImage(content);
            if (testImg)
            {
                testImg.texture = btn.tex;
            }
            btn.btn.GetComponentInChildren<Text>().text = "Added";
            //btn.btn.GetComponent<Button>().interactable = false;
            Globals.currentUser.userCerts[btn.btnId - 1] = "users/" + Globals.userId + "/Certs/Cert" + btn.btnId;
            Globals.showLoadingPanel = true;
            Firebase.Storage.FirebaseStorage.DefaultInstance.RootReference.Child("users").Child(Globals.userId).Child("Certs").Child("Cert" + btn.btnId).PutBytesAsync(content).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Globals.showLoadingPanel = false;
                    btn.certPath = Globals.currentUser.userCerts[btn.btnId - 1];
                    MessageBoxMan.Open("Uploaded Certification ... ");
                }
            });
            Debug.Log("Loaded Image content ... ");
        }
    }
    void HideAllPanels()
    {
        HelpersMan.HidePanel(signUpPanel);
        HelpersMan.HidePanel(loginPanel);
        HelpersMan.HidePanel(dashboardPanel);
        HelpersMan.HidePanel(notificationsPanel);
        HelpersMan.HidePanel(profilePanel);
        HelpersMan.HidePanel(visionTestPanel);
        HelpersMan.HidePanel(startExaminationPanel);
        HelpersMan.HidePanel(contactUsPanel);
        HelpersMan.HidePanel(contactADoctorPanel);
        HelpersMan.HidePanel(communityPanel);
        HelpersMan.HidePanel(alarmsPanel);
    }
    void HideAllVisionTestPanels()
    {
        HelpersMan.HidePanel(startVisionTestPanel);
        HelpersMan.HidePanel(visionAcuityTestPanel);
        HelpersMan.HidePanel(astigmatismTestPanel);
        HelpersMan.HidePanel(lightSensitivityTestPanel);
        HelpersMan.HidePanel(nearVisionTestPanel);
        HelpersMan.HidePanel(colorVisionTestPanel);
    }
    void DeHighlightAllPanels()
    {
        HelpersMan.DeHighlightPanel(visionTestBtnContainer);
        HelpersMan.DeHighlightPanel(startExaminationBtnContainer);
        HelpersMan.DeHighlightPanel(profileBtnContainer);
        HelpersMan.DeHighlightPanel(notificationsBtnContainer);
        HelpersMan.DeHighlightPanel(contactADoctorBtnContainer);
        HelpersMan.DeHighlightPanel(contactUsBtnContainer);
        HelpersMan.DeHighlightPanel(communityBtnContainer);
        HelpersMan.DeHighlightPanel(alarmsBtnContainer);
    }
    void DeHighlightAllVisionTestPanels()
    {
        HelpersMan.DeHighlightPanel(visionAcuityTestBtnContainer);
        HelpersMan.DeHighlightPanel(astigmatismTestBtnContainer);
        HelpersMan.DeHighlightPanel(lightSensitvityTestBtnContainer);
        HelpersMan.DeHighlightPanel(nearVisionTestBtnContainer);
        HelpersMan.DeHighlightPanel(colorVisionTestBtnContainer);
    }
    void CertBtnInitializationCallback()
    {
        foreach (CertBtn btn in certBtns)
        {
            btn.tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
            btn.btn.gameObject.GetComponent<Button>().onClick.AddListener(delegate {
                StartCoroutine(ShowLoadItemImageCoroutine(btn));
            });
        }
    }
    void DisplayInfoNotification(string info)
    {
        if (notificationsContainerPanel && informationNotificationPrefab)
        {
            GameObject notificationSlot = Instantiate(informationNotificationPrefab, notificationsContainerPanel.transform);
            notificationSlot.GetComponentInChildren<Text>().text = info;
            notificationSlot.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate {
                // Destroy this notification slot
                Destroy(notificationSlot.gameObject);
            });
        }
    }
    //void CertBtnInitializer(int id)
    //{
    //    if (canvas && fileBrowserPrefab)
    //    {
    //        //GameObject fileBrowserObject = Instantiate(fileBrowserPrefab, canvas.gameObject.transform);
    //        //fileBrowserObject.transform.localScale = canvas.gameObject.transform.localScale;
    //        //fileBrowserObject.name = "Load a certificate";
    //        //string[] fileExtensions = { "png", "jpeg", "jpg", "image" };
    //        //FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
    //        //fileBrowserScript.SetupFileBrowser(ViewMode.Portrait);
    //        //fileBrowserScript.OpenFilePanel(fileExtensions);

    //        //if (id == 1)
    //        //{
    //        //    fileBrowserScript.OnFileSelect += LoadImageUsingPathBtn1;
    //        //}
    //        //else if (id == 2)
    //        //{
    //        //    fileBrowserScript.OnFileSelect += LoadImageUsingPathBtn2;
    //        //}
    //        //else if (id == 3)
    //        //{
    //        //    fileBrowserScript.OnFileSelect += LoadImageUsingPathBtn3;
    //        //}
    //    }
    //}
    //void LoadImageUsingPathBtn1(string path)
    //{
    //    if (!(string.IsNullOrEmpty(path)))
    //    {
    //        certBtns[0].certPath = path;
    //        byte[] imgContent = File.ReadAllBytes(path);
    //        if (certBtns[0].tex)
    //        {
    //            certBtns[0].tex.LoadImage(imgContent);
    //        }
    //    }
    //}
    //void LoadImageUsingPathBtn2(string path)
    //{
    //    if (!(string.IsNullOrEmpty(path)))
    //    {
    //        certBtns[1].certPath = path;
    //        byte[] imgContent = File.ReadAllBytes(path);
    //        if (certBtns[1].tex)
    //        {
    //            certBtns[1].tex.LoadImage(imgContent);
    //        }
    //    }
    //}
    //void LoadImageUsingPathBtn3(string path)
    //{
    //    if (!(string.IsNullOrEmpty(path)))
    //    {
    //        certBtns[2].certPath = path;
    //        byte[] imgContent = File.ReadAllBytes(path);
    //        if (certBtns[2].tex)
    //        {
    //            certBtns[2].tex.LoadImage(imgContent);
    //        }
    //    }
    //}
    #endregion

    #region MainFunctions
    void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (loginPasswordIF && signupPasswordIF && signupPasswordConfirmIF)
            {
                loginPasswordIF.contentType = InputField.ContentType.Password;
                signupPasswordIF.contentType = InputField.ContentType.Password;
                signupPasswordConfirmIF.contentType = InputField.ContentType.Password;
            }
            HideAllPanels();
            HelpersMan.ShowPanel(loginPanel);
        }
        else if (SceneManager.GetActiveScene().name == "Dashboard")
        {
            // Load Prefabs 
            informationNotificationPrefab = Resources.Load<GameObject>("InformationNotificationSlot");
            chatPanelPrefab = Resources.Load<GameObject>("ChatPanel");
            thisUserMessageSlotPrefab = Resources.Load<GameObject>("ThisUserMessageSlot");
            otherUserMessageSlotPrefab = Resources.Load<GameObject>("OtherUserMessageSlot");
            alarmSlot = Resources.Load<GameObject>("AlarmSlot");
            addAlarmPanelPrefab = Resources.Load<GameObject>("AddAlarmPanel");
            voiceChatPanelPrefab = Resources.Load<GameObject>("VoiceChatPanel");

            // Assign Cert Btn objects 
            for (int i = 0; i < certBtns.Count; i++)
            {
                int temp = i + 1;
                certBtns[i].btn = GameObject.FindGameObjectWithTag("AddCertBtn" + temp);
            }
            CertBtnInitializationCallback();
            // Load Dashboard and Vision Test Panel
            HideAllPanels();
            HelpersMan.ShowPanel(dashboardPanel);
            OpenVisionTestPanelBtnCallback();
            DeHighlightAllPanels();
            HelpersMan.HighlightPanel(visionTestBtnContainer);
            Globals.currentUser.userId = Globals.userId;
            // Load User Settings
            #region LoadUserInfo
            Debug.Log("User ID: " + Globals.userId);
            Globals.currentUser.friends.Clear();
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(Globals.userId).Child("profile").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    IDictionary dictUser = (IDictionary)snapshot.Value;
                    // Assign Values of the current user
                    Globals.currentUser.userColorMode = dictUser["userColorMode"].ToString();
                    Globals.currentUser.isFirstTime = dictUser["isFirstTime"].ToString();
                    Globals.currentUser.userPhoneNumber = dictUser["userPhoneNumber"].ToString();
                    //Debug.Log("Phone Number: " + Globals.currentUser.userPhoneNumber);
                    Globals.currentUser.userAddress = dictUser["userAddress"].ToString();
                    Globals.currentUser.userWeight = dictUser["userWeight"].ToString();
                    Globals.currentUser.userPreviousDiseases = dictUser["userPreviousDiseases"].ToString();
                    Globals.currentUser.userCurrentDiseases = dictUser["userCurrentDiseases"].ToString();
                    Globals.currentUser.userCurrentDrugs = dictUser["userCurrentDrugs"].ToString();
                    Globals.currentUser.userPreviousOperations = dictUser["userPreviousOperations"].ToString();
                    Globals.currentUser.userWearingGlasses = dictUser["userWearingGlasses"].ToString();
                    Globals.currentUser.userFamilyChronicDiseases = dictUser["userFamilyChronicDiseases"].ToString();
                    Globals.currentUser.userSmoking = dictUser["userSmoking"].ToString();
                    Globals.currentUser.userDrinksAlcohols = dictUser["userDrinksAlcohols"].ToString();
                    Globals.currentUser.userType = dictUser["userType"].ToString();
                    Globals.currentUser.bio = dictUser["bio"].ToString();
                    Globals.currentUser.userChatId = "";
                    foreach (DataSnapshot snap in snapshot.Child("userCerts").Children)
                    {
                        Globals.currentUser.userCerts[int.Parse(snap.Key)] = snap.Value.ToString();
                    }
                    //Debug.Log("User Drinks Alcohol: " + Globals.currentUser.userDrinksAlcohols);
                    Globals.showLoadingPanel = true;
                    Globals.currentUser.friends.Clear();
                    FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(Globals.userId).Child("friends").GetValueAsync().ContinueWith(task1 =>
                    {
                        if (task1.IsCompleted)
                        {
                            try
                            {
                                DataSnapshot res = task1.Result;
                                foreach (DataSnapshot shot in res.Children)
                                {
                                    Debug.Log("You have friend: " + shot.Key);
                                    IDictionary dictFriends = (IDictionary)shot.Value;
                                    Globals.currentUser.friends.Add(new Friend(
                                        dictFriends["username"].ToString(),
                                        dictFriends["email"].ToString(),
                                        dictFriends["userId"].ToString(),
                                        dictFriends["bio"].ToString(),
                                        dictFriends["type"].ToString()
                                    ));
                                    Globals.currentUser.friends[Globals.currentUser.friends.Count - 1].ratings.Clear();
                                    if (shot.Child("type").Value.ToString() == "doctor")
                                    {
                                        FirebaseDatabase.DefaultInstance.RootReference
                                        .Child("users")
                                        .Child(dictFriends["userId"].ToString())
                                        .Child("ratings").GetValueAsync()
                                        .ContinueWith(result =>
                                        {
                                            if (result.IsCompleted)
                                            {
                                                DataSnapshot ratings = result.Result;
                                                foreach (DataSnapshot rate in ratings.Children)
                                                {
                                                    DoctorRating r = new DoctorRating();
                                                    r.review = rate.Child("review").Value.ToString();
                                                    r.stars = int.Parse(rate.Child("stars").Value.ToString());
                                                    Globals.currentUser.friends[Globals.currentUser.friends.Count - 1].ratings.Add(r);
                                                }
                                            }
                                            Globals.showLoadingPanel = false;
                                        });
                                    }
                                    Debug.Log("Added friend: " + Globals.currentUser.friends[Globals.currentUser.friends.Count - 1].username);
                                }
                            }
                            catch
                            {
                                MessageBoxMan.Open("Couldn't load friends at the moment ... ");
                                Globals.showLoadingPanel = false;
                            }
                        }
                        Globals.showLoadingPanel = false;
                    });
                }
                Globals.currentUser.username = Globals.username;
                Globals.currentUser.email = Globals.username;
                Globals.currentUser.userId = Globals.userId;
            });

            #endregion

            // Initialize PubNub 
            PNConfiguration pnConfiguration = new PNConfiguration();
            pnConfiguration.PublishKey = "pub-c-4d5f4bb5-21a4-43f3-890f-5fd5d7500044";
            pnConfiguration.SubscribeKey = "sub-c-896f1e02-c1d9-11ea-8089-3ec3506d555b";
            pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
            pnConfiguration.UUID = Globals.userId;
            pubnub = new PubNub(pnConfiguration);

            // Initialize Agora Engine For Voice Chat Functionality
            LoadAgoraEngine();
            // Checking Persmissions 
            if (!(Permission.HasUserAuthorizedPermission(Permission.Microphone)))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
        }
    }
    void Update()
    {
        CheckInternet();
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (loadDashboard && Globals.isLoggedIn)
            {
                loadDashboard = false;
                SceneManager.LoadScene("Dashboard", LoadSceneMode.Single);
            }
            if (signedUpSuccessfully)
            {
                signedUpSuccessfully = false;
                HelpersMan.HidePanel(signUpPanel);
                HelpersMan.ShowPanel(loginPanel);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Dashboard")
        {
            if (!(loadDashboard) && !(Globals.isLoggedIn))
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
            if (Globals.logOutBtnClicked)
            {
                // Free all resources
                Globals.currentUser.friends.Clear();
                Globals.friendSuggestions.Clear();
                for (int i = 0; i < friendsList.Count; i++)
                {
                    Destroy(friendsList[i].gameObject);
                    friendsList.Remove(friendsList[i]);
                }
                for (int i = 0; i < friendSuggestionsList.Count; i++)
                {
                    Destroy(friendSuggestionsList[i].gameObject);
                    friendSuggestionsList.Remove(friendSuggestionsList[i]);
                }
                // Clear inputFields
                ClearAllPersonalInfo();
                UnloadAgoraEngine();
                Globals.logOutBtnClicked = false;
                LogOut();
            }
            if (messageReceived)
            {
                if (Time.frameCount % 30 == 0)
                {
                    refreshTimes++;
                    if (chatPanel)
                    {
                        if (refreshTimes < 3)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponentsInChildren<Image>()[2].gameObject.GetComponent<RectTransform>());
                        }
                        else
                        {
                            refreshTimes = 0;
                            messageReceived = false;
                        }
                    }
                }
            }
            //if (messageReceived)
            //{
            //    Globals.showLoadingPanel = true;
            //    if (Time.frameCount%100 == 0)
            //    {
            //        refreshTimes++;
            //        if (chatPanel)
            //        {
            //            chatPanel.SetActive(false);
            //            if (refreshTimes >= 2)
            //            {
            //                chatPanel.SetActive(true);
            //                refreshTimes = 0;
            //                messageReceived = false;
            //                Globals.showLoadingPanel = false;
            //            }
            //        }
            //    }
            //}
            //if (messageReceived)
            //{
            //    if (Time.frameCount % 100 == 0)
            //    {
            //        refreshTimes++;
            //        if (refreshTimes >= 10)
            //        {
            //            chatPanel.SetActive(false);
            //            chatPanel.SetActive(true);
            //        }
            //        else
            //        {
            //            chatPanel.SetActive(true);
            //            messageReceived = false;
            //            refreshTimes = 0;
            //        }
            //    }
            //}
            if (Globals.showLoadingPanel)
            {
                HelpersMan.ShowPanel(loadingPanel);
            }
            else
            {
                HelpersMan.HidePanel(loadingPanel);
            }
            if (FriendSlot.openChatPanel)
            {
                LoadChatPanelUI(FriendSlot.openedChatOtherUserId);
                messageReceived = true;
                FriendSlot.openChatPanel = false;
            }
            if (openVoiceChatPanel)
            {
                VoiceCall(Globals.currentUser.userChatId);
                openVoiceChatPanel = false;
            }
            if (redirectToProfilePanel)
            {
                OpenProfilePanelBtnCallback();
                redirectToProfilePanel = false;
            }
            if (Globals.friendSuggestionsLoaded)
            {
                LoadFriendSuggestionsUI();
                Globals.friendSuggestionsLoaded = false;
            }
            if (Globals.friendsLoaded)
            {
                // Load Friends List
                LoadFriendsListUI();
                Globals.friendsLoaded = false;
            }
            // Vision Tests Flags 
            if (Globals.showVisionTestResults)
            {
                if (Globals.visionAcuityTestSelected)
                {
                    int rightCount = 0;
                    int wrongCount = 0;
                    for (int i = 0; i < Globals.visionTestResults.Count; i++)
                    {
                        if (Globals.visionTestResults[i] == "pass")
                        {
                            rightCount++;
                        }
                        else if (Globals.visionTestResults[i] == "fail")
                        {
                            wrongCount++;
                        }
                    }
                    //MessageBoxMan.Open("You got " + rightCount + " right out of " + Globals.visionTestResults.Count + " times ... ");
                    if (rightCount >= 6)
                    {
                        MessageBoxMan.Open("You do not suffer from Vision Acuity issues ... ");
                    }
                    else
                    {
                        MessageBoxMan.Open("You may be suffering from Vision Acuity Issues.\n you should see a doctor about this ... ");
                    }
                    OpenVisionTestPanelBtnCallback();
                    HelpersMan.ResetSymbolSizeAndRotation(visionTestSymbol);
                    Globals.visionTestCounter = 0;
                    Globals.showVisionTestResults = false;
                    Globals.visionTestResults.Clear();
                    Globals.visionAcuityTestSelected = false;
                    DisplayInfoNotification("You got " + rightCount + " right out of " + Globals.visionTestResults.Count + " in vision acuity test");
                }
                else if (Globals.lightSensitivityTestSelected)
                {
                    int rightCount = 0;
                    int wrongCount = 0;
                    for (int i = 0; i < Globals.visionTestResults.Count; i++)
                    {
                        if (Globals.visionTestResults[i] == "pass")
                        {
                            rightCount++;
                        }
                        else if (Globals.visionTestResults[i] == "fail")
                        {
                            wrongCount++;
                        }
                    }
                    //MessageBoxMan.Open("You got " + rightCount + " right out of " + Globals.visionTestResults.Count + " times ... ");
                    if (rightCount >= 13)
                    {
                        MessageBoxMan.Open("You do not suffer from Light Sensitivity issues ... ");
                        DisplayInfoNotification("You do not suffer from Light Sensitivity issues ... ");
                    }
                    else
                    {
                        MessageBoxMan.Open("You may be suffering from Light Sensitivity Issues.\n you should see a doctor about this ... ");
                        DisplayInfoNotification("You may be suffering from Light Sensitivity Issues.\n you should see a doctor about this ... ");
                    }
                    OpenLightSensitivityTestPanelBtnCallback();
                    OpenVisionTestPanelBtnCallback();
                    OpenLightSensitivityTestPanelBtnCallback();
                    HelpersMan.ResetSymbolSizeAndRotation(lightSensitivitySymbol);
                    HelpersMan.ResetSymbolOpacity(lightSensitivitySymbol);
                    Globals.lightSensitivityTestCounter = 0;
                    Globals.showVisionTestResults = false;
                    Globals.visionTestResults.Clear();
                    Globals.lightSensitivityTestSelected = false;
                }
                else if (Globals.astigmatismTestSelected)
                {
                    if (Globals.astigmatismTestResult == "pass")
                    {
                        MessageBoxMan.Open("You do not suffer from Astigmatism issues ... ");
                        DisplayInfoNotification("You do not suffer from Astigmatism issues ... ");
                    }
                    else if (Globals.astigmatismTestResult == "fail")
                    {
                        MessageBoxMan.Open("You may be suffering from Astigmatism Issues.\n you should see a doctor about this ... ");
                        DisplayInfoNotification("You may be suffering from Astigmatism Issues.\n you should see a doctor about this ... ");
                    }
                    Globals.astigmatismTestResult = "";
                    Globals.astigmatismTestSelected = false;
                    Globals.showVisionTestResults = false;
                    OpenAstigmatismTestPanelBtnCallback();
                }
                else if (Globals.nearVisionTestSelected)
                {
                    if (Globals.nearVisionTestResult == "pass")
                    {
                        MessageBoxMan.Open("You are correct ... you do not suffer from near vision problems");
                        DisplayInfoNotification("You are correct ... you do not suffer from near vision problems");
                        Debug.Log("Right");
                    }
                    else if (Globals.nearVisionTestResult == "fail")
                    {
                        MessageBoxMan.Open("You may be suffering from near vision problems ... ");
                        DisplayInfoNotification("You may be suffering from near vision problems ... ");
                        Debug.Log("Wrong");
                    }
                    Globals.nearVisionTestResult = "";
                    Globals.nearVisionTestSelected = false;
                    Globals.showVisionTestResults = false;
                    OpenNearVisionTestPanelBtnCallback();
                }
                else if (Globals.colorVisionTestSelected)
                {
                    int rightCount = 0;
                    int wrongCount = 0;
                    for (int i = 0; i < Globals.visionTestResults.Count; i++)
                    {
                        if (Globals.visionTestResults[i] == "pass")
                        {
                            rightCount++;
                        }
                        else if (Globals.visionTestResults[i] == "fail")
                        {
                            wrongCount++;
                        }
                    }
                    //MessageBoxMan.Open("You got " + rightCount + " right out of " + Globals.colorVisionTestCounter);
                    if (rightCount >= 2)
                    {
                        MessageBoxMan.Open("You do not suffer from Color Vision Issues ... ");
                        DisplayInfoNotification("You do not suffer from Color Vision Issues ... ");
                    }
                    else
                    {
                        MessageBoxMan.Open("You may be suffering from Color Vision Issues.\n You should check a doctor for that ... ");
                        DisplayInfoNotification("You may be suffering from Color Vision Issues.\n You should check a doctor for that ... ");
                    }
                    Globals.colorVisionTestCounter = 0;
                    Globals.colorVisionTestSelected = false;
                    Globals.showVisionTestResults = false;
                    Globals.visionTestResults.Clear();
                    if (colorVisionTestNumberIF)
                    {
                        colorVisionTestNumberIF.text = "";
                    }
                    OpenColorVisionTestPanelBtnCallback();
                }
            }
        }
    }
    #endregion

    #region Callbacks
    public void LoginBtnCallback()
    {
        if (loginEmailIF && loginPasswordIF)
        {
            if (HelpersMan.CheckEmpty(loginEmailIF) || HelpersMan.CheckEmpty(loginPasswordIF))
            {
                ViewMessageOnScreen("Some Fields are empty ... ");
            }
            else
            {
                loginPasswordIF.contentType = InputField.ContentType.Standard;
                string password = loginPasswordIF.text;
                loginPasswordIF.contentType = InputField.ContentType.Password;
                EmailLogin(loginEmailIF.text.ToString(), password);
            }
        }
    }
    public void LoginWithFaceboolBtnCallback()
    {
        SignInWithFacebook();
    }
    public void SignUpBtnCallback()
    {
        if (signupEmailIF && signupPasswordIF && signupPasswordConfirmIF)
        {
            if (HelpersMan.CheckEmpty(signupEmailIF) || HelpersMan.CheckEmpty(signupPasswordIF) || HelpersMan.CheckEmpty(signupPasswordConfirmIF))
            {
                ViewMessageOnScreen("Some Fields are empty ... ");
            }
            else
            {
                signupPasswordIF.contentType = InputField.ContentType.Standard;
                string password = signupPasswordIF.text;
                signupPasswordIF.contentType = InputField.ContentType.Password;
                signupPasswordConfirmIF.contentType = InputField.ContentType.Standard;
                string passwordConfirm = signupPasswordConfirmIF.text;
                signupPasswordConfirmIF.contentType = InputField.ContentType.Password;
                if (password != passwordConfirm)
                {
                    ViewMessageOnScreen("Passwords don't match ... ");
                }
                else
                {
                    bool loginResult = RegisterUser(signupEmailIF.text.ToString(), password);
                }
            }
        }
    }
    public void CreateAccountBtnCallback()
    {
        HideAllPanels();
        HelpersMan.ShowPanel(signUpPanel);
    }
    public void BackToLoginPanelBtnCallback()
    {
        HideAllPanels();
        HelpersMan.ShowPanel(loginPanel);
    }
    public void ExitAppBtnCallback()
    {
        Application.Quit();
    }
    public void LogOutBtnCallback()
    {
        // Save Info
        Globals.currentUser.email = Globals.email;
        Globals.currentUser.username = Globals.username;
        if (darkModeToggle)
        {
            if (darkModeToggle.isOn)
            {
                Globals.currentUser.userColorMode = "Dark";
            }
            else
            {
                Globals.currentUser.userColorMode = "Normal";
            }
        }
        if (phoneNumberIF)
        {
            Globals.currentUser.userPhoneNumber = phoneNumberIF.text.ToString();
        }
        if (addressIF)
        {
            Globals.currentUser.userAddress = addressIF.text.ToString();
        }
        if (weightIF)
        {
            Globals.currentUser.userWeight = weightIF.text.ToString();
        }
        if (previousDiseasesIF)
        {
            Globals.currentUser.userPreviousDiseases = previousDiseasesIF.text.ToString();
        }
        if (currentDiseasesIF)
        {
            Globals.currentUser.userCurrentDiseases = currentDiseasesIF.text.ToString();
        }
        if (currentDrugsIF)
        {
            Globals.currentUser.userCurrentDrugs = currentDrugsIF.text.ToString();
        }
        if (previousOperationsIF)
        {
            Globals.currentUser.userPreviousOperations = previousOperationsIF.text.ToString();
        }
        if (wearingGlassesToggle)
        {
            if (wearingGlassesToggle.isOn)
            {
                Globals.currentUser.userWearingGlasses = "yes";
            }
            else
            {
                Globals.currentUser.userWearingGlasses = "no";
            }
        }
        if (familyMembersChronicDiseasesIF)
        {
            Globals.currentUser.userFamilyChronicDiseases = familyMembersChronicDiseasesIF.text.ToString();
        }
        if (smokingToggle)
        {
            if (smokingToggle.isOn)
            {
                Globals.currentUser.userSmoking = "yes";
            }
            else
            {
                Globals.currentUser.userSmoking = "no";
            }
        }
        if (alcoholToggle)
        {
            if (alcoholToggle.isOn)
            {
                Globals.currentUser.userDrinksAlcohols = "yes";
            }
            else
            {
                Globals.currentUser.userDrinksAlcohols = "no";
            }
        }
        if (userTypeDD)
        {
            if (userTypeDD.value == 0)
            {
                Globals.currentUser.userType = "patient";
            }
            else if (userTypeDD.value == 1)
            {
                Globals.currentUser.userType = "doctor";
            }
        }
        Globals.showLoadingPanel = true;
        string json = JsonUtility.ToJson(Globals.currentUser);
        //Debug.Log("User Current Color Mode: " + Globals.currentUser.userColorMode);
        FirebaseDatabase.DefaultInstance.RootReference.Child("users")
            .Child(Globals.userId).Child("profile").SetRawJsonValueAsync(json)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {

                    ViewMessageOnScreen("Changes Saved to your account .. Please Login Again");
                    Globals.logOutBtnClicked = true;
                    //LogOut();
                }
                else
                {
                    ViewMessageOnScreen("Failed To Save Changes ... ");
                }
                Globals.showLoadingPanel = false;
            });
    }
    public void OpenVisionTestPanelBtnCallback()
    {
        Debug.Log("Vision Panel is Active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(visionTestPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(visionTestBtnContainer);
        if (visionTestSymbol)
        {
            Debug.Log("Symbol Z Rotation Value: " + visionTestSymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z);
            //HelpersMan.ShowPanel(startVisionTestPanel);
        }
        OpenVisionAcuityTestPanelBtnCallback();
    }
    public void OpenStartExaminationPanelBtnCallback()
    {
        Debug.Log("Examination Panel is Active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(startExaminationPanel);
        HelpersMan.HidePanel(cameraShotPreviewPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(startExaminationBtnContainer);
        if (cameraStreamPanel)
        {
            deviceCamera = new WebCamTexture();
            //deviceCamera.width = Screen.currentResolution.width;
            //deviceCamera.height = (int)(Screen.currentResolution.width + 0.5f * Screen.currentResolution.width);
            cameraStreamPanel.GetComponent<RawImage>().texture = deviceCamera;
            deviceCamera.Play();
        }
    }
    public void OpenProfilePanelBtnCallback()
    {
        Debug.Log("Profile Panel is Active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(profilePanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(profileBtnContainer);
        // Assign the info in the profile panel
        if (profileUsernameTxt)
        {
            profileUsernameTxt.text = Globals.username;
        }
        if (profileEmailTxt)
        {
            profileEmailTxt.text = Globals.email;
        }
        // Assign App Colors 
        if (Globals.currentUser.userColorMode == "Dark")
        {
            if (dashboardPanel)
            {
                dashboardPanel.GetComponent<Image>().color = Globals.userColorDark;
                if (darkModeToggle)
                {
                    darkModeToggle.isOn = true;
                }
            }
        }
        else
        {
            if (dashboardPanel)
            {
                dashboardPanel.GetComponent<Image>().color = Globals.userColorNormal;
                if (darkModeToggle)
                {
                    darkModeToggle.isOn = false;
                }
            }
        }
        // Assign Values of the profile panel fields 
        if (phoneNumberIF)
        {
            //if (string.IsNullOrEmpty(phoneNumberIF.text))
            //{
            //    phoneNumberIF.text = "";
            //}
            //else
            //{
            //    phoneNumberIF.text = Globals.currentUser.userPhoneNumber;
            //}
            phoneNumberIF.text = Globals.currentUser.userPhoneNumber;
        }
        if (addressIF)
        {
            //if (string.IsNullOrEmpty(addressIF.text))
            //{
            //    addressIF.text = "";
            //}
            //else
            //{
            //    addressIF.text = Globals.currentUser.userAddress;
            //}
            addressIF.text = Globals.currentUser.userAddress;
        }
        if (weightIF)
        {
            //if (string.IsNullOrEmpty(weightIF.text))
            //{
            //    weightIF.text = "";
            //}
            //else
            //{
            //    weightIF.text = Globals.currentUser.userWeight;
            //}
            weightIF.text = Globals.currentUser.userWeight;
        }
        if (previousDiseasesIF)
        {
            //if (string.IsNullOrEmpty(previousDiseasesIF.text))
            //{
            //    previousDiseasesIF.text = "";
            //}
            //else
            //{
            //    previousDiseasesIF.text = Globals.currentUser.userPreviousDiseases;
            //}
            previousDiseasesIF.text = Globals.currentUser.userPreviousDiseases;
        }
        if (currentDiseasesIF)
        {
            //if (string.IsNullOrEmpty(currentDiseasesIF.text))
            //{
            //    currentDiseasesIF.text = "";
            //}
            //else
            //{
            //    currentDiseasesIF.text = Globals.currentUser.userCurrentDiseases;
            //}
            currentDiseasesIF.text = Globals.currentUser.userCurrentDiseases;
        }
        if (currentDrugsIF)
        {
            //if (string.IsNullOrEmpty(currentDrugsIF.text))
            //{
            //    currentDrugsIF.text = "";
            //}
            //else
            //{
            //    currentDrugsIF.text = Globals.currentUser.userCurrentDrugs;
            //}
            currentDrugsIF.text = Globals.currentUser.userCurrentDrugs;
        }
        if (previousOperationsIF)
        {
            //if (string.IsNullOrEmpty(previousOperationsIF.text))
            //{
            //    previousOperationsIF.text = "";
            //}
            //else
            //{
            //    previousOperationsIF.text = Globals.currentUser.userPreviousOperations;
            //}
            previousOperationsIF.text = Globals.currentUser.userPreviousOperations;
        }
        if (wearingGlassesToggle)
        {
            //if (string.IsNullOrEmpty(Globals.currentUser.userWearingGlasses))
            //{
            //    wearingGlassesToggle.isOn = false;
            //}
            //else
            //{
            //    if (Globals.currentUser.userWearingGlasses == "yes")
            //    {
            //        wearingGlassesToggle.isOn = true;
            //    }
            //    else
            //    {
            //        wearingGlassesToggle.isOn = false;
            //    }
            //}
            if (Globals.currentUser.userWearingGlasses == "yes")
            {
                wearingGlassesToggle.isOn = true;
            }
            else
            {
                wearingGlassesToggle.isOn = false;
            }
        }
        if (familyMembersChronicDiseasesIF)
        {
            //if (string.IsNullOrEmpty(familyMembersChronicDiseasesIF.text))
            //{
            //    familyMembersChronicDiseasesIF.text = "";
            //}
            //else
            //{
            //    familyMembersChronicDiseasesIF.text = Globals.currentUser.userFamilyChronicDiseases;
            //}
            familyMembersChronicDiseasesIF.text = Globals.currentUser.userFamilyChronicDiseases;
        }
        if (smokingToggle)
        {
            //if (string.IsNullOrEmpty(Globals.currentUser.userSmoking))
            //{
            //    smokingToggle.isOn = false;
            //}
            //else
            //{
            //    if (Globals.currentUser.userSmoking == "yes")
            //    {
            //        smokingToggle.isOn = true;
            //    }
            //    else
            //    {
            //        smokingToggle.isOn = false;
            //    }
            //}
            if (Globals.currentUser.userSmoking == "yes")
            {
                smokingToggle.isOn = true;
            }
            else
            {
                smokingToggle.isOn = false;
            }
        }
        if (alcoholToggle)
        {
            //if (string.IsNullOrEmpty(Globals.currentUser.userDrinksAlcohols))
            //{
            //    alcoholToggle.isOn = false;
            //}
            //else
            //{
            //    if (Globals.currentUser.userDrinksAlcohols == "yes")
            //    {
            //        alcoholToggle.isOn = true;
            //    }
            //    else
            //    {
            //        alcoholToggle.isOn = false;
            //    }
            //}
            if (Globals.currentUser.userDrinksAlcohols == "yes")
            {
                alcoholToggle.isOn = true;
            }
            else
            {
                alcoholToggle.isOn = false;
            }
        }
        if (userTypeDD)
        {
            //if (string.IsNullOrEmpty(Globals.currentUser.userType))
            //{
            //    userTypeDD.value = 0;
            //}
            //else
            //{
            //    if (Globals.currentUser.userType == "patient")
            //    {
            //        userTypeDD.value = 0;

            //    }
            //    else if (Globals.currentUser.userType == "doctor")
            //    {
            //        userTypeDD.value = 1;
            //    }
            //}
            if (Globals.currentUser.userType == "patient")
            {
                userTypeDD.value = 0;

            }
            else if (Globals.currentUser.userType == "doctor")
            {
                userTypeDD.value = 1;
            }
        }

        if (string.IsNullOrEmpty(Globals.currentUser.userType))
        {
            HelpersMan.HidePanel(doctorInfoPanel);
            HelpersMan.ShowPanel(patientInfoPanel);
        }
        else if (Globals.currentUser.userType == "patient")
        {
            HelpersMan.HidePanel(doctorInfoPanel);
            HelpersMan.ShowPanel(patientInfoPanel);
        }
        else if (Globals.currentUser.userType == "doctor")
        {
            HelpersMan.HidePanel(patientInfoPanel);
            HelpersMan.ShowPanel(doctorInfoPanel);
        }

        for (int i = 0; i < Globals.currentUser.userCerts.Length; i++)
        {
            certBtns[i].certPath = Globals.currentUser.userCerts[i];
            if (!(string.IsNullOrEmpty(certBtns[i].certPath)))
            {
                //certBtns[i].btn.GetComponent<Button>().interactable = false;
                certBtns[i].btn.GetComponentInChildren<Text>().text = "Added";
            }
        }

        Debug.Log("User Info Loaded ... ");
    }
    public void OpenNotificationsPanelBtnCallback()
    {
        Debug.Log("Notifications Panel is active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(notificationsPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(notificationsBtnContainer);
    }
    public void OpenContactADoctorPanelBtnCallback()
    {
        Debug.Log("Contact A Doctor Panel is active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(contactADoctorPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(contactADoctorBtnContainer);
    }
    public void OpenContactUsPanelBtnCallback()
    {
        Debug.Log("Contact Us Panel is active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(contactUsPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(contactUsBtnContainer);
    }
    public void OpenCommunityPanelBtnCallback()
    {
        Debug.Log("Community Panel is active ... ");
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(communityPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(communityBtnContainer);
        ShowFriendSuggestionsPanelBtnCallback();
    }
    public void SaveProfileBtnCallback()
    {
        Globals.currentUser.email = Globals.email;
        Globals.currentUser.username = Globals.username;
        if (darkModeToggle)
        {
            if (darkModeToggle.isOn)
            {
                Globals.currentUser.userColorMode = "Dark";
            }
            else
            {
                Globals.currentUser.userColorMode = "Normal";
            }
        }
        if (phoneNumberIF)
        {
            Globals.currentUser.userPhoneNumber = phoneNumberIF.text.ToString();
        }
        if (addressIF)
        {
            Globals.currentUser.userAddress = addressIF.text.ToString();
        }
        if (weightIF)
        {
            Globals.currentUser.userWeight = weightIF.text.ToString();
        }
        if (previousDiseasesIF)
        {
            Globals.currentUser.userPreviousDiseases = previousDiseasesIF.text.ToString();
        }
        if (currentDiseasesIF)
        {
            Globals.currentUser.userCurrentDiseases = currentDiseasesIF.text.ToString();
        }
        if (currentDrugsIF)
        {
            Globals.currentUser.userCurrentDrugs = currentDrugsIF.text.ToString();
        }
        if (previousOperationsIF)
        {
            Globals.currentUser.userPreviousOperations = previousOperationsIF.text.ToString();
        }
        if (wearingGlassesToggle)
        {
            if (wearingGlassesToggle.isOn)
            {
                Globals.currentUser.userWearingGlasses = "yes";
            }
            else
            {
                Globals.currentUser.userWearingGlasses = "no";
            }
        }
        if (familyMembersChronicDiseasesIF)
        {
            Globals.currentUser.userFamilyChronicDiseases = familyMembersChronicDiseasesIF.text.ToString();
        }
        if (smokingToggle)
        {
            if (smokingToggle.isOn)
            {
                Globals.currentUser.userSmoking = "yes";
            }
            else
            {
                Globals.currentUser.userSmoking = "no";
            }
        }
        if (alcoholToggle)
        {
            if (alcoholToggle.isOn)
            {
                Globals.currentUser.userDrinksAlcohols = "yes";
            }
            else
            {
                Globals.currentUser.userDrinksAlcohols = "no";
            }
        }
        if (userTypeDD)
        {
            if (userTypeDD.value == 0)
            {
                Globals.currentUser.userType = "patient";
            }
            else if (userTypeDD.value == 1)
            {
                Globals.currentUser.userType = "doctor";
            }
        }
        Globals.showLoadingPanel = true;
        string json = JsonUtility.ToJson(Globals.currentUser);
        //Debug.Log("User Current Color Mode: " + Globals.currentUser.userColorMode);
        FirebaseDatabase.DefaultInstance.RootReference.Child("users")
            .Child(Globals.userId).Child("profile").SetRawJsonValueAsync(json)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {

                    ViewMessageOnScreen("Changes Saved to your account .. Please Login Again");
                    //LogOut();
                }
                else
                {
                    ViewMessageOnScreen("Failed To Save Changes ... ");
                }
                Globals.showLoadingPanel = false;
            });
    }
    public void DarkModeToggleOnValueChangedCallback()
    {
        if (darkModeToggle)
        {
            if (darkModeToggle.isOn)
            {
                if (dashboardPanel)
                {
                    dashboardPanel.GetComponent<Image>().color = Globals.userColorDark;
                    Globals.currentUser.userColorMode = "Dark";
                }
            }
            else
            {
                if (dashboardPanel)
                {
                    dashboardPanel.GetComponent<Image>().color = Globals.userColorNormal;
                    Globals.currentUser.userColorMode = "Normal";
                }
            }
        }
    }
    public void ClosePhotoPreviewPanelBtnCallback()
    {
        HelpersMan.HidePanel(cameraShotPreviewPanel);
    }
    public void CapturePhotoBtnCallback()
    {
        if (cameraShotPreviewPanel && cameraShotPreviewImage)
        {
            HelpersMan.ShowPanel(cameraShotPreviewPanel);
            if (deviceCamera)
            {
                Texture2D snap = new Texture2D(deviceCamera.width, deviceCamera.height);
                snap.SetPixels(deviceCamera.GetPixels());
                snap.Apply();
                Globals.snapshotPhoto = snap;
                cameraShotPreviewImage.GetComponent<Image>().sprite = Sprite.Create(snap, new Rect(0.0f, 0.0f, snap.width, snap.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }
    public void SavePhotoBtnCallback()
    {
        if (cameraStreamPanel)
        {
            if (Globals.snapshotPhoto)
            {
                string savePath = Application.persistentDataPath;
                File.WriteAllBytes(savePath + "IMG" +
                    DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                    DateTime.Now.Day + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                    + DateTime.Now.Second.ToString() + ".png", Globals.snapshotPhoto.EncodeToPNG());
                Globals.currentSnapshotPath = savePath + "IMG" +
                    DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                    DateTime.Now.Day + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                    + DateTime.Now.Second.ToString() + ".png";
                ViewMessageOnScreen("Saved To Device ... ");
                Debug.Log("Saved To: " + Application.persistentDataPath);
            }
            //if (deviceCamera)
            //{
            //    string savePath = Application.persistentDataPath;
            //    Texture2D snap = new Texture2D(deviceCamera.width, deviceCamera.height);
            //    snap.SetPixels(deviceCamera.GetPixels());
            //    snap.Apply();
            //    System.IO.File.WriteAllBytes(savePath + "IMG" +
            //        DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + 
            //        DateTime.Now.Day + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() 
            //        + DateTime.Now.Second.ToString() + ".png", snap.EncodeToPNG());
            //    ViewMessageOnScreen("Saved To Device ... ");
            //    Debug.Log("Saved To: " + Application.persistentDataPath);
            //}
        }
    }
    public void SendPhotoByMailBtnCallback()
    {
        if (Globals.snapshotPhoto)
        {
            string savePath = Application.persistentDataPath;
            File.WriteAllBytes(savePath + "IMG" +
                DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                DateTime.Now.Day + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                + DateTime.Now.Second.ToString() + ".png", Globals.snapshotPhoto.EncodeToPNG());
            Globals.currentSnapshotPath = savePath + "IMG" +
                DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +
                DateTime.Now.Day + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()
                + DateTime.Now.Second.ToString() + ".png";
            //ViewMessageOnScreen("Saved To Device ... ");
            Debug.Log("Saved To: " + Application.persistentDataPath);
            byte[] data = Globals.snapshotPhoto.GetRawTextureData();
            MemoryStream memstream = new MemoryStream(data);
            ContentType contentType = new ContentType();
            contentType.MediaType = MediaTypeNames.Image.Jpeg;
            HelpersMan.SendEmail(
                "This is a diagnosis photo of my eye.\n Please respond if there are any problems. \nThanks.",
                Globals.username + " Eye Snapshot " + DateTime.Now.DayOfWeek + " " + DateTime.Now.Hour,
                "tmoteam77@gmail.com",
                new Attachment(Globals.currentSnapshotPath));
            ViewMessageOnScreen("Mail Sent ... ");
        }
        
    }
    public void StartVisionTestBtnCallback()
    {
        HelpersMan.HidePanel(startVisionTestPanel);
        Globals.visionTestStarted = true;
        Globals.visionTestResults.Clear();
        Globals.visionTestCounter = 0;
    }
    public void VisionTestLeftDirectionBtnCallback()
    {
        if (visionTestSymbol)
        {
            if (Globals.visionTestCounter >= 9)
            {
                Globals.showVisionTestResults = true;
                Globals.visionAcuityTestSelected = true;
                return;
            }
            if (visionTestSymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 180)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.visionTestCounter++;
            // Shuffle for the next pattern direction
            HelpersMan.ShuffleSymbolDirection(visionTestSymbol);
            // Scale Down Symbol 
            HelpersMan.ScaleDownSymbol(visionTestSymbol);
        }
    }
    public void VisionTestRightDirectionBtnCallback()
    {
        if (Globals.visionTestCounter >= 9)
        {
            Globals.showVisionTestResults = true;
            Globals.visionAcuityTestSelected = true;
            return;
        }
        if (visionTestSymbol)
        {
            if (visionTestSymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 0)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.visionTestCounter++;
            // Shuffle for the next pattern direction
            HelpersMan.ShuffleSymbolDirection(visionTestSymbol);
            // Scale Down Symbol 
            HelpersMan.ScaleDownSymbol(visionTestSymbol);
        }
    }
    public void VisionTestUpDirectionBtnCallback()
    {
        if (Globals.visionTestCounter >= 9)
        {
            Globals.showVisionTestResults = true;
            Globals.visionAcuityTestSelected = true;
            return;
        }
        if (visionTestSymbol)
        {
            if (visionTestSymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 90)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.visionTestCounter++;
            // Shuffle for the next pattern direction
            HelpersMan.ShuffleSymbolDirection(visionTestSymbol);
            // Scale Down Symbol 
            HelpersMan.ScaleDownSymbol(visionTestSymbol);
        }
    }
    public void VisionTestDownDirectionBtnCallback()
    {
        if (Globals.visionTestCounter >= 9)
        {
            Globals.showVisionTestResults = true;
            Globals.visionAcuityTestSelected = true;
            return;
        }
        if (visionTestSymbol)
        {
            if (visionTestSymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 270)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.visionTestCounter++;
            // Shuffle for the next pattern direction
            HelpersMan.ShuffleSymbolDirection(visionTestSymbol);
            // Scale Down Symbol 
            HelpersMan.ScaleDownSymbol(visionTestSymbol);
        }
    }
    public void OpenVisionAcuityTestPanelBtnCallback()
    {
        HideAllVisionTestPanels();
        HelpersMan.ShowPanel(visionAcuityTestPanel);
        HelpersMan.ShowPanel(startVisionTestPanel);
        DeHighlightAllVisionTestPanels();
        HelpersMan.HighlightPanel(visionAcuityTestBtnContainer);
        HelpersMan.ResetSymbolSizeAndRotation(visionTestSymbol);
        Globals.visionTestCounter = 0;
        Globals.showVisionTestResults = false;
        Globals.visionAcuityTestSelected = true;
        Globals.astigmatismTestSelected = false;
        Globals.lightSensitivityTestSelected = false;
        Globals.nearVisionTestSelected = false;
        Globals.colorVisionTestSelected = false;
        if (startTestPanelText && startTestPanelInstructionsText)
        {
            startTestPanelText.text = "Vision Acuity Test Selected ... ";
            startTestPanelInstructionsText.text = "Observe the symbol and choose to which direction it points.\n\nPerform the test with two eyes then close one eye and perform the test\n\nAim of this test is to check vision acuity ... ";
        }
    }
    public void OpenAstigmatismTestPanelBtnCallback()
    {
        HideAllVisionTestPanels();
        HelpersMan.ShowPanel(astigmatismTestPanel);
        HelpersMan.ShowPanel(startVisionTestPanel);
        DeHighlightAllVisionTestPanels();
        HelpersMan.HighlightPanel(astigmatismTestBtnContainer);
        Globals.astigmatismTestResult = "";
        Globals.showVisionTestResults = false;
        Globals.astigmatismTestSelected = true;
        Globals.visionAcuityTestSelected = false;
        Globals.lightSensitivityTestSelected = false;
        Globals.nearVisionTestSelected = false;
        Globals.colorVisionTestSelected = false;
        if (startTestPanelText && startTestPanelInstructionsText)
        {
            startTestPanelText.text = "Astigmatism Test Selected ... ";
            startTestPanelInstructionsText.text = "Observe the shape and verify if all the lines are the same or different.\n\nPerform the test with two eyes at first then close one eye and perform it\n\nAim of this test is to check the astigmatism .... ";
        }
    }
    public void AstigmatismYesBtnCallback()
    {
        Globals.astigmatismTestResult = "fail";
        Globals.astigmatismTestSelected = true;
        Globals.showVisionTestResults = true;
    }
    public void AstigmatismNoBtnCallback()
    {
        Globals.astigmatismTestResult = "pass";
        Globals.astigmatismTestSelected = true;
        Globals.showVisionTestResults = true;
    }
    public void OpenLightSensitivityTestPanelBtnCallback()
    {
        HideAllVisionTestPanels();
        HelpersMan.ShowPanel(lightSensitivityTestPanel);
        HelpersMan.ShowPanel(startVisionTestPanel);
        DeHighlightAllVisionTestPanels();
        HelpersMan.HighlightPanel(lightSensitvityTestBtnContainer);
        HelpersMan.ResetSymbolSizeAndRotation(lightSensitivitySymbol);
        HelpersMan.ResetSymbolOpacity(lightSensitivitySymbol);
        Globals.lightSensitivityTestCounter = 0;
        Globals.showVisionTestResults = false;
        Globals.visionAcuityTestSelected = false;
        Globals.astigmatismTestSelected = false;
        Globals.lightSensitivityTestSelected = true;
        Globals.nearVisionTestSelected = false;
        Globals.colorVisionTestSelected = false;
        if (startTestPanelText && startTestPanelInstructionsText)
        {
            startTestPanelText.text = "Light Senstivity Test Selected ... ";
            startTestPanelInstructionsText.text = "Observe the symbol and choose to which direction it points.\n\nPerform the test with two eyes at first then close one eye and perform it.\n\nAim of this test is to check the light sensitivity in the eyes .... ";
        }
    }
    public void LightSensitivityTestLeftDirectionBtnCallback()
    {
        if (lightSensitivitySymbol)
        {
            if (Globals.lightSensitivityTestCounter >= 18)
            {
                Globals.showVisionTestResults = true;
                Globals.lightSensitivityTestSelected = true;
                return;
            }
            if (lightSensitivitySymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 180)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.lightSensitivityTestCounter++;
            // Shuffle for the next pattern direction 
            HelpersMan.ShuffleSymbolDirection(lightSensitivitySymbol);
            // Scale Down Symbol 
            HelpersMan.SmallScaleDownSymbol(lightSensitivitySymbol);
            // Decrease Symbol Opacity
            HelpersMan.DecreaseSymbolOpacity(lightSensitivitySymbol);
        }
    }
    public void LightSensitivityTestRightDirectionBtnCallback()
    {
        if (lightSensitivitySymbol)
        {
            if (Globals.lightSensitivityTestCounter >= 18)
            {
                Globals.showVisionTestResults = true;
                Globals.lightSensitivityTestSelected = true;
                return;
            }
            if (lightSensitivitySymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 0)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.lightSensitivityTestCounter++;
            // Shuffle for the next pattern direction 
            HelpersMan.ShuffleSymbolDirection(lightSensitivitySymbol);
            // Scale Down Symbol 
            HelpersMan.SmallScaleDownSymbol(lightSensitivitySymbol);
            // Decrease Symbol Opacity
            HelpersMan.DecreaseSymbolOpacity(lightSensitivitySymbol);
        }
    }
    public void LightSensitivityTestUpDirectionBtnCallback()
    {
        if (lightSensitivitySymbol)
        {
            if (Globals.lightSensitivityTestCounter >= 18)
            {
                Globals.showVisionTestResults = true;
                Globals.lightSensitivityTestSelected = true;
                return;
            }
            if (lightSensitivitySymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 90)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.lightSensitivityTestCounter++;
            // Shuffle for the next pattern direction 
            HelpersMan.ShuffleSymbolDirection(lightSensitivitySymbol);
            // Scale Down Symbol 
            HelpersMan.SmallScaleDownSymbol(lightSensitivitySymbol);
            // Decrease Symbol Opacity
            HelpersMan.DecreaseSymbolOpacity(lightSensitivitySymbol);
        }
    }
    public void LightSensitivityTestDownDirectionBtnCallback()
    {
        if (lightSensitivitySymbol)
        {
            if (Globals.lightSensitivityTestCounter >= 18)
            {
                Globals.showVisionTestResults = true;
                Globals.lightSensitivityTestSelected = true;
                return;
            }
            if (lightSensitivitySymbol.gameObject.GetComponent<RectTransform>().eulerAngles.z == 270)
            {
                Globals.visionTestResults.Add("pass");
                Debug.Log("Right");
            }
            else
            {
                Globals.visionTestResults.Add("fail");
                Debug.Log("Wrong");
            }
            Globals.lightSensitivityTestCounter++;
            // Shuffle for the next pattern direction 
            HelpersMan.ShuffleSymbolDirection(lightSensitivitySymbol);
            // Scale Down Symbol 
            HelpersMan.SmallScaleDownSymbol(lightSensitivitySymbol);
            // Decrease Symbol Opacity
            HelpersMan.DecreaseSymbolOpacity(lightSensitivitySymbol);
        }
    }
    public void OpenNearVisionTestPanelBtnCallback()
    {
        HideAllVisionTestPanels();
        HelpersMan.ShowPanel(nearVisionTestPanel);
        HelpersMan.ShowPanel(startVisionTestPanel);
        DeHighlightAllVisionTestPanels();
        HelpersMan.HighlightPanel(nearVisionTestBtnContainer);
        Globals.nearVisionTestResult = "";
        Globals.showVisionTestResults = false;
        Globals.astigmatismTestSelected = false;
        Globals.visionAcuityTestSelected = false;
        Globals.lightSensitivityTestSelected = false;
        Globals.nearVisionTestSelected = true;
        Globals.colorVisionTestSelected = false;
        if (startTestPanelText && startTestPanelInstructionsText)
        {
            startTestPanelText.text = "Near Vision Test Selected ... ";
            startTestPanelInstructionsText.text = "Observe the shape and see if there is a difference betweeb red, green and blue areas.\n\nPerform the test with two eyes at first then close one eye and perform it.\n\nAim of this test is to check for Near Vision Defects in the eyes .... ";
        }
    }
    public void NearVisionRedBtnCallback()
    {
        Globals.showVisionTestResults = true;
        Globals.nearVisionTestSelected = true;
        Globals.nearVisionTestResult = "fail";
    }
    public void NearVisionGreenBtnCallback()
    {
        Globals.showVisionTestResults = true;
        Globals.nearVisionTestSelected = true;
        Globals.nearVisionTestResult = "fail";
    }
    public void NearVisionBlueBtnCallback()
    {
        Globals.showVisionTestResults = true;
        Globals.nearVisionTestSelected = true;
        Globals.nearVisionTestResult = "fail";
    }
    public void NearVisionNoBtnCallback()
    {
        Globals.showVisionTestResults = true;
        Globals.nearVisionTestSelected = true;
        Globals.nearVisionTestResult = "pass";
    }
    public void OpenColorVisionTestPanelBtnCallback()
    {
        HideAllVisionTestPanels();
        HelpersMan.ShowPanel(colorVisionTestPanel);
        HelpersMan.ShowPanel(startVisionTestPanel);
        DeHighlightAllVisionTestPanels();
        HelpersMan.HighlightPanel(colorVisionTestBtnContainer);
        Globals.colorVisionTestCounter = 0;
        if (colorVisionSymbol)
        {
            colorVisionSymbol.sprite = colorVisionSymbols[0].symbolIcon;
            //Globals.colorVisionTestCounter++;
        }
        Globals.showVisionTestResults = false;
        Globals.astigmatismTestSelected = false;
        Globals.visionAcuityTestSelected = false;
        Globals.lightSensitivityTestSelected = false;
        Globals.nearVisionTestSelected = false;
        Globals.colorVisionTestSelected = true;
        if (startTestPanelText && startTestPanelInstructionsText)
        {
            startTestPanelText.text = "Color Vision Test Selected ... ";
            startTestPanelInstructionsText.text = "Observe the given shapes and try to tell what number is in the shape.\n\nAim of this test is to check for color vision defects and color blindness .... ";
        }
    }
    public void ColorVisionNextBtnCallback()
    {
        if (colorVisionSymbol)
        {
            if (Globals.colorVisionTestCounter >= colorVisionSymbols.Count-1)
            {
                Globals.showVisionTestResults = true;
                Globals.colorVisionTestSelected = true;
                return;
            }
            if (colorVisionTestNumberIF)
            {
                if (colorVisionTestNumberIF.text == colorVisionSymbols[Globals.colorVisionTestCounter].symbolNumber)
                {
                    Globals.visionTestResults.Add("pass");
                    Debug.Log("Right");
                }
                else
                {
                    Globals.visionTestResults.Add("fail");
                    Debug.Log("Wrong");
                }
                colorVisionTestNumberIF.text = "";
                Globals.colorVisionTestCounter++;
                colorVisionSymbol.sprite = colorVisionSymbols[Globals.colorVisionTestCounter].symbolIcon;
            }
        }
    }
    public void UserTypeDDCallback()
    {
        if (userTypeDD.value == 0)  // patient
        {
            HelpersMan.HidePanel(doctorInfoPanel);
            HelpersMan.ShowPanel(patientInfoPanel);
        }
        else if (userTypeDD.value == 1)     // Doctor
        {
            HelpersMan.HidePanel(patientInfoPanel);
            HelpersMan.ShowPanel(doctorInfoPanel);
        }
    }
    public void ShowFriendsPanelBtnCallback()
    {
        HelpersMan.HidePanel(friendSuggestionsPanel);
        HelpersMan.HidePanel(friendSuggestionsPanelContainer);
        HelpersMan.ShowPanel(friendsPanelContainer);
        HelpersMan.ShowPanel(friendsPanel);
        Globals.showLoadingPanel = true;
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                try
                {
                    List<Friend> tempFriends = new List<Friend>();
                    DataSnapshot snapshot = task.Result;
                    foreach (DataSnapshot snap in snapshot.Children)
                    {
                        if (HelpersMan.CheckSuggestionInFriends(snap.Key.ToString()))
                        {
                            Friend buddy = new Friend(
                                snap.Child("profile").Child("username").Value.ToString(),
                                snap.Child("profile").Child("email").Value.ToString(),
                                snap.Child("profile").Child("userId").Value.ToString(),
                                "",
                                snap.Child("profile").Child("userType").Value.ToString());
                            foreach (DataSnapshot s in snap.Child("ratings").Children)
                            {
                                DoctorRating rating = new DoctorRating();
                                rating.review = s.Child("review").Value.ToString();
                                rating.stars = int.Parse(s.Child("stars").Value.ToString());
                                buddy.ratings.Add(rating);
                            }
                            tempFriends.Add(buddy);
                        }
                    }
                    Globals.currentUser.friends = tempFriends;
                }
                catch
                {
                    MessageBoxMan.Open("Failed to get user info ... ");
                }
            }
            Globals.showLoadingPanel = false;
            Globals.friendsLoaded = true;
        });
    }
    public void ShowFriendSuggestionsPanelBtnCallback()
    {
        HelpersMan.HidePanel(friendsPanel);
        HelpersMan.HidePanel(friendsPanelContainer);
        HelpersMan.ShowPanel(friendSuggestionsPanelContainer);
        HelpersMan.ShowPanel(friendSuggestionsPanel);
        Globals.showLoadingPanel = true;
        Globals.friendSuggestions.Clear();
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").GetValueAsync().ContinueWith(task =>
        {
            try
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    foreach (DataSnapshot snap in snapshot.Children)
                    {
                        Debug.Log("Snap: " + snap.Child("profile").Child("username").Key + " Value: " + snap.Child("profile").Child("username").Value);
                        //foreach (DataSnapshot shot in snap.Child("profile").Children)
                        //{
                        //    Debug.Log("Snap: " + shot.Key + " Value: " + shot.Value);
                        //}
                        Friend friend = new Friend();
                        friend.userId = snap.Child("profile").Child("userId").Value.ToString();
                        Debug.Log("Assigned user id ... ");
                        friend.username = snap.Child("profile").Child("username").Value.ToString();
                        Debug.Log("Assigned username ... ");
                        friend.email = snap.Child("profile").Child("email").Value.ToString();
                        Debug.Log("Assigned user email ... ");
                        friend.type = snap.Child("profile").Child("userType").Value.ToString();
                        Debug.Log("Assigned user type ... ");
                        friend.bio = "";
                        if (friend.userId != Globals.userId && !(HelpersMan.CheckSuggestionInFriends(friend.userId)))
                        {
                            Globals.friendSuggestions.Add(friend);
                        }
                        Debug.Log("Retrieved friend suggestion ... ");
                    }
                }
                Globals.friendSuggestionsLoaded = true;
                Globals.showLoadingPanel = false;
            }
            catch
            {
                MessageBoxMan.Open("Save your info before you proceed ... ");
                Globals.showLoadingPanel = false;
                //redirectToProfilePanel = true;
            }
        });
        
    }
    public void OpenAlarmsPanelBtnCallback()
    {
        HideAllPanels();
        HelpersMan.ShowPanel(dashboardPanel);
        HelpersMan.ShowPanel(alarmsPanel);
        DeHighlightAllPanels();
        HelpersMan.HighlightPanel(alarmsBtnContainer);

    }
    #endregion

    #region SocialPlatformUIAuxilaries
    void ClearAllPersonalInfo()
    {
        HelpersMan.ClearInputField(phoneNumberIF);
        HelpersMan.ClearInputField(addressIF);
        HelpersMan.ClearInputField(weightIF);
        HelpersMan.ClearInputField(previousDiseasesIF);
        HelpersMan.ClearInputField(currentDiseasesIF);
        HelpersMan.ClearInputField(currentDrugsIF);
        HelpersMan.ClearInputField(previousOperationsIF);
        HelpersMan.ClearInputField(familyMembersChronicDiseasesIF);
        HelpersMan.ResetDropDown(userTypeDD);
        HelpersMan.ResetToggle(wearingGlassesToggle);
        HelpersMan.ResetToggle(darkModeToggle);
        HelpersMan.ResetToggle(smokingToggle);
        HelpersMan.ResetToggle(alcoholToggle);
        for (int i = 0; i < addCertBtns.Count; i++)
        {
            addCertBtns[i].gameObject.GetComponentInChildren<Text>().text = "Add";
        }
        for (int i = 0; i < certBtns.Count; i++)
        {
            certBtns[i].certPath = "";
        }
    }
    void LoadUserProfile()
    {
        Globals.currentUser.email = Globals.email;
        Globals.currentUser.username = Globals.username;
        if (darkModeToggle)
        {
            if (darkModeToggle.isOn)
            {
                Globals.currentUser.userColorMode = "Dark";
            }
            else
            {
                Globals.currentUser.userColorMode = "Normal";
            }
        }
        if (phoneNumberIF)
        {
            Globals.currentUser.userPhoneNumber = phoneNumberIF.text.ToString();
        }
        if (addressIF)
        {
            Globals.currentUser.userAddress = addressIF.text.ToString();
        }
        if (weightIF)
        {
            Globals.currentUser.userWeight = weightIF.text.ToString();
        }
        if (previousDiseasesIF)
        {
            Globals.currentUser.userPreviousDiseases = previousDiseasesIF.text.ToString();
        }
        if (currentDiseasesIF)
        {
            Globals.currentUser.userCurrentDiseases = currentDiseasesIF.text.ToString();
        }
        if (currentDrugsIF)
        {
            Globals.currentUser.userCurrentDrugs = currentDrugsIF.text.ToString();
        }
        if (previousOperationsIF)
        {
            Globals.currentUser.userPreviousOperations = previousOperationsIF.text.ToString();
        }
        if (wearingGlassesToggle)
        {
            if (wearingGlassesToggle.isOn)
            {
                Globals.currentUser.userWearingGlasses = "yes";
            }
            else
            {
                Globals.currentUser.userWearingGlasses = "no";
            }
        }
        if (familyMembersChronicDiseasesIF)
        {
            Globals.currentUser.userFamilyChronicDiseases = familyMembersChronicDiseasesIF.text.ToString();
        }
        if (smokingToggle)
        {
            if (smokingToggle.isOn)
            {
                Globals.currentUser.userSmoking = "yes";
            }
            else
            {
                Globals.currentUser.userSmoking = "no";
            }
        }
        if (alcoholToggle)
        {
            if (alcoholToggle.isOn)
            {
                Globals.currentUser.userDrinksAlcohols = "yes";
            }
            else
            {
                Globals.currentUser.userDrinksAlcohols = "no";
            }
        }
        if (userTypeDD)
        {
            if (userTypeDD.value == 0)
            {
                Globals.currentUser.userType = "patient";
            }
            else if (userTypeDD.value == 1)
            {
                Globals.currentUser.userType = "doctor";
            }
        }
    }
    void LoadFriendsListUI()
    {
        for (int j = 0; j < friendsList.Count; j++)
        {
            Destroy(friendsList[j]);
        }
        friendsList.Clear();
        for (int i = 0; i < Globals.currentUser.friends.Count; i++)
        {
            if (friendSlotPrefab && friendsPanel && Globals.currentUser.friends[i].userId != Globals.userId)
            {
                // Instantiate the friend slot to the friends panel transform
                GameObject slot = Instantiate(friendSlotPrefab, friendsPanel.transform);
                slot.GetComponent<FriendSlot>().friend = Globals.currentUser.friends[i];
                slot.GetComponentInChildren<Button>().onClick.AddListener(delegate
                {
                    slot.GetComponent<FriendSlot>().ViewFriendProfile(true);
                });
                slot.GetComponentsInChildren<Text>()[0].text = Globals.currentUser.friends[i].username;

                // Keep track of the added object 
                friendsList.Add(slot);
            }
        }
    }
    void LoadFriendSuggestionsUI()
    {
        for (int j = 0; j < friendSuggestionsList.Count; j++)
        {
            Destroy(friendSuggestionsList[j]);
        }
        friendSuggestionsList.Clear();
        for (int i = 0; i < Globals.friendSuggestions.Count; i++)
        {
            if (friendSuggestionSlotPrefab && friendSuggestionsPanel)
            {
                if (!(HelpersMan.CheckSuggestionInFriends(Globals.friendSuggestions[i].userId)) && Globals.friendSuggestions[i].userId != Globals.userId)
                {
                    // Instantiate the friend suggestion slot Prefab to the parent transform of friend suggestions panel
                    GameObject slot = Instantiate(friendSuggestionSlotPrefab, friendSuggestionsPanel.transform);
                    //Friend friend = new Friend();
                    //friend.userId = Globals.friendSuggestions[i].userId;
                    //friend.username = Globals.friendSuggestions[i].username;
                    //friend.email = Globals.friendSuggestions[i].email;
                    //friend.bio = Globals.friendSuggestions[i].bio;
                    //friend.type = Globals.friendSuggestions[i].type;
                    slot.GetComponent<FriendSlot>().friend = Globals.friendSuggestions[i];
                    slot.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate 
                    {
                        slot.GetComponent<FriendSlot>().ViewFriendProfile(false);
                    });
                    slot.GetComponentsInChildren<Button>()[1].onClick.AddListener(slot.GetComponent<FriendSlot>().AddToFriends);
                    // Assign username in suggestion slot 
                    slot.GetComponentsInChildren<Text>()[0].text = Globals.friendSuggestions[i].username;

                    // Keep track of the added object 
                    friendSuggestionsList.Add(slot);
                }
            }
        }
        Debug.Log("Loaded Friends Suggestions .... ");
    }
    #endregion

    #region AlarmFunctionality
    public GameObject alarmsContainerPanel;
    GameObject alarmSlot;
    GameObject addAlarmPanelPrefab;

    System.Timers.Timer alarmTimer;

    static string hours;
    static string midnightStatus;
    static string minutes;
    public void AddAlarmBtnCallback()
    {
        if (alarmsContainerPanel && alarmSlot && addAlarmPanelPrefab && alarmsPanel)
        {
            GameObject addAlarmPanelObject = Instantiate(addAlarmPanelPrefab, alarmsPanel.transform);
            // Assign Hours Field 
            hours = addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[0].options[addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[0].value].text;
            // Assign day half period
            midnightStatus = addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[1].options[addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[1].value].text;
            // Assign Minutes Field 
            minutes = addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[2].options[addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[2].value].text;
            // Assign Add Click event
            addAlarmPanelObject.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
            {
                // Assign Hours Field 
                hours = addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[0].options[addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[0].value].text;
                // Assign day half period
                midnightStatus = addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[1].options[addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[1].value].text;
                // Assign Minutes Field 
                minutes = addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[2].options[addAlarmPanelObject.GetComponentsInChildren<Dropdown>()[2].value].text;
                // Assign a background process for alarm handling 
                Debug.Log("Alarm Added ... ");
                GameObject alarmSlotObject = Instantiate(alarmSlot, alarmsContainerPanel.transform);
                alarmSlotObject.GetComponentsInChildren<Text>()[1].text = "Alarm set to: " + hours + " " + midnightStatus + " and " + minutes + " minutes";
                // Measure the time difference between current time and specified time 
                double hourDiff;
                double minuteDiff;
                if (midnightStatus == "P.M")
                {
                    hours = (int.Parse(hours) + 12).ToString();
                    if (double.Parse(hours) < DateTime.Now.Hour)
                    {
                        // Next day alarm 
                        hourDiff = 24 - (DateTime.Now.Hour - double.Parse(hours));
                    }
                    else
                    {
                        // Current day alarm
                        hourDiff = double.Parse(hours) - DateTime.Now.Hour;
                    }
                    if (double.Parse(minutes) < DateTime.Now.Minute)
                    {
                        if (hourDiff == 0)
                        {
                            // Next day same hour alarm 
                            hourDiff = 23;
                            minuteDiff = DateTime.Now.Minute - double.Parse(minutes);
                        }
                        else
                        {
                            minuteDiff = DateTime.Now.Minute - double.Parse(minutes);
                        }
                    }
                    else
                    {
                        minuteDiff = double.Parse(minutes) - DateTime.Now.Minute;
                    }
                }
                else
                {
                    if (double.Parse(hours) < DateTime.Now.Hour)
                    {
                        // Next day alarm 
                        hourDiff = 24 - (DateTime.Now.Hour - double.Parse(hours));
                    }
                    else
                    {
                        // Current day alarm 
                        hourDiff = double.Parse(hours) - DateTime.Now.Hour;
                    }
                    if (double.Parse(minutes) < DateTime.Now.Minute)
                    {
                        if (hourDiff == 0)
                        {
                            // Next day same hour alarm 
                            hourDiff = 23;
                            minuteDiff = DateTime.Now.Minute - double.Parse(minutes);
                        }
                        else
                        {
                            minuteDiff = DateTime.Now.Minute - double.Parse(minutes);
                        }
                    }
                    else
                    {
                        minuteDiff = double.Parse(minutes) - DateTime.Now.Minute;
                    }
                }
                // Create a fire time for the alarm timer
                DateTime fireTime = DateTime.Now;
                fireTime.AddHours(hourDiff);
                fireTime.AddMinutes(minuteDiff);
                // Set the fire timer 
                alarmTimer = new System.Timers.Timer();
                alarmTimer.Interval = fireTime.Hour * 60 * 60 + fireTime.Minute * 60;
                alarmTimer.Elapsed += OnAlarmFired;
                alarmTimer.Start();
                alarmSlotObject.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
                {
                    // Remove alarm callback
                    Destroy(alarmSlotObject.gameObject);
                });
                // Close Add Alarm Panel
                Destroy(addAlarmPanelObject.gameObject);
                // Message box confirming alaram addition 
                MessageBoxMan.Open("Alarm Added ... ");
                // Add an android notification in case application is not opened 
                ScheduleGenericNotification("Medication Time", "your medication time is now ... ", fireTime);
            });
            addAlarmPanelObject.GetComponentsInChildren<Button>()[1].onClick.AddListener(delegate
            {
                // Close Add Alarm Panel 
                Destroy(addAlarmPanelObject.gameObject);
            });
        }
    }
    void OnAlarmFired(object sender, System.Timers.ElapsedEventArgs e)
    {
        // On Time Elapsed stop timer and remove the alarm slot;
        alarmTimer.Stop();
        MessageBoxMan.Open("Alarm Fired ... ");
    }
    #endregion

    #region BackgroundProcesses
    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            // start service notification in the background
        }
        else
        {
            // stop service notification in the background
        }
    }
    #region NotificationsFunctionality
    void ScheduleGenericNotification(string title, string text, DateTime fireTime)
    {
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = text;
        notification.FireTime = fireTime;

        AndroidNotificationCenter.SendNotification(notification, "710120277560-4mj7olb7o4f618r1op4t1ujovsmfnuf8.apps.googleusercontent.com");
    }
    #endregion
    #endregion

    #region OneToOneChatFunctionality
    GameObject chatPanel;
    GameObject chatPanelPrefab;
    GameObject thisUserMessageSlotPrefab;
    GameObject otherUserMessageSlotPrefab;
    public static PubNub pubnub;
    Queue<GameObject> chatMessageQueue = new Queue<GameObject>();
    int messageCounter = 0;
    bool messageReceived = false;
    int refreshTimes = 0;

    void LoadChatPanelUI(string otherUserChatId)
    {
        if (chatPanelPrefab && dashboardPanel && thisUserMessageSlotPrefab && otherUserMessageSlotPrefab)
        {
            // Add Chat Panel to UI
            chatPanel = Instantiate(chatPanelPrefab, dashboardPanel.transform);
            // Assign Btns Callbacks
            chatPanel.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
            {
                // Send Message Btn Callback
                // When the user clicks the send btn
                string msgText = chatPanel.GetComponentsInChildren<InputField>()[0].text;
                if (!(string.IsNullOrEmpty(msgText)))
                {
                    JSONInformation publishMessage = new JSONInformation();
                    publishMessage.username = Globals.username;
                    publishMessage.text = msgText;
                    string publishMessageToJSON = JsonUtility.ToJson(publishMessage);

                    // Publish the Json object to the assigned PubNub Channel 
                    pubnub.Publish().Channel(otherUserChatId).Message(publishMessageToJSON).Async((result, status) =>
                    {
                        if (status.Error)
                        {
                            MessageBoxMan.Open("Error occured ... ");
                        }
                        else
                        {
                            Debug.Log("Message Sent with time: " + result.Timetoken);
                            //Debug.Log(HelpersMan.GenerateRandomeID(100));
                        }
                        LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponentsInChildren<Image>()[2].gameObject.GetComponent<RectTransform>());
                        messageReceived = true;
                    });
                }
                chatPanel.GetComponentsInChildren<InputField>()[0].text = "";
                chatPanel.GetComponentsInChildren<InputField>()[0].Select();
                //messageReceived = true;
            });
            // Assign Voice Chat btn 
            chatPanel.GetComponentsInChildren<Button>()[1].onClick.AddListener(delegate
            {
                openVoiceChatPanel = true;
            });
            chatPanel.GetComponentsInChildren<Button>()[chatPanel.GetComponentsInChildren<Button>().Length - 1].onClick.AddListener(delegate
            {
                // Close Chat Panel
                Destroy(chatPanel.gameObject);
            });
            //LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponent<RectTransform>());
            // Fetch Messages
            Globals.showLoadingPanel = true;
            pubnub.FetchMessages().Channels(new List<string> { otherUserChatId }).Count(30).Async((result, status) =>
            {
                if (status.Error)
                {
                    MessageBoxMan.Open("Error while fetching messages ... ");
                }
                else
                {
                    foreach (KeyValuePair<string, List<PNMessageResult>> kvp in result.Channels)
                    {
                        foreach (PNMessageResult pnMessageResult in kvp.Value)
                        {
                            // Create a message UI 
                            JSONInformation msg = JsonUtility.FromJson<JSONInformation>(pnMessageResult.Payload.ToString());
                            if (msg.username == Globals.username)
                            {
                                // Add this user chat slot 
                                GameObject thisUserMessageSlot = Instantiate(thisUserMessageSlotPrefab, chatPanel.GetComponentsInChildren<Image>()[2].gameObject.transform);
                                thisUserMessageSlot.GetComponentsInChildren<Text>()[0].text = msg.text;
                                thisUserMessageSlot.GetComponentsInChildren<Text>()[0].text = ArabicFixer.Fix(thisUserMessageSlot.GetComponentsInChildren<Text>()[0].text);
                                chatMessageQueue.Enqueue(thisUserMessageSlot);
                                messageCounter++;
                            }
                            else
                            {
                                // Add other user caht slot 
                                GameObject otherUserMessageSlot = Instantiate(otherUserMessageSlotPrefab, chatPanel.GetComponentsInChildren<Image>()[2].gameObject.transform);
                                otherUserMessageSlot.GetComponentsInChildren<Text>()[0].text = msg.text;
                                otherUserMessageSlot.GetComponentsInChildren<Text>()[0].text = ArabicFixer.Fix(otherUserMessageSlot.GetComponentsInChildren<Text>()[0].text);
                                chatMessageQueue.Enqueue(otherUserMessageSlot);
                                messageCounter++;
                            }
                        }
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponentsInChildren<Image>()[2].gameObject.GetComponent<RectTransform>());
                    //// Refresh chat panel
                    //chatPanel.GetComponentsInChildren<Image>()[1].gameObject.GetComponent<VerticalLayoutGroup>().enabled = false;
                    //chatPanel.GetComponentsInChildren<Image>()[1].gameObject.GetComponent<VerticalLayoutGroup>().enabled = true;
                    //chatPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
                    //chatPanel.GetComponent<VerticalLayoutGroup>().enabled = true;
                }
                //LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponent<RectTransform>());
                messageReceived = true;
                Globals.showLoadingPanel = false;
            });
            // Subscribe on this channel event 
            pubnub.SubscribeCallback += (sender, e) =>
            {
                Globals.showLoadingPanel = true;
                SubscribeEventEventArgs message = e as SubscribeEventEventArgs;
                if (message.MessageResult != null)
                {
                    JSONInformation msg = JsonUtility.FromJson<JSONInformation>(message.MessageResult.Payload.ToString());
                    // Create Chat 
                    if (msg.username == Globals.username)
                    {
                        // Add this user chat slot 
                        GameObject thisUserMessageSlot = Instantiate(thisUserMessageSlotPrefab, chatPanel.GetComponentsInChildren<Image>()[2].gameObject.transform);
                        thisUserMessageSlot.GetComponentsInChildren<Text>()[0].text = msg.text;
                        thisUserMessageSlot.GetComponentsInChildren<Text>()[0].text = ArabicFixer.Fix(thisUserMessageSlot.GetComponentsInChildren<Text>()[0].text);
                        chatMessageQueue.Enqueue(thisUserMessageSlot);
                        messageCounter++;
                    }
                    else
                    {
                        // Add other user caht slot 
                        GameObject otherUserMessageSlot = Instantiate(otherUserMessageSlotPrefab, chatPanel.GetComponentsInChildren<Image>()[2].gameObject.transform);
                        otherUserMessageSlot.GetComponentsInChildren<Text>()[0].text = msg.text;
                        otherUserMessageSlot.GetComponentsInChildren<Text>()[0].text = ArabicFixer.Fix(otherUserMessageSlot.GetComponentsInChildren<Text>()[0].text);
                        chatMessageQueue.Enqueue(otherUserMessageSlot);
                        messageCounter++;
                    }
                    //// Sync Chat 
                    //if (messageCounter > 30)
                    //{
                    //    // Delete the first game object in the queue
                    //    GameObject deleteChat = chatMessageQueue.Dequeue();
                    //    Destroy(deleteChat);
                    //    messageCounter--;
                    //}
                    LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponentsInChildren<Image>()[2].gameObject.GetComponent<RectTransform>());
                }
                //// Refresh chat panel
                //chatPanel.GetComponentsInChildren<Image>()[1].gameObject.GetComponent<VerticalLayoutGroup>().enabled = false;
                //chatPanel.GetComponentsInChildren<Image>()[1].gameObject.GetComponent<VerticalLayoutGroup>().enabled = true;
                //chatPanel.GetComponent<VerticalLayoutGroup>().enabled = false;
                //chatPanel.GetComponent<VerticalLayoutGroup>().enabled = true;
                //LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponent<RectTransform>());
                messageReceived = true;
                Globals.showLoadingPanel = false;
            };
            // Subscribe to a pubnub channel and receive messages on that channel
            pubnub.Subscribe().Channels(new List<string> { otherUserChatId }).WithPresence().Execute();
        }
    }

    #endregion

    #region VoiceChatFunctionality
    public static IRtcEngine mRtcEngine;
    public uint mRemotePeer;
    GameObject voiceChatPanelPrefab;
    GameObject voiceChatPanelObj;
    bool openVoiceChatPanel = false;
    public static string voiceChatId = "";

    void VoiceCall(string otherUserChatId)
    {
        if (!(string.IsNullOrEmpty(otherUserChatId)))
        {
            // Add Voice Chat Panel Object 
            if (voiceChatPanelPrefab && dashboardPanel)
            {
                voiceChatPanelObj = Instantiate(voiceChatPanelPrefab, dashboardPanel.transform);
                // Assign its on close event 
                voiceChatPanelObj.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
                {
                    // Remove audio resources and destroy voice chat panel
                    Destroy(voiceChatPanelObj.gameObject);
                });
                // Set up voice chat functionality 
                JoinAudioChannel(otherUserChatId);
            }
        }
    }
    void LoadAgoraEngine()
    {
        // Start Agora SDK 
        if (mRtcEngine != null)
        {
            Debug.Log("Agora Engine is already initialized ... ");
            return;
        }

        // Init RTC Engine
        mRtcEngine = IRtcEngine.getEngine(Globals.agoraAppId);
        // Set the engine to log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG);

    }
    void JoinAudioChannel(string channelName)
    {
        if (!(string.IsNullOrEmpty(channelName)))
        {
            LoadAgoraEngine();
            if (mRtcEngine != null)
            {
                Debug.Log("Joining Channel " + channelName);

                // Set Callbacks 
                mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
                mRtcEngine.OnUserJoined += OnUserJoined;
                mRtcEngine.OnUserOffline += OnUserOffline;

                // Enable Audio 
                mRtcEngine.EnableAudio();

                // Join Audio Chat Channel 
                mRtcEngine.JoinChannel(channelName, null, 0);
            }
            else
            {
                Debug.Log("Agora Engine is not initialized ... ");
            }
        }
    }
    void LeaveAudioChannel()
    {
        if (mRtcEngine != null)
        {
            Debug.Log("Leaving Channel ... ");

            // Leave Channel 
            mRtcEngine.LeaveChannel();
        }
        else
        {
            Debug.Log("Agora engine is not initialized .... ");
        }
    }
    void UnloadAgoraEngine()
    {
        Debug.Log("Unloading Agora Engine .... ");

        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }
    // Agora Engine Callbacks 
    void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("Successfully joined channel: " + channelName + " with ID: " + uid + " after TIME: " + elapsed);
        if (voiceChatPanelObj)
        {
            // Assign its on close event 
            voiceChatPanelObj.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
            {
                // Remove audio resources and destroy voice chat panel
                LeaveAudioChannel();
                Destroy(voiceChatPanelObj.gameObject);
            });
        }
    }
    void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log("New User has joined with ID: " + uid);
        mRemotePeer = uid;
        if (voiceChatPanelObj)
        {
            // Assign its on close event 
            voiceChatPanelObj.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
            {
                // Remove audio resources and destroy voice chat panel
                LeaveAudioChannel();
                Destroy(voiceChatPanelObj.gameObject);
            });
        }
    }
    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        Debug.Log("User with ID: " + uid + " has left the channel for Reason: " + reason.ToString());
    }
    #endregion

    #region InAppNotitificationsFunctionality

    #endregion
    ///////////////////// Database Functionality ////////////////////////////
    ///
    #region DBFunctionality
    public static string databaseReference = "https://tmov1-31d59.firebaseio.com/";

    #region SaveData
    void SaveUserInfo(User user, string userId)
    {
        RestClient.Put(databaseReference + "users/" + userId + "/profile.json", user);
        ViewMessageOnScreen("User Info Updated ... ");
    }
    #endregion
    #region LoadData
    void LoadUserInfo()
    {
        if (Globals.userId != "")
        {
            FirebaseDatabase.DefaultInstance.GetReference("users").Child(Globals.userId).Child("profile").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    IDictionary dictUser = (IDictionary)snapshot.Value;
                    Globals.currentUser = new User(
                        dictUser["email"].ToString(),
                        dictUser["username"].ToString(),
                        dictUser["userId"].ToString(),
                        dictUser["userColorMode"].ToString()
                    );
                    Debug.Log("Retrieved User Info ... ");
                }
            });
        }
    }
    #endregion
    #endregion
}
