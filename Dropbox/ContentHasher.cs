using System;
using System.Security.Cryptography;

namespace Dropbox;

public class DropboxContentHasher(SHA256 overallHasher, SHA256 blockHasher, int blockPos) : HashAlgorithm
{
    private readonly SHA256 _overallHasher = overallHasher;
    private readonly SHA256 _blockHasher = blockHasher;
    private int _blockPos = blockPos;

    public const int BLOCK_SIZE = 4 * 1024 * 1024;

    public DropboxContentHasher() : this(SHA256.Create(), SHA256.Create(), 0) { }

    public override int HashSize { get { return _overallHasher.HashSize; } }

    protected override void HashCore(byte[] input, int offset, int len)
    {
        int inputEnd = offset + len;
        while (offset < inputEnd)
        {
            if (_blockPos == BLOCK_SIZE)
            {
                FinishBlock();
            }

            int spaceInBlock = BLOCK_SIZE - this._blockPos;
            int inputPartEnd = Math.Min(inputEnd, offset + spaceInBlock);
            int inputPartLength = inputPartEnd - offset;
            _blockHasher.TransformBlock(input, offset, inputPartLength, input, offset);

            _blockPos += inputPartLength;
            offset += inputPartLength;
        }
    }

    protected override byte[] HashFinal()
    {
        if (_blockPos > 0)
        {
            FinishBlock();
        }
        _overallHasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        if (_overallHasher.Hash == null) throw new Exception("Null overall Hash");
        return _overallHasher.Hash;
    }

    public override void Initialize()
    {
        _blockHasher.Initialize();
        _overallHasher.Initialize();
        _blockPos = 0;
    }

    private void FinishBlock()
    {
        _blockHasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        if (_blockHasher.Hash == null) throw new Exception("Null block Hash");
        byte[] blockHash = _blockHasher.Hash;
        _blockHasher.Initialize();

        _overallHasher.TransformBlock(blockHash, 0, blockHash.Length, blockHash, 0);
        _blockPos = 0;
    }

    private const string HEX_DIGITS = "0123456789abcdef";

    public static string ToHex(byte[] data)
    {
        var r = new System.Text.StringBuilder();
        foreach (byte b in data)
        {
            r.Append(HEX_DIGITS[(b >> 4)]);
            r.Append(HEX_DIGITS[(b & 0xF)]);
        }
        return r.ToString();
    }
}