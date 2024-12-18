﻿namespace GameManager.Modules.SecurityProvider
{
    public interface ISecurityProvider
    {
        public Task<string> EncryptAsync(string plainText);

        public Task<string> DecryptAsync(string encryptedText);
    }
}