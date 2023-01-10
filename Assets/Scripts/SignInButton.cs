using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SignInButton : MonoBehaviour
{
    [SerializeField] GameObject buttonGameObj;

    void Update()
    {
        // hide sign in button when already authenticated
        buttonGameObj.SetActive(!SocialManager.Instance.IsAuthenticated());
    }

    public void SignInOut()
    {
        // apparently you are not supposed to provide a way to sign out,
        // so this actually just handles manual sign-in currently
        SocialManager.Instance.SignIn();
    }
}
