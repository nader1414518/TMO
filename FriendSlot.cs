using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;

public class FriendSlot : MonoBehaviour
{
    public bool friendAdded = false;
    public bool destroyAddReviewPanel = false;
    public static bool openChatPanel = false;
    public static string openedChatOtherUserId = "";
    public Friend friend = new Friend();
    public GameObject friendProfileTextPrefab;
    public GameObject friendProfilePanelPrefab;
    public GameObject reviewLabelPrefab;
    public GameObject reviewStarsSliderPrefab;
    public GameObject addReviewBtnPrefab;
    public GameObject addReviewPanelPrefab;
    public GameObject privateChatBtnPrefab;
    public GameObject communityPanel;

    public GameObject addReviewPanel;
    public GameObject profilePanel;
    public GameObject innerPanel;
    public GameObject noReviewsAvailableTxt;

    public string tempReview = "";
    public int tempStars = 0;

    void OnEnable()
    {
        friendAdded = false;
        friendProfileTextPrefab = Resources.Load<GameObject>("FriendProfileInfoText");
        friendProfilePanelPrefab = Resources.Load<GameObject>("FriendProfilePanel");
        reviewLabelPrefab = Resources.Load<GameObject>("ReviewsLabel");
        reviewStarsSliderPrefab = Resources.Load<GameObject>("ReviewStarSlider");
        addReviewBtnPrefab = Resources.Load<GameObject>("AddReviewBtn");
        addReviewPanelPrefab = Resources.Load<GameObject>("AddReviewPanel");
        privateChatBtnPrefab = Resources.Load<GameObject>("PrivateMessageBtn");
        communityPanel = GameObject.FindGameObjectWithTag("CommunityPanel");
    }

