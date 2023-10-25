using System;
using System.Linq;
using System.Numerics;
using Streebog_Hash.Data;

namespace Streebog_Hash.Hash
{
    internal class Streebog
    {
        // Исключающее ИЛИ двух байт
        private byte[] Xor(byte[] a, byte[] b)
        {
            byte[] c = new byte[64];

            for (int i = 0; i < 64; i++)
                c[i] = (byte)(a[i] ^ b[i]);

            return c;
        }

        // Нелинейное преобразование (S)
        private byte[] STrans(byte[] a)
        {
            byte[] b = new byte[64];

            for (int i = 0; i < 64; i++)
                b[i] = StreebogConstants.Pi[a[i]];

            return b;
        }

        // Переупорядочивание байт (P)
        private byte[] PTrans(byte[] a)
        {
            byte[] b = new byte[64];

            for (int i = 0; i < 64; i++)
                b[i] = a[StreebogConstants.Tau[i]];

            return b;
        }

        // Линейное преобразование (L)
        private byte[] LTrans(byte[] a)
        {
            const int BIT_MASK = 0x01;
            byte[] result = Array.Empty<byte>();

            for (int i = 0; i < 8; i++)
            {
                ulong V = BitConverter.ToUInt64(a, 8 * i);
                ulong t = 0;

                for (int j = 0; j < 64; j++)
                    if ((BIT_MASK & (V >> j)) == 1)
                        t ^= StreebogConstants.A[64 - 1 - j];

                result = result.Concat(BitConverter.GetBytes(t)).ToArray();
            }

            return result;
        }

        // Фукнция сжатия (g)
        private byte[] Compress(byte[] n, byte[] m, byte[] h)
        {
            byte[] state = LPSX(h, n);
            state = ETrans(state, m);
            state = Xor(h, state);

            return Xor(state, m);
        }

        // Преобразование E
        private byte[] ETrans(byte[] k, byte[] m)
        {
            byte[] state = m;

            for (int i = 0; i < 12; i++)
            {
                state = LPSX(state, k);
                k = LPSX(k, StreebogConstants.C[i].Reverse().ToArray());
            }

            return Xor(state, k);
        }

        // Основные преобразования X,S,P,L
        private byte[] LPSX(byte[] a, byte[] b)
        {
            byte[] transform = Xor(a, b);
            transform = LTrans(PTrans(STrans(transform)));

            return transform;
        }

        // Разбиение на блоки по 512 бит
        private void DivIntoBlocks(ref byte[] m, ref byte[] n, ref byte[] sigma, ref byte[] h)
        {
            while (m.Length >= 64)
            {
                byte[] block = m.Take(64).ToArray();
                h = Compress(n, block, h);

                n = AddMod512(n, ((BigInteger)512).ToByteArray());
                sigma = AddMod512(sigma, m);

                m = m.Skip(64).ToArray();
            }
        }

        // Дополнение до 512 бит
        private byte[] Padding(byte[] m)
        {           
            m = m.Append((byte)1).ToArray();
            m = m.Concat(new byte[64 - m.Length]).ToArray();

            return m;
        }

        // Сложение по модулю 2^512
        private byte[] AddMod512(byte[] a, byte[] b)
        {
            byte[] res = new byte[64];
            
            byte[] tmp = ((new BigInteger(a) + new BigInteger(b)) % BigInteger.Pow(2, 511)).ToByteArray();
            Array.Copy(tmp, 0, res, 0, tmp.Length);

            return res;
        }

        // Получение хэша 512 бит
        public string GetHash512(string text)
        {
            byte[] h = new byte[64];

            h = GetHash(Convert.FromHexString(text), h);
            h = h.Reverse().ToArray();

            string result = Convert.ToHexString(h);

            return result;
        }

        // Получение хэша 256 бит
        public string GetHash256(string text)
        {
            byte[] h = new byte[64];

            Array.Fill<byte>(h, 1);
            h = GetHash(Convert.FromHexString(text), h);
            
            h = h.Skip(32).ToArray();
            h = h.Reverse().ToArray();

            string result = Convert.ToHexString(h);

            return result;
        }

        // Вычисление хэша сообщения
        private byte[] GetHash(byte[] m, byte[] h)
        {
            byte[] n = new byte[64];
            byte[] sigma = new byte[64];

            m = m.Reverse().ToArray();

            DivIntoBlocks(ref m, ref n, ref sigma, ref h);
           
            int length = m.Length;
            m = Padding(m);
            h = Compress(n, m, h);

            n = AddMod512(n, ((BigInteger)length * 8).ToByteArray());
            sigma = AddMod512(sigma, m);

            h = Compress(new byte[64], n, h);
            h = Compress(new byte[64], sigma, h);

            return h;
        }
    }
}
