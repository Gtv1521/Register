using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace FrameworkDriver_Api.src.Utils
{
    public class Argon2Hasher
    {
        // Parámetros recomendados en 2026 (ajustables según tu app)
        private const int TimeCost = 4;           // Número de iteraciones (tiempo)
        private const int MemoryCost = 65536;     // 64 MiB de memoria (memoria-hard)
        private const int Parallelism = 4;        // Número de hilos paralelos
        private const int SaltSize = 16;          // 128 bits de sal
        private const int HashLength = 32;        // 256 bits de hash

        /// <summary>
        /// Genera un hash seguro de la contraseña usando Argon2id
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash en formato string (compatible con Verify)</returns>
        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

            // Genera sal aleatoria
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing, 
                Version = Argon2Version.Nineteen,
                TimeCost = TimeCost,
                MemoryCost = MemoryCost,
                Lanes = Parallelism,
                Threads = Parallelism,
                Salt = salt,
                HashLength = HashLength,
                Password = Encoding.UTF8.GetBytes(password)
            };

            return Argon2.Hash(config);
        }

        /// <summary>
        /// Verifica si la contraseña en texto plano coincide con el hash almacenado
        /// </summary>
        /// <param name="password">Contraseña a verificar</param>
        /// <param name="hashedPassword">Hash almacenado en la base de datos</param>
        /// <returns>true si coincide, false si no</returns>
        public static bool Verify(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            return Argon2.Verify(hashedPassword, password);
        }
    }
}