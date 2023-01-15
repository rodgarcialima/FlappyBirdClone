using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text scoreText;
    public GameObject playButton;
    public GameObject gameOver;
    public Player player;
    public GameObject leaderBoard;
    public Text lbPlayers;
    public Text lbScores;
    public Text playerID;

    private int score;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        // authentication        
        SetupAuthenticationEvents();
        SignInAnonymouslyAsync();

        // force app to 60 fps max
        Application.targetFrameRate = 60;
        gameOver.SetActive(false);

        Pause();
    }

    async void Start()
    {
        await ShowLeaderboard();
    }

    private void SetupAuthenticationEvents()
    {
        AuthenticationService.Instance.SignedIn += SignedIn;
        AuthenticationService.Instance.SignInFailed += SignInFailed;
        AuthenticationService.Instance.SignedOut += SignedOut;
        AuthenticationService.Instance.Expired += Expired;
    }

    private async void SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            playerID.text = AuthenticationService.Instance.PlayerId;

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    private void SignedIn()
    {
        // Shows how to get a playerID
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        // Shows how to get an access token
        Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
    }

    private void SignInFailed(RequestFailedException err)
    {
        Debug.LogError(err);
    }

    private void SignedOut()
    {
        Debug.Log("Player signed out.");
    }

    private void Expired()
    {
        Debug.Log("Player session could not be refreshed and expired.");
    }

    public void Play()
    {
        score = 0;
        scoreText.text = score.ToString();

        playButton.SetActive(false);
        gameOver.SetActive(false);
        HideLeaderboard();

        Time.timeScale = 1f;
        player.enabled = true;

        Pipes[] pipes = FindObjectsOfType<Pipes>();
        for (int i = 0; i < pipes.Length; i++)
        {
            Destroy(pipes[i].gameObject);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        player.enabled = false;
    }

    public void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }

    public async void GameOver()
    {
        gameOver.SetActive(true);
        playButton.SetActive(true);

        Pause();

        await UpdateLeaderboard();
        await ShowLeaderboard();
    }

    private async Task UpdateLeaderboard()
    {
        try
        {
            var args = new Dictionary<string, object> { { "score", score } };
            await CloudCodeService.Instance.CallEndpointAsync("register-score", args);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private async Task ShowLeaderboard()
    {
        try
        {
            // clear leaderboard
            lbPlayers.text = "Loading...";
            lbScores.text = "";

            // get data
            var args = new Dictionary<string, object>();
            var leaderboard = await CloudCodeService.Instance.CallEndpointAsync<Leaderboard>("get-leaderboard", args);
            Debug.Log($"LB = {leaderboard}");

            // display data
            string p = "", s = "";
            for (int i = 0; i < leaderboard.players.Length; i++)
            {
                if (leaderboard.players[i] == AuthenticationService.Instance.PlayerId)
                {
                    p += "YOU" + Environment.NewLine;
                } else
                {
                    p += Truncate(leaderboard.players[i], 16) + Environment.NewLine;
                }
                
                s += leaderboard.scores[i] + Environment.NewLine;
            }
            lbPlayers.text = p;
            lbScores.text = s;

            leaderBoard.SetActive(true);
        } 
        catch (Exception ex)
        {
            HideLeaderboard();
            Debug.LogError(ex);
        }
    }

    private void HideLeaderboard()
    {
        leaderBoard.SetActive(false);
    }

    class Leaderboard
    {
        public string[] players;
        public int[] scores;
    }

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}
