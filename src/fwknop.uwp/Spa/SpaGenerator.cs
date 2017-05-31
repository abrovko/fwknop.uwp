using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace fwknop.uwp
{
    class SpaGenerator
    {
        readonly string EncryptionKeyBase64;
        readonly string HmakKeyBase64;
        public SpaGenerator(string encryptionKeyBase64, string hmakKeyBase64)
        {
            this.EncryptionKeyBase64 = encryptionKeyBase64;
            this.HmakKeyBase64 = hmakKeyBase64;
        }
        public byte[] CreateSpaPacket(string accessPortList, string allowIp, string optionalNatAccess = null)
        {
            string encodedPacketData = CreateEncodedPacket(accessPortList, allowIp, optionalNatAccess);
            string encryptedPacket = EncryptPacket(encodedPacketData);
            string signedPacket = SignPacket(encryptedPacket);
            return Encoding.UTF8.GetBytes(signedPacket); 
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodedPacketData"></param>
        /// <returns>Base64 encoded encrypted message</returns>
        string EncryptPacket(string encodedPacketData)
        {
            byte[] derivedKey, derivedIV, salt;
            RijSaltAndIv(out derivedKey, out derivedIV, out salt);
            SymmetricAlgorithm enc = Aes.Create();
            enc.Key = derivedKey;
            enc.IV = derivedIV;
            enc.Mode = CipherMode.CBC;
            var ms = new MemoryStream();
            ms.Write(Encoding.UTF8.GetBytes("Salted__"), 0, 8);
            ms.Write(salt, 0, salt.Length);
            using (var encryptor = enc.CreateEncryptor())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                var encodedPacketDataBytes = Encoding.UTF8.GetBytes(encodedPacketData);
                cs.Write(encodedPacketDataBytes, 0, encodedPacketDataBytes.Length);
            }

            var encryptedMessage = ms.ToArray();
            return ToBase64TrimPadding(encryptedMessage);
        }

        string SignPacket(string encryptedPacket)
        {
            HMACSHA256 hm = new HMACSHA256(Convert.FromBase64String(HmakKeyBase64));
            var digestBase64 = ToBase64TrimPadding(hm.ComputeHash(Encoding.UTF8.GetBytes(encryptedPacket)));

            //substring removes the base64 encoded "Salted__" prefix as per fwknop specs
            string signedPacket = encryptedPacket.Substring(10) + digestBase64;
            return signedPacket;

        }

        string CreateEncodedPacket(string accessPortList, string allowIp, string optionalNatAccess = null)
        {
            StringBuilder spaData = new StringBuilder();

            //16 bytes random
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] random = new byte[16];
            rng.GetBytes(random);
            var r = BitConverter.ToUInt64(random, 0).ToString().PadLeft(16, '0');
            r = r.Substring(r.Length - 16, 16);
            spaData.Append(r);

            //username
            var username = "someuser"; //not used?
            spaData.Append(":").Append(ToBase64TrimPadding(Encoding.UTF8.GetBytes(username)));

            //timestamp
            uint unixTimestamp = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            spaData.Append(":").Append(unixTimestamp);

            //protocol version
            spaData.Append(":").Append("2.0.1");

            //Message type
            spaData.Append(":").Append(string.IsNullOrWhiteSpace(optionalNatAccess) ? "1" : "2");

            //SPA message
            spaData.Append(":").Append(ToBase64TrimPadding(Encoding.UTF8.GetBytes($"{allowIp},{accessPortList}")));

            //Nat Access
            if (!string.IsNullOrWhiteSpace(optionalNatAccess))
                spaData.Append(":").Append(ToBase64TrimPadding(Encoding.UTF8.GetBytes(optionalNatAccess)));

            //Server auth?

            //SPA Data Digest
            SHA256 hash = SHA256.Create();
            var md5Hash = hash.ComputeHash(Encoding.UTF8.GetBytes(spaData.ToString()));
            spaData.Append(":").Append(ToBase64TrimPadding(md5Hash));

            return spaData.ToString();
        }

        const int MD5_DIGEST_LEN = 16;
        const int SALT_LEN = 8;
        const int RIJNDAEL_MAX_KEYSIZE = 32;
        const int RIJNDAEL_BLOCKSIZE = 16;

        /// <summary>
        /// Magic. Almost line to line translation from fwknop c code
        /// cipher_funcs.c: rij_salt_and_iv
        /// Used fwknop-2.6.8
        /// </summary>
        /// <param name="generatedKey"></param>
        /// <param name="generatedIv"></param>
        /// <param name="salt"></param>
        private void RijSaltAndIv(out byte[] generatedKey, out byte[] generatedIv, out byte[] salt)
        {
            byte[] pw_buf = new byte[RIJNDAEL_MAX_KEYSIZE];
            byte[] tmp_buf = new byte[MD5_DIGEST_LEN + RIJNDAEL_MAX_KEYSIZE + RIJNDAEL_BLOCKSIZE];
            byte[] kiv_buf = new byte[RIJNDAEL_MAX_KEYSIZE + RIJNDAEL_BLOCKSIZE];
            byte[] md5_buf = new byte[MD5_DIGEST_LEN];

            int final_key_len = 0;
            int kiv_len = 0;

            var key = Convert.FromBase64String(EncryptionKeyBase64);

            //memcpy(pw_buf, key, key_len);
            Array.Copy(key, pw_buf, key.Length);
            final_key_len = key.Length;

            salt = new byte[SALT_LEN];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            // memcpy(tmp_buf + MD5_DIGEST_LEN, pw_buf, final_key_len);
            Array.Copy(pw_buf, 0, tmp_buf, MD5_DIGEST_LEN, final_key_len);
            //memcpy(tmp_buf + MD5_DIGEST_LEN + final_key_len, ctx->salt, SALT_LEN);
            Array.Copy(salt, 0, tmp_buf, MD5_DIGEST_LEN + final_key_len, SALT_LEN);

            MD5 md5 = MD5.Create();
            while (kiv_len < kiv_buf.Length)
            {
                if (kiv_len == 0)
                    md5_buf = md5.ComputeHash(tmp_buf, MD5_DIGEST_LEN, final_key_len + SALT_LEN);
                //md5(md5_buf, tmp_buf + MD5_DIGEST_LEN, final_key_len + SALT_LEN);
                else
                    md5_buf = md5.ComputeHash(tmp_buf, 0, MD5_DIGEST_LEN + final_key_len + SALT_LEN);
                //md5(md5_buf, tmp_buf, MD5_DIGEST_LEN + final_key_len + SALT_LEN);

                Array.Copy(md5_buf, tmp_buf, MD5_DIGEST_LEN);
                //memcpy(tmp_buf, md5_buf, MD5_DIGEST_LEN);

                Array.Copy(md5_buf, 0, kiv_buf, kiv_len, MD5_DIGEST_LEN);
                //memcpy(kiv_buf + kiv_len, md5_buf, MD5_DIGEST_LEN);

                kiv_len += MD5_DIGEST_LEN;
            }

            generatedKey = new byte[RIJNDAEL_MAX_KEYSIZE];
            Array.Copy(kiv_buf, generatedKey, RIJNDAEL_MAX_KEYSIZE);

            generatedIv = new byte[RIJNDAEL_BLOCKSIZE];
            Array.Copy(kiv_buf, RIJNDAEL_MAX_KEYSIZE, generatedIv, 0, RIJNDAEL_BLOCKSIZE);
        }

        string ToBase64TrimPadding(byte[] buf)
        {
            return Convert.ToBase64String(buf).TrimEnd('=');
        }
    }
}
