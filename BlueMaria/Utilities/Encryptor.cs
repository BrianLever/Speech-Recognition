using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace NSDK
{
	/// <summary>
	/// This is a wrapper for symmetric key algorithm (Rijndael/AES). 
	/// Encryption and decryption should use the same parameters to generate the keys in order to have matching
	/// plain and cypher strings.
	/// </summary>
	public sealed class Encryptor
	{
		public static readonly Encryptor I = new Encryptor();
		private Encryptor(){}

		string   _passPhrase         = "Pas5pr@se";        // can be any string
		string   _saltValue          = "s@1tValue";        // can be any string
		string   _hashAlgorithm      = "SHA1";             // can be "MD5"
		int      _passwordIterations = 2;                  // can be any number
		string   _initVector         = "@1B2c3D4e5F6g7H8"; // must be 16 bytes
		int      _keySize            = 256;                // can be 192 or 128

		public string   PassPhrase {get{return _passPhrase;}set{_passPhrase=value;} }
		public string   SaltValue {get{return _saltValue;}set{_saltValue=value;} }
		public string   HashAlgorithm {get{return _hashAlgorithm;}set{_hashAlgorithm=value;} }
		public int      PasswordIterations {get{return _passwordIterations;}set{_passwordIterations=value;} }
		public string   InitVector {get{return _initVector;}set{_initVector=value;} }
		public int      KeySize {get{return _keySize;}set{_keySize=value;} }

		public string Encrypt(string   plainText )
		{
			return this.Encrypt(
				plainText,
				_passPhrase,
				_saltValue,
				_hashAlgorithm,
				_passwordIterations,
				_initVector,
				_keySize );
		}

		/// <summary>
		/// Encrypts specified plaintext using Rijndael symmetric key algorithm
		/// and returns a base64-encoded result.
		/// </summary>
		/// <param name="plainText">
		/// Plaintext value to be encrypted.
		/// </param>
		/// <param name="passPhrase">
		/// Passphrase from which a pseudo-random password will be derived. The
		/// derived password will be used to generate the encryption key.
		/// Passphrase can be any string. In this example we assume that this
		/// passphrase is an ASCII string.
		/// </param>
		/// <param name="saltValue">
		/// Salt value used along with passphrase to generate password. Salt can
		/// be any string. In this example we assume that salt is an ASCII string.
		/// </param>
		/// <param name="hashAlgorithm">
		/// Hash algorithm used to generate password. Allowed values are: "MD5" and
		/// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
		/// </param>
		/// <param name="passwordIterations">
		/// Number of iterations used to generate password. One or two iterations
		/// should be enough.
		/// </param>
		/// <param name="initVector">
		/// Initialization vector (or IV). This value is required to encrypt the
		/// first block of plaintext data. For RijndaelManaged class IV must be 
		/// exactly 16 ASCII characters long.
		/// </param>
		/// <param name="keySize">
		/// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
		/// Longer keys are more secure than shorter keys.
		/// </param>
		/// <returns>
		/// Encrypted value formatted as a base64-encoded string.
		/// </returns>
		public string Encrypt(string   plainText,
			string   passPhrase,
			string   saltValue,
			string   hashAlgorithm,
			int      passwordIterations,
			string   initVector,
			int      keySize)
		{
			// Convert strings into byte arrays.
			// Let us assume that strings only contain ASCII codes.
			// If strings include Unicode characters, use Unicode, UTF7, or UTF8 
			// encoding.
			byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
			byte[] saltValueBytes  = Encoding.ASCII.GetBytes(saltValue);
        
			// Convert our plaintext into a byte array.
			// Let us assume that plaintext contains UTF8-encoded characters.
			byte[] plainTextBytes  = Encoding.UTF8.GetBytes(plainText);
        
			// First, we must create a password, from which the key will be derived.
			// This password will be generated from the specified passphrase and 
			// salt value. The password will be created using the specified hash 
			// algorithm. Password creation can be done in several iterations.
			PasswordDeriveBytes password = new PasswordDeriveBytes(
				passPhrase, 
				saltValueBytes, 
				hashAlgorithm, 
				passwordIterations);
        
			// Use the password to generate pseudo-random bytes for the encryption
			// key. Specify the size of the key in bytes (instead of bits).
			byte[] keyBytes = password.GetBytes(keySize / 8);
        
			// Create uninitialized Rijndael encryption object.
			RijndaelManaged symmetricKey = new RijndaelManaged();
        
			// It is reasonable to set encryption mode to Cipher Block Chaining
			// (CBC). Use default options for other symmetric key parameters.
			symmetricKey.Mode = CipherMode.CBC;        
        
			// Generate encryptor from the existing key bytes and initialization 
			// vector. Key size will be defined based on the number of the key 
			// bytes.
			ICryptoTransform encryptor = symmetricKey.CreateEncryptor(
				keyBytes, 
				initVectorBytes);
        
			// Define memory stream which will be used to hold encrypted data.
			MemoryStream memoryStream = new MemoryStream();        
                
			// Define cryptographic stream (always use Write mode for encryption).
			CryptoStream cryptoStream = new CryptoStream(memoryStream, 
				encryptor,
				CryptoStreamMode.Write);
			// Start encrypting.
			cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                
			// Finish encrypting.
			cryptoStream.FlushFinalBlock();

			// Convert our encrypted data from a memory stream into a byte array.
			byte[] cipherTextBytes = memoryStream.ToArray();
                
			// Close both streams.
			memoryStream.Close();
			cryptoStream.Close();
        
			// Convert encrypted data into a base64-encoded string.
			string cipherText = Convert.ToBase64String(cipherTextBytes);
        
			// Return encrypted string.
			return cipherText;
		}

		public string Decrypt( string   cipherText )
		{
			return this.Decrypt(
				cipherText,
				_passPhrase,
				_saltValue,
				_hashAlgorithm,
				_passwordIterations,
				_initVector,
				_keySize );
		}
		/// <summary>
		/// Decrypts specified ciphertext using Rijndael symmetric key algorithm.
		/// </summary>
		/// <param name="cipherText">
		/// Base64-formatted ciphertext value.
		/// </param>
		/// <param name="passPhrase">
		/// Passphrase from which a pseudo-random password will be derived. The
		/// derived password will be used to generate the encryption key.
		/// Passphrase can be any string. In this example we assume that this
		/// passphrase is an ASCII string.
		/// </param>
		/// <param name="saltValue">
		/// Salt value used along with passphrase to generate password. Salt can
		/// be any string. In this example we assume that salt is an ASCII string.
		/// </param>
		/// <param name="hashAlgorithm">
		/// Hash algorithm used to generate password. Allowed values are: "MD5" and
		/// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
		/// </param>
		/// <param name="passwordIterations">
		/// Number of iterations used to generate password. One or two iterations
		/// should be enough.
		/// </param>
		/// <param name="initVector">
		/// Initialization vector (or IV). This value is required to encrypt the
		/// first block of plaintext data. For RijndaelManaged class IV must be
		/// exactly 16 ASCII characters long.
		/// </param>
		/// <param name="keySize">
		/// Size of encryption key in bits. Allowed values are: 128, 192, and 256.
		/// Longer keys are more secure than shorter keys.
		/// </param>
		/// <returns>
		/// Decrypted string value.
		/// </returns>
		/// <remarks>
		/// Most of the logic in this function is similar to the Encrypt
		/// logic. In order for decryption to work, all parameters of this function
		/// - except cipherText value - must match the corresponding parameters of
		/// the Encrypt function which was called to generate the
		/// ciphertext.
		/// </remarks>
		public string Decrypt(string   cipherText,
			string   passPhrase,
			string   saltValue,
			string   hashAlgorithm,
			int      passwordIterations,
			string   initVector,
			int      keySize)
		{
			// Convert strings defining encryption key characteristics into byte
			// arrays. Let us assume that strings only contain ASCII codes.
			// If strings include Unicode characters, use Unicode, UTF7, or UTF8
			// encoding.
			byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
			byte[] saltValueBytes  = Encoding.ASCII.GetBytes(saltValue);
        
			// Convert our ciphertext into a byte array.
			byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        
			// First, we must create a password, from which the key will be 
			// derived. This password will be generated from the specified 
			// passphrase and salt value. The password will be created using
			// the specified hash algorithm. Password creation can be done in
			// several iterations.
			PasswordDeriveBytes password = new PasswordDeriveBytes(
				passPhrase, 
				saltValueBytes, 
				hashAlgorithm, 
				passwordIterations);
        
			// Use the password to generate pseudo-random bytes for the encryption
			// key. Specify the size of the key in bytes (instead of bits).
			byte[] keyBytes = password.GetBytes(keySize / 8);
        
			// Create uninitialized Rijndael encryption object.
			RijndaelManaged    symmetricKey = new RijndaelManaged();
        
			// It is reasonable to set encryption mode to Cipher Block Chaining
			// (CBC). Use default options for other symmetric key parameters.
			symmetricKey.Mode = CipherMode.CBC;
        
			// Generate decryptor from the existing key bytes and initialization 
			// vector. Key size will be defined based on the number of the key 
			// bytes.
			ICryptoTransform decryptor = symmetricKey.CreateDecryptor(
				keyBytes, 
				initVectorBytes);
        
			// Define memory stream which will be used to hold encrypted data.
			MemoryStream  memoryStream = new MemoryStream(cipherTextBytes);
                
			// Define cryptographic stream (always use Read mode for encryption).
			CryptoStream  cryptoStream = new CryptoStream(memoryStream, 
				decryptor,
				CryptoStreamMode.Read);

			// Since at this point we don't know what the size of decrypted data
			// will be, allocate the buffer long enough to hold ciphertext;
			// plaintext is never longer than ciphertext.
			byte[] plainTextBytes = new byte[cipherTextBytes.Length];
        
			// Start decrypting.
			int decryptedByteCount = cryptoStream.Read(plainTextBytes, 
				0, 
				plainTextBytes.Length);
                
			// Close both streams.
			memoryStream.Close();
			cryptoStream.Close();
        
			// Convert decrypted data into a string. 
			// Let us assume that the original plaintext string was UTF8-encoded.
			string plainText = Encoding.UTF8.GetString(plainTextBytes, 
				0, 
				decryptedByteCount);
        
			// Return decrypted string.   
			return plainText;
		}

		public static byte[] EncryptPassword( string password )
		{
			//			MD5 md5 = new MD5CryptoServiceProvider();
			//			md5.ComputeHash(
			UnicodeEncoding encoding = new UnicodeEncoding();
			byte[] hashBytes = encoding.GetBytes(password);
			SHA1 sha1 = new SHA1CryptoServiceProvider();
			byte[] bytes = sha1.ComputeHash( hashBytes );
			return bytes;
		}

		// Create an md5 sum string of this string
		static public string GetMd5Sum(string str)
		{
			System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
			System.Security.Cryptography.MD5CryptoServiceProvider hashProvider = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] bytes = hashProvider.ComputeHash(UTF8.GetBytes(str));
			// Build the final string by converting each byte
			// into hex and appending it to a StringBuilder
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				sb.Append(bytes[i].ToString("X2"));
			}
			// And return it
			return sb.ToString();

			//// First we need to convert the string into bytes, which
			//// means using a text encoder.
			//Encoder enc = System.Text.Encoding.Unicode.GetEncoder();

			//// Create a buffer large enough to hold the string
			//byte[] unicodeText = new byte[str.Length * 2];
			//enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

			//// Now that we have a byte array we can ask the CSP to hash it
			//MD5 md5 = new MD5CryptoServiceProvider();
			//byte[] result = md5.ComputeHash(unicodeText);

			//// Build the final string by converting each byte
			//// into hex and appending it to a StringBuilder
			//StringBuilder sb = new StringBuilder();
			//for (int i = 0; i < result.Length; i++)
			//{
			//    sb.Append(result[i].ToString("X2"));
			//}

			//// And return it
			//return sb.ToString();
		}

	}
}
