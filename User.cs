using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class User : MonoBehaviour
{
    public string bio;
    public string userChatId;
    public string email;
    public string username;
    public string userId;
    public string userColorMode;
    public string userType;
    public string isFirstTime;
    public string userPhoneNumber;
    public string userAddress;
    public string userWeight;
    public string userPreviousDiseases;
    public string userCurrentDiseases;
    public string userCurrentDrugs;
    public string userPreviousOperations;
    public string userWearingGlasses;
    public string userFamilyChronicDiseases;
    public string userSmoking;
    public string userDrinksAlcohols;
    public string[] userCerts = new string[3];
    public List<Friend> friends = new List<Friend>();
    public List<DoctorRating> doctorRatings = new List<DoctorRating>();


    public User()
    {
        this.email = "";
        this.username = "";
        this.userId = "";
        this.userColorMode = "";
        this.isFirstTime = "";
        this.userType = "";
        bio = "";
        userChatId = "";
    }

    public User(string email, string username, string userId)
    {
        this.email = email;
        this.username = username;
        this.userId = userId;
    }
    public User(string email, string username, string userId, string userColorMode)
    {
        this.email = email;
        this.username = username;
        this.userId = userId;
        this.userColorMode = userColorMode;
    }
}
