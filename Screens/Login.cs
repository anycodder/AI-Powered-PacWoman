using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button goToRegisterButton;
    public GameObject panel3;
    public static string LoggedInUsername { get; private set; }
    private List<string> credentials;

    private string filePath;
    private readonly string encryptionKey = "12345678901234567890123456789012"; // 32-byte key

    private void Start()
    {
        filePath = Application.persistentDataPath + "/credentials.txt";

        panel3.SetActive(false);
        loginButton.onClick.AddListener(LoginUser);
        goToRegisterButton.onClick.AddListener(MoveToRegister);

        usernameInput.onSelect.AddListener(delegate { ResetInputFieldColor(usernameInput); });
        passwordInput.onSelect.AddListener(delegate { ResetInputFieldColor(passwordInput); });

        // Set password input field to use asterisks
        passwordInput.contentType = TMP_InputField.ContentType.Password;

        InitializeCredentials();
    }

    void InitializeCredentials()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string encryptedText = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(encryptedText))
                {
                    string decryptedText = Decrypt(encryptedText);
                    if (!string.IsNullOrEmpty(decryptedText))
                    {
                        credentials = new List<string>(decryptedText.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
                    }
                    else
                    {
                        credentials = new List<string>();
                        File.WriteAllText(filePath, Encrypt(""));
                    }
                }
                else
                {
                    credentials = new List<string>();
                    File.WriteAllText(filePath, Encrypt(""));
                }
            }
            else
            {
                Debug.Log("Credential file doesn't exist");
                credentials = new List<string>();
                File.WriteAllText(filePath, Encrypt(""));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error reading credentials: " + ex.Message);
            credentials = new List<string>();
            File.WriteAllText(filePath, Encrypt(""));
        }
    }

    private void LoginUser()
    {
        bool isExists = false;

        foreach (var i in credentials)
        {
            string line = i.ToString();
            int indexOfColon = line.IndexOf(":");
            if (indexOfColon != -1 && line.Substring(0, indexOfColon).Equals(usernameInput.text) &&
                line.Substring(indexOfColon + 1).Equals(passwordInput.text))
            {
                isExists = true;
                break;
            }
        }

        if (isExists)
        {
            Debug.Log($"Logging in '{usernameInput.text}'");
            LoggedInUsername = usernameInput.text;
            SceneManager.LoadScene("Scenes/MainMenu");
            Debug.Log("Scene loaded");
        }
        else
        {
            Debug.Log("Incorrect credentials");
            panel3.SetActive(true);
            usernameInput.image.color = Color.red;
            passwordInput.image.color = Color.red;
            StartCoroutine(ResetInputFieldColorsAfterDelay(2f));
        }
    }

    private void MoveToRegister()
    {
        SceneManager.LoadScene("Register");
    }

    private void ResetInputFieldColor(TMP_InputField inputField)
    {
        inputField.image.color = Color.white;
    }

    private void ResetInputFieldColors()
    {
        usernameInput.image.color = Color.white;
        passwordInput.image.color = Color.white;

        usernameInput.interactable = true;
        passwordInput.interactable = true;
    }

    private IEnumerator ResetInputFieldColorsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetInputFieldColors();
        panel3.SetActive(false);
    }

    private string Encrypt(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = new byte[16];
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(string.IsNullOrEmpty(plainText) ? " " : plainText);
                    }
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string Decrypt(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32));
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = new byte[16];
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd().TrimEnd('\0');
                        }
                    }
                }
            }
        }
        catch (CryptographicException ce)
        {
            Debug.LogError("Cryptographic error: " + ce.Message);
            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error decrypting data: " + ex.Message);
            return string.Empty;
        }
    }
}
