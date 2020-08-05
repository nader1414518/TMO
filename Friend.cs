using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable]
public class Friend : MonoBehaviour
{
    public string username = "";
    public string email = "";
    public string userId = "";
    public string bio = "";
    public string type = "";
    public List<DoctorRating> ratings = new List<DoctorRating>();
    public string userChatId = "";

    public Friend()
    {
        username = "";
        email = "";
        userId = "";
        bio = "";
        type = "";
        userChatId = "";
    }

    public Friend(string username, string email, string userId, string bio, string type)
    {
        this.username = username;
        this.email = email;
        this.userId = userId;
        this.bio = bio;
        this.type = type;
        this.userChatId = "";
    }
}
