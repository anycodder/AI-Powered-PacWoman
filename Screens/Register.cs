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

public class Register : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button registerButton;
    public Button goToLoginButton;
    public GameObject panel;
    public GameObject panel2;
    public GameObject panel3;
    private List<string> credentials;

    private string filePath;
    private readonly string encryptionKey = "12345678901234567890123456789012"; // 32-byte key

    void Start()
    {
        filePath = Application.persistentDataPath + "/credentials.txt";

        panel.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);

        usernameInput.onSelect.AddListener(delegate { ResetInputFieldColor(usernameInput); });
        passwordInput.onSelect.AddListener(delegate { ResetInputFieldColor(passwordInput); });

        // Set password input field to use asterisks
        passwordInput.contentType = TMP_InputField.ContentType.Password;

        registerButton.onClick.AddListener(writeStuffToFile);
        goToLoginButton.onClick.AddListener(goToLoginScene);

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

    void goToLoginScene()
    {
        SceneManager.LoadScene("Login");
    }

    void writeStuffToFile()
    {
        ResetInputFieldColors();
        bool isExists = false;

        if (passwordInput.text.Length < 6)
        {
            Debug.Log("Password must be at least 6 characters long");
            panel3.SetActive(true);
            panel2.SetActive(false);
            panel.SetActive(false);
            usernameInput.image.color = Color.red;
            passwordInput.image.color = Color.red;
            StartCoroutine(ResetInputFieldColorsAfterDelay(2f));
            return;
        }

        foreach (var i in credentials)
        {
            if (i.Contains(usernameInput.text))
            {
                isExists = true;
                break;
            }
        }

        if (isExists)
        {
            Debug.Log($"Username '{usernameInput.text}' already exists");
            panel2.SetActive(false);
            panel.SetActive(true);
            panel3.SetActive(false);
            usernameInput.image.color = Color.red;
            passwordInput.image.color = Color.red;
        }
        else
        {
            credentials.Add(usernameInput.text + ":" + passwordInput.text);
            panel.SetActive(false);
            panel2.SetActive(true);
            panel3.SetActive(false);
            File.WriteAllText(filePath, Encrypt(string.Join(Environment.NewLine, credentials)));
            Debug.Log("Account Registered");
        }
    }

    private void ResetInputFieldColor(TMP_InputField inputField)
    {
        usernameInput.image.color = Color.white;
        passwordInput.image.color = Color.white;
        usernameInput.interactable = true;
        passwordInput.interactable = true;
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