    public void AddToFriends()
    {
        Globals.currentUser.friends.Add(friend);
        Debug.Log("Added Friend To List ... ");
        string json = JsonUtility.ToJson(friend);
        Friend currentUser = new Friend(Globals.username, Globals.email, Globals.userId, "", Globals.currentUser.userType);
        string currentUserJson = JsonUtility.ToJson(currentUser);
        Globals.showLoadingPanel = true;
        Debug.Log("JSON Data: " + json);
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child("users")
            .Child(Globals.userId)
            .Child("friends")
            .Child(friend.userId)
            .SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child("users")
                    .Child(friend.userId)
                    .Child("friends")
                    .Child(Globals.userId)
                    .SetRawJsonValueAsync(currentUserJson).ContinueWith(task1 =>
                    {
                        if (task1.IsCompleted)
                        {
                            friendAdded = true;
                            Globals.showLoadingPanel = false;
                        }
                        else
                        {
                            MessageBoxMan.Open("Couldn't add friend. please try again ... ");
                            Globals.showLoadingPanel = false;
                        }
                    });
            }
            else
            {
                MessageBoxMan.Open("Couldn't add friend. please try again ... ");
                Globals.showLoadingPanel = false;
            }
        });
    }

    public void ViewFriendProfile(bool isInFriends)
    {
        Debug.Log("View Friend Profile Btn Clicked ... ");
        if (friendProfileTextPrefab && friendProfilePanelPrefab && communityPanel)
        {
            profilePanel = Instantiate(friendProfilePanelPrefab, communityPanel.transform);
            innerPanel = profilePanel.GetComponentsInChildren<Image>()[1].gameObject; 
            if (innerPanel)
            {
                // Instantiate Profile Info 
                // Username Text
                GameObject usernameText = Instantiate(friendProfileTextPrefab, innerPanel.transform);
                usernameText.GetComponent<Text>().text = "Username: " + friend.username;
                // Email Text
                GameObject emailText = Instantiate(friendProfileTextPrefab, innerPanel.transform);
                emailText.GetComponent<Text>().text = "Email: " + friend.email;
                // User Type Text
                GameObject typeText = Instantiate(friendProfileTextPrefab, innerPanel.transform);
                typeText.GetComponent<Text>().text = "Role: " + friend.type;
                // Instantiate doctor reviews
                if (friend.type == "doctor")
                {
                    if (reviewLabelPrefab && reviewStarsSliderPrefab)
                    {
                        // Put a separator text for reviews area
                        GameObject reviewAreaLabel = Instantiate(friendProfileTextPrefab, innerPanel.transform);
                        reviewAreaLabel.GetComponent<Text>().text = "Reviews";
                        reviewAreaLabel.GetComponent<Text>().fontStyle = FontStyle.Bold;
                        reviewAreaLabel.GetComponent<Text>().fontSize = 15;

                        // Iterate through reviews if there are any 
                        if (friend.ratings.Count == 0)
                        {
                            noReviewsAvailableTxt = Instantiate(friendProfileTextPrefab, innerPanel.transform);
                            noReviewsAvailableTxt.GetComponent<Text>().text = "No reviews available for this user at the moment ... ";
                            noReviewsAvailableTxt.GetComponent<Text>().fontStyle = FontStyle.Italic;
                            noReviewsAvailableTxt.GetComponent<Text>().fontSize = 12;
                        }
                        else
                        {
                            for (int i = 0; i < friend.ratings.Count; i++)
                            {
                                GameObject labelTxt = Instantiate(reviewLabelPrefab, innerPanel.transform);
                                labelTxt.GetComponent<Text>().text = friend.ratings[i].review;

                                GameObject reviewSlider = Instantiate(reviewStarsSliderPrefab, innerPanel.transform);
                                reviewSlider.GetComponent<Slider>().value = (float)(friend.ratings[i].stars / 5);
                            }
                        }
                        // Add a button for adding a review 
                        if (addReviewBtnPrefab && addReviewPanelPrefab)
                        {
                            GameObject addReviewBtn = Instantiate(addReviewBtnPrefab, innerPanel.transform);
                            addReviewBtn.GetComponent<Button>().onClick.AddListener(delegate
                            {
                                // Open Add Review Panel
                                addReviewPanel = Instantiate(addReviewPanelPrefab, communityPanel.transform);
                                addReviewPanel.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                                    // Submit review to firebase database 
                                    string review = addReviewPanel.GetComponentInChildren<InputField>().text;
                                    int stars = (int)(addReviewPanel.GetComponentInChildren<Slider>().value * 5.0f);
                                    if (string.IsNullOrEmpty(review))
                                    {
                                        MessageBoxMan.Open("Please enter a review ... ");
                                    }
                                    else
                                    {
                                        tempReview = review;
                                        tempStars = stars;
                                        DoctorRating ratingObj = new DoctorRating();
                                        ratingObj.review = review;
                                        ratingObj.stars = stars;
                                        string ratingJson = JsonUtility.ToJson(ratingObj);
                                        Globals.showLoadingPanel = true;
                                        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference
                                        .Child("users")
                                        .Child(friend.userId)
                                        .Child("ratings").Child(Globals.userId)
                                        .SetRawJsonValueAsync(ratingJson).ContinueWith(task =>
                                        {
                                            if (task.IsCompleted)
                                            {
                                                MessageBoxMan.Open("Submitted review successfully ... ");
                                                destroyAddReviewPanel = true;
                                            }
                                            Globals.showLoadingPanel = false;
                                        });
                                    }
                                });
                            });
                        }
                    }
                }
                if (isInFriends)
                {
                    // Add Private Chat Btn
                    if (privateChatBtnPrefab)
                    {
                        GameObject openChatPanelBtnObj = Instantiate(privateChatBtnPrefab, innerPanel.transform);
                        openChatPanelBtnObj.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            //Globals.showLoadingPanel = true;
                            // If chat id is not avaialble create a new one (this means that a new chat is created)
                            FirebaseDatabase.DefaultInstance.RootReference
                            .Child("users")
                            .Child(friend.userId)
                            .Child("friends").GetValueAsync().ContinueWith(task =>
                            {
                                if (task.IsCompleted)
                                {
                                    DataSnapshot snapshot = task.Result;
                                    // Check if current user has a mutual chat id in the friends list of the other person
                                    if (string.IsNullOrEmpty(snapshot.Child(Globals.userId).Child("userChatId").Value.ToString()))  // If chat is opened for the first time
                                    {
                                        // Chat for the first time (save mutual chat id for the two users)
                                        Friend modFriend = new Friend();
                                        modFriend.userId = friend.userId;
                                        modFriend.username = friend.username;
                                        modFriend.type = friend.type;
                                        modFriend.email = friend.email;
                                        modFriend.bio = friend.bio;
                                        modFriend.ratings = friend.ratings;
                                        Debug.Log("User Chat Id: " + snapshot.Child(Globals.userId).Child("userId").Value.ToString());
                                        string randChatId = "Chat" + Globals.userId + friend.userId;
                                        Debug.Log("Generated Chat ID: " + randChatId);
                                        modFriend.userChatId = randChatId;
                                        string otherUserJson = JsonUtility.ToJson(modFriend);
                                        FirebaseDatabase.DefaultInstance.RootReference
                                        .Child("users")
                                        .Child(Globals.userId)
                                        .Child("friends")
                                        .Child(friend.userId).SetRawJsonValueAsync(otherUserJson).ContinueWith(fTask =>
                                        {
                                            if (fTask.IsCompleted)   // Saved the chat id for the other user 
                                            {
                                                // Save the chat id for the current user
                                                Friend modCurrentUser = new Friend();
                                                modCurrentUser.bio = Globals.currentUser.bio;
                                                modCurrentUser.email = Globals.currentUser.email;
                                                modCurrentUser.username = Globals.currentUser.username;
                                                modCurrentUser.type = Globals.currentUser.userType;
                                                modCurrentUser.userId = Globals.currentUser.userId;
                                                modCurrentUser.ratings = Globals.currentUser.doctorRatings;
                                                modCurrentUser.userChatId = modFriend.userChatId;
                                                Globals.currentUser.userChatId = modCurrentUser.userChatId;
                                                string currentUserJson = JsonUtility.ToJson(modCurrentUser);
                                                FirebaseDatabase.DefaultInstance.RootReference
                                                .Child("users")
                                                .Child(friend.userId)
                                                .Child("friends")
                                                .Child(Globals.userId).SetRawJsonValueAsync(currentUserJson).ContinueWith(uTask =>
                                                {
                                                    if (uTask.IsCompleted)      // Saved the chat id for the current user
                                                    {
                                                        // Add Chat Panel and get the user channel id
                                                        openChatPanel = true;
                                                        openedChatOtherUserId = Globals.currentUser.userChatId;
                                                    }
                                                    Globals.showLoadingPanel = false;
                                                });
                                            }
                                        });
                                    }
                                    else
                                    {
                                        // This is not the first chat between the two users 
                                        Globals.currentUser.userChatId = snapshot.Child(Globals.userId).Child("userChatId").Value.ToString();
                                        // Open Chat for this chat id and Add Chat Panel and get the user channel id
                                        openChatPanel = true;
                                        openedChatOtherUserId = Globals.currentUser.userChatId;
                                        Globals.showLoadingPanel = false;
                                    }
                                }
                                Globals.showLoadingPanel = false;
                            });
                        });
                    }
                }
            }
            Debug.Log("Buttons Count: " + (profilePanel.GetComponentsInChildren<Button>().Length - 1));
            Button closeProfileBtn = profilePanel.GetComponentsInChildren<Button>()[profilePanel.GetComponentsInChildren<Button>().Length - 1];
            closeProfileBtn.onClick.AddListener(delegate
            {
                Debug.Log("Terminating profile panel ... ");
                Destroy(profilePanel.gameObject);
            });
        }
    }

    void Update()
    {
        if (friendAdded)
        {
            friendAdded = false;
            Destroy(this.gameObject);
        }
        if (destroyAddReviewPanel)
        {
            destroyAddReviewPanel = false;
            Destroy(addReviewPanel.gameObject);
            MessageBoxMan.Open("Please reload friends list to see changes ... ");
        }
    }
}
